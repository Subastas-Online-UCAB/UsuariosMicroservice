using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MediatR;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace UsuarioServicio.Aplicacion.Services
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Guid>
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public RegisterUserHandler(ApplicationDbContext context)
        {
            _context = context;
            _httpClient = new HttpClient(); // Se puede inyectar para producción limpia
        }

        public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // 1. Validar rol en la base de datos
            var rol = await _context.Roles.FindAsync(request.RolId);
            if (rol == null)
                throw new Exception("El rol especificado no existe.");

            // 2. Validar que el correo no esté duplicado
            var existeUsuario = await _context.Usuarios
                .AnyAsync(u => u.Email == request.Email, cancellationToken);

            if (existeUsuario)
                throw new Exception("El correo electrónico ya está registrado.");

            // 3. Obtener token de administrador Keycloak
            var tokenResponse = await _httpClient.PostAsync(
                "http://localhost:8081/realms/master/protocol/openid-connect/token",
                new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("client_id", "admin-cli"),
                    new KeyValuePair<string, string>("username", "admin"),
                    new KeyValuePair<string, string>("password", "admin")
                }), cancellationToken);

            if (!tokenResponse.IsSuccessStatusCode)
                throw new Exception($"Error obteniendo token admin Keycloak: {tokenResponse.StatusCode}");

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
            using var tokenDoc = JsonDocument.Parse(tokenJson);
            var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();

            // 4. Crear usuario en Keycloak
            var newUser = new
            {
                username = request.Email,
                enabled = true,
                email = request.Email,
                firstName = request.Nombre,
                lastName = request.Apellido,
                credentials = new[]
                {
                    new {
                        type = "password",
                        value = request.Password,
                        temporary = false
                    }
                }
            };

            var createUserRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost:8081/admin/realms/microservicio-usuarios/users")
            {
                Content = new StringContent(JsonSerializer.Serialize(newUser), Encoding.UTF8, "application/json")
            };
            createUserRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var createUserResponse = await _httpClient.SendAsync(createUserRequest, cancellationToken);
            if (!createUserResponse.IsSuccessStatusCode)
                throw new Exception($"Error al crear el usuario en Keycloak: {createUserResponse.StatusCode}");

            // 5. Obtener userId desde Location header
            var userLocation = createUserResponse.Headers.Location?.ToString();
            var userId = userLocation?.Split('/').Last();

            if (string.IsNullOrEmpty(userId))
                throw new Exception("No se pudo obtener el ID del usuario creado en Keycloak.");

            // 6. Obtener roles disponibles en Keycloak
            var rolesRequest = new HttpRequestMessage(HttpMethod.Get, "http://localhost:8081/admin/realms/microservicio-usuarios/roles");
            rolesRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var rolesResponse = await _httpClient.SendAsync(rolesRequest, cancellationToken);
            if (!rolesResponse.IsSuccessStatusCode)
                throw new Exception($"Error al obtener roles de Keycloak: {rolesResponse.StatusCode}");

            var rolesJson = await rolesResponse.Content.ReadAsStringAsync(cancellationToken);
            using var rolesDoc = JsonDocument.Parse(rolesJson);

            var keycloakRole = rolesDoc.RootElement
                .EnumerateArray()
                .FirstOrDefault(r => r.GetProperty("name").GetString() == rol.Nombre);

            if (keycloakRole.ValueKind == JsonValueKind.Undefined)
                throw new Exception($"El rol '{rol.Nombre}' no existe en Keycloak.");

            var roleRepresentation = new[]
            {
                new
                {
                    id = keycloakRole.GetProperty("id").GetString(),
                    name = keycloakRole.GetProperty("name").GetString()
                }
            };

            // 7. Asignar rol al usuario en Keycloak
            var assignRoleRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"http://localhost:8081/admin/realms/microservicio-usuarios/users/{userId}/role-mappings/realm")
            {
                Content = new StringContent(JsonSerializer.Serialize(roleRepresentation), Encoding.UTF8, "application/json")
            };
            assignRoleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var assignRoleResponse = await _httpClient.SendAsync(assignRoleRequest, cancellationToken);
            if (!assignRoleResponse.IsSuccessStatusCode)
                throw new Exception($"Error al asignar el rol en Keycloak: {assignRoleResponse.StatusCode}");

            // 8. Crear usuario en la base de datos (con rollback si falla)
            var user = new Usuario
            {
                Id = Guid.NewGuid(),
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FechaCreacion = DateTime.UtcNow,
                Telefono = request.Telefono,
                Direccion = request.Direccion,
                RolId = request.RolId
            };

            try
            {
                _context.Usuarios.Add(user);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception dbEx)
            {
                // Rollback manual en Keycloak
                var deleteRequest = new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"http://localhost:8081/admin/realms/microservicio-usuarios/users/{userId}"
                );
                deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                await _httpClient.SendAsync(deleteRequest, cancellationToken);

                throw new Exception($"Error al guardar en la base de datos. Se eliminó el usuario en Keycloak. Detalle: {dbEx.Message}");
            }

            return user.Id;
        }
    }
}
