using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using UsuarioServicio.Infraestructura.Services;
using Xunit;

public class KeycloakUserRegistrationServiceTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;
    private readonly KeycloakUserRegistrationService _service;

    public KeycloakUserRegistrationServiceTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        _service = new KeycloakUserRegistrationService(_httpClient);
    }

    [Fact]
    public async Task RegistrarUsuarioAsync_DeberiaRetornarUserId()
    {
        var expectedUserId = "abc123";

        _handlerMock.Protected().SetupSequence<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())

            .ReturnsAsync(new HttpResponseMessage // Token
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"fake-token\"}")
            })

            .ReturnsAsync(new HttpResponseMessage // Crear usuario
            {
                StatusCode = HttpStatusCode.Created,
                Headers = { Location = new Uri($"http://localhost/admin/realms/microservicio-usuarios/users/{expectedUserId}") }
            });

        var result = await _service.RegistrarUsuarioAsync("Miguel", "Garcia", "miguel@example.com", "123", CancellationToken.None);

        Assert.Equal(expectedUserId, result);
    }

    [Fact]
    public async Task AsignarRolAsync_DeberiaAsignarRolCorrectamente()
    {
        var userId = "abc123";
        var roleId = "admin-role-id";

        _handlerMock.Protected().SetupSequence<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())

            .ReturnsAsync(new HttpResponseMessage // Token
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"fake-token\"}")
            })

            .ReturnsAsync(new HttpResponseMessage // Lista de roles
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent($"[{{\"id\":\"{roleId}\",\"name\":\"admin\"}}]")
            })

            .ReturnsAsync(new HttpResponseMessage // Asignar rol
            {
                StatusCode = HttpStatusCode.NoContent
            });

        await _service.AsignarRolAsync(userId, "admin", CancellationToken.None);
    }

    [Fact]
    public async Task EnviarCorreoVerificacionAsync_DeberiaEnviarSinErrores()
    {
        var userId = "abc123";

        _handlerMock.Protected().SetupSequence<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())

            .ReturnsAsync(new HttpResponseMessage // Token
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"fake-token\"}")
            })

            .ReturnsAsync(new HttpResponseMessage // Enviar correo
            {
                StatusCode = HttpStatusCode.NoContent
            });

        await _service.EnviarCorreoVerificacionAsync(userId, CancellationToken.None);
    }

    [Fact]
    public async Task AsignarRolAsync_DeberiaLanzarExcepcionSiRolNoExiste()
    {
        var userId = "abc123";

        _handlerMock.Protected().SetupSequence<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())

            .ReturnsAsync(new HttpResponseMessage // Token
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"fake-token\"}")
            })

            .ReturnsAsync(new HttpResponseMessage // Lista de roles vacía
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[]")
            });

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _service.AsignarRolAsync(userId, "invalido", CancellationToken.None));

        Assert.Equal("El rol 'invalido' no existe en Keycloak.", ex.Message);
    }

    [Fact]
    public async Task RegistrarUsuarioAsync_DeberiaLanzarExcepcionSiNoSeCrea()
    {
        _handlerMock.Protected().SetupSequence<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage // Token
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"fake-token\"}")
            })
            .ReturnsAsync(new HttpResponseMessage // Falla al crear usuario
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _service.RegistrarUsuarioAsync("Juan", "Lopez", "juan@example.com", "1234", CancellationToken.None));

        Assert.Equal("No se pudo crear el usuario en Keycloak.", ex.Message);
    }

    [Fact]
    public async Task EnviarCorreoVerificacionAsync_DeberiaLanzarExcepcionSiFalla()
    {
        var userId = "abc123";

        _handlerMock.Protected().SetupSequence<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage // Token
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"fake-token\"}")
            })
            .ReturnsAsync(new HttpResponseMessage // Falla al enviar correo
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Fallo en envío")
            });

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _service.EnviarCorreoVerificacionAsync(userId, CancellationToken.None));

        Assert.Equal("No se pudo enviar el correo de verificación: Fallo en envío", ex.Message);
    }

    [Fact]
    public async Task ObtenerTokenAdminAsync_DeberiaLanzarExcepcionSiFalla()
    {
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post && req.RequestUri!.ToString().Contains("/token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("Unauthorized")
            });

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _service.RegistrarUsuarioAsync("Test", "Fail", "fail@example.com", "1234", CancellationToken.None));

        Assert.Contains("Error obteniendo token admin Keycloak", ex.Message);
    }

}
