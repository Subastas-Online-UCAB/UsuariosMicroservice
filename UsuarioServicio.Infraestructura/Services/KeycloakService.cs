using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Http.Headers;
using System.Text.Json;
using Keycloak.Net.Models.RealmsAdmin;
using UsuarioServicio.Dominio.Interfaces;

namespace UsuarioServicio.Infraestructura.Services
{
    public class KeycloakService : IKeycloakAccountService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://localhost:8081"; // o la URL de tu Keycloak
        private readonly string _realm = "microservicio-usuarios";
        public KeycloakService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string> GetAdminToken(CancellationToken cancellationToken)
        {
            var response = await _httpClient.PostAsync(
                "http://localhost:8081/realms/master/protocol/openid-connect/token",
                new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("client_id", "admin-cli"),
                    new KeyValuePair<string, string>("username", "admin"),
                    new KeyValuePair<string, string>("password", "admin")
                }), cancellationToken);

            response.EnsureSuccessStatusCode();

            var tokenJson = await response.Content.ReadAsStringAsync(cancellationToken);
            using var tokenDoc = JsonDocument.Parse(tokenJson);
            return tokenDoc.RootElement.GetProperty("access_token").GetString();
        }

        public async Task<string?> GetUserIdByEmail(string email, CancellationToken cancellationToken)
        {
            var token = await GetAdminToken(cancellationToken);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"http://localhost:8081/admin/realms/microservicio-usuarios/users?email={email}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(responseJson);

            // using var doc = JsonDocument.Parse(responseJson);

            var userElement = doc.RootElement.EnumerateArray().FirstOrDefault();
            string? userId = null;

            if (userElement.ValueKind != JsonValueKind.Undefined && userElement.TryGetProperty("id", out var idProperty))
            {
                userId = idProperty.GetString();
            }

            return userId;

        }

        public async Task DeleteUser(string userId, CancellationToken cancellationToken)
        {
            var token = await GetAdminToken(cancellationToken);

            var request = new HttpRequestMessage(
                HttpMethod.Delete,
                $"http://localhost:8081/admin/realms/microservicio-usuarios/users/{userId}"
            );
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
        }


        // 2️⃣ Función pública para actualizar el usuario
        public async Task UpdateUserAsync(string email, string firstName, string lastName, CancellationToken cancellationToken)
        {
            var token = await GetAdminToken(cancellationToken);

            // Paso 1: Buscar usuario por email (HTTP GET)
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"http://localhost:8081/admin/realms/microservicio-usuarios/users?email={email}"
            );
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            string? userId = null;

            using (var doc = JsonDocument.Parse(responseJson))
            {
                var userElement = doc.RootElement.EnumerateArray().FirstOrDefault();
                if (userElement.ValueKind != JsonValueKind.Undefined && userElement.TryGetProperty("id", out var idProperty))
                {
                    userId = idProperty.GetString();
                }
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("Usuario no encontrado en Keycloak.");
            }

            // Paso 2: Actualizar el usuario (HTTP PUT)
            var updatedUser = new
            {
                firstName,
                lastName
            };

            var content = new StringContent(JsonSerializer.Serialize(updatedUser), Encoding.UTF8, "application/json");

            var updateRequest = new HttpRequestMessage(
                HttpMethod.Put,
                $"http://localhost:8081/admin/realms/microservicio-usuarios/users/{userId}"
            );
            updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            updateRequest.Content = content;

            var updateResponse = await _httpClient.SendAsync(updateRequest, cancellationToken);
            updateResponse.EnsureSuccessStatusCode();
        }

        public async Task SendResetPasswordEmailAsync(string email, CancellationToken cancellationToken)
        {
            var token = await GetAdminToken(cancellationToken);
            var userId = await GetUserIdByEmail(email, cancellationToken);

            if (userId == null)
            {
                throw new Exception("User not found");
            }

            var request = new HttpRequestMessage(
                HttpMethod.Put,
                $"http://localhost:8081/admin/realms/microservicio-usuarios/users/{userId}/execute-actions-email"
            );

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Aquí indicamos qué acción queremos ejecutar por correo
            var actions = new[] { "UPDATE_PASSWORD" };
            request.Content = new StringContent(JsonSerializer.Serialize(actions), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
        }


        public async Task CreateRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            var token = await GetAdminToken(cancellationToken);

            var role = new
            {
                name = roleName
            };

            var content = new StringContent(JsonSerializer.Serialize(role), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"http://localhost:8081/admin/realms/{_realm}/roles"
            );

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            var response = await _httpClient.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            var token = await GetAdminToken(cancellationToken);

            var request = new HttpRequestMessage(
                HttpMethod.Delete,
                $"http://localhost:8081/admin/realms/{_realm}/roles/{roleName}"
            );
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task AsignarRolAsync(string email, string nombreRol, CancellationToken cancellationToken)
        {
            var token = await GetAdminToken(cancellationToken);
            var userId = await GetUserIdByEmail(email, cancellationToken);

            if (userId == null)
                throw new Exception("Usuario no encontrado en Keycloak.");

            // Obtener todos los roles del realm
            var rolesResponse = await _httpClient.GetAsync(
                $"{_baseUrl}/admin/realms/{_realm}/roles", cancellationToken);
            rolesResponse.EnsureSuccessStatusCode();

            var rolesJson = await rolesResponse.Content.ReadAsStringAsync(cancellationToken);
            using var rolesDoc = JsonDocument.Parse(rolesJson);
            var role = rolesDoc.RootElement.EnumerateArray()
                .FirstOrDefault(r => r.GetProperty("name").GetString() == nombreRol);

            if (role.ValueKind == JsonValueKind.Undefined)
                throw new Exception("El rol especificado no existe en Keycloak.");

            var roleId = role.GetProperty("id").GetString();
            var roleName = role.GetProperty("name").GetString();

            var content = new StringContent(
                JsonSerializer.Serialize(new[] { new { id = roleId, name = roleName } }),
                Encoding.UTF8, "application/json");

            // ✅ CORREGIDO: primero se crea el request, luego se agrega el token
            var assignRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_baseUrl}/admin/realms/{_realm}/users/{userId}/role-mappings/realm");
            assignRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            assignRequest.Content = content;

            var assignResponse = await _httpClient.SendAsync(assignRequest, cancellationToken);
            assignResponse.EnsureSuccessStatusCode();
        }




    }
}

