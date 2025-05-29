using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text.Json;
using UsuarioServicio.Dominio.Interfaces;

namespace UsuarioServicio.Infraestructura.Services
{
    public class KeycloakUserRegistrationService : IKeycloakService
    {
        private readonly HttpClient _httpClient;

        public KeycloakUserRegistrationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private async Task<string> ObtenerTokenAdminAsync(CancellationToken cancellationToken)
        {
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
            return tokenDoc.RootElement.GetProperty("access_token").GetString()!;
        }

        public async Task<string> RegistrarUsuarioAsync(string nombre, string apellido, string email, string password, CancellationToken cancellationToken)
        {
            var token = await ObtenerTokenAdminAsync(cancellationToken);

            var newUser = new
            {
                username = email,
                enabled = true,
                emailVerified = false,
                email,
                firstName = nombre,
                lastName = apellido,
                credentials = new[]
                {
                    new {
                        type = "password",
                        value = password,
                        temporary = false
                    }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:8081/admin/realms/microservicio-usuarios/users")
            {
                Content = new StringContent(JsonSerializer.Serialize(newUser), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new Exception("No se pudo crear el usuario en Keycloak.");

            var location = response.Headers.Location?.ToString();
            var userId = location?.Split('/').Last();
            if (string.IsNullOrEmpty(userId))
                throw new Exception("No se pudo obtener el ID del usuario.");

            return userId;
        }

        public async Task AsignarRolAsync(string keycloakUserId, string rolNombre, CancellationToken cancellationToken)
        {
            var token = await ObtenerTokenAdminAsync(cancellationToken);

            var rolesRequest = new HttpRequestMessage(HttpMethod.Get, "http://localhost:8081/admin/realms/microservicio-usuarios/roles");
            rolesRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var rolesResponse = await _httpClient.SendAsync(rolesRequest, cancellationToken);
            if (!rolesResponse.IsSuccessStatusCode)
                throw new Exception("Error obteniendo roles de Keycloak.");

            var rolesJson = await rolesResponse.Content.ReadAsStringAsync(cancellationToken);
            using var rolesDoc = JsonDocument.Parse(rolesJson);

            var keycloakRole = rolesDoc.RootElement
                .EnumerateArray()
                .FirstOrDefault(r => r.GetProperty("name").GetString() == rolNombre);

            if (keycloakRole.ValueKind == JsonValueKind.Undefined)
                throw new Exception($"El rol '{rolNombre}' no existe en Keycloak.");

            var rolePayload = new[] {
                new {
                    id = keycloakRole.GetProperty("id").GetString(),
                    name = keycloakRole.GetProperty("name").GetString()
                }
            };

            var assignRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"http://localhost:8081/admin/realms/microservicio-usuarios/users/{keycloakUserId}/role-mappings/realm")
            {
                Content = new StringContent(JsonSerializer.Serialize(rolePayload), Encoding.UTF8, "application/json")
            };
            assignRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var assignResponse = await _httpClient.SendAsync(assignRequest, cancellationToken);
            if (!assignResponse.IsSuccessStatusCode)
                throw new Exception("Error al asignar el rol en Keycloak.");
        }

        private readonly string _realm = "microservicio-usuarios";
        public async Task EnviarCorreoVerificacionAsync(string userId, CancellationToken cancellationToken)
        {
            var accessToken = await ObtenerTokenAdminAsync(cancellationToken);

            var request = new HttpRequestMessage(HttpMethod.Put,
                $"http://localhost:8081/admin/realms/{_realm}/users/{userId}/send-verify-email");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception($"No se pudo enviar el correo de verificación: {content}");
            }
        }

    }
}
