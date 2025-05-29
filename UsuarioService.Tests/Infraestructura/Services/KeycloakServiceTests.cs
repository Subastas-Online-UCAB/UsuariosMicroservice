using Xunit;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UsuarioServicio.Infraestructura.Services;

public class KeycloakServiceTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;
    private readonly KeycloakService _service;

    public KeycloakServiceTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
        _service = new KeycloakService(_httpClient);
    }

    [Fact]
    public async Task GetAdminToken_ShouldReturnToken_WhenRequestIsSuccessful()
    {
        // Arrange
        var expectedToken = "test-token";

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains("/token")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent($"{{\"access_token\":\"{expectedToken}\"}}")
            });

        // Act
        var result = await _service.GetAdminToken(CancellationToken.None);

        // Assert
        Assert.Equal(expectedToken, result);

        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString().Contains("/token")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetUserIdByEmail_DeberiaRetornarIdSiUsuarioExiste()
    {
        // Arrange
        var email = "miguel@example.com";
        var fakeUserId = "123456789";

        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())

            // 1️⃣ Simula GetAdminToken
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"fake-token\"}")
            })

            // 2️⃣ Simula respuesta de búsqueda de usuario
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent($"[{{\"id\":\"{fakeUserId}\"}}]")
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var service = new KeycloakService(httpClient);

        // Act
        var result = await service.GetUserIdByEmail(email, CancellationToken.None);

        // Assert
        Assert.Equal(fakeUserId, result);

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetUserIdByEmail_DeberiaRetornarNullSiUsuarioNoExiste()
    {
        // Arrange
        var email = "no-existe@example.com";

        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())

            // 1️⃣ Simula GetAdminToken
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"fake-token\"}")
            })

            // 2️⃣ Simula respuesta vacía (usuario no encontrado)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[]") // sin usuarios
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var service = new KeycloakService(httpClient);

        // Act
        var result = await service.GetUserIdByEmail(email, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SendResetPasswordEmailAsync_DeberiaEjecutarSolicitudCuandoUsuarioExiste()
    {
        var email = "miguel@example.com";
        var userId = "123456";

        var handlerMock = new Mock<HttpMessageHandler>();

        // 1️⃣ Token
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString().Contains("/token") &&
                    req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"fake-token\"}")
            });

        // 2️⃣ Buscar usuario
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString().Contains("/users?email=") &&
                    req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent($"[{{\"id\":\"{userId}\"}}]")
            });

        // 3️⃣ Enviar correo
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString().Contains($"/users/{userId}/execute-actions-email") &&
                    req.Method == HttpMethod.Put),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            });

        var client = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var service = new KeycloakService(client);

        // Act
        await service.SendResetPasswordEmailAsync(email, CancellationToken.None);

        // Assert
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(4),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }



    [Fact]
    public async Task SendResetPasswordEmailAsync_DeberiaLanzarExcepcionCuandoUsuarioNoExiste()
    {
        // Arrange
        var email = "inexistente@example.com";

        var handlerMock = new Mock<HttpMessageHandler>();

        // 1️⃣ Simular respuesta del token
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains("/token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"fake-token\"}")
            });

        // 2️⃣ Simular búsqueda de usuario que no existe
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains("/users?email=")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[]") // Usuario no encontrado
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var service = new KeycloakService(httpClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            service.SendResetPasswordEmailAsync(email, CancellationToken.None));

        Assert.Equal("User not found", exception.Message);

        // Verificar que se realizaron exactamente 2 llamadas
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(3),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task AsignarRolAsync_DeberiaAsignarRolCorrectamente()
    {
        // Arrange
        var email = "miguel@example.com";
        var rol = "admin";
        var userId = "123";
        var rolId = "456";

        var handlerMock = new Mock<HttpMessageHandler>();

        // 1️⃣ Token
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains("/token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"fake-token\"}")
            });

        // 2️⃣ Buscar usuario por email
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains("/users?email=")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent($"[{{\"id\":\"{userId}\"}}]")
            });

        // 3️⃣ Obtener lista de roles disponibles
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains("/roles") &&
                    !req.RequestUri.ToString().Contains("/users")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent($"[{{\"id\":\"{rolId}\",\"name\":\"{rol}\"}}]")
            });

        // 4️⃣ Asignar rol
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains($"/users/{userId}/role-mappings/realm")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var service = new KeycloakService(httpClient);

        // Act
        await service.AsignarRolAsync(email, rol, CancellationToken.None);

        // Assert: Se deben hacer 4 llamadas HTTP (una para cada paso)
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(5),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }


    [Fact]
    public async Task AsignarRolAsync_DeberiaLanzarExcepcionCuandoRolNoExiste()
    {
        // Arrange
        var email = "miguel@example.com";
        var rolBuscado = "admin";
        var userId = "123";

        var handlerMock = new Mock<HttpMessageHandler>();

        // 1️⃣ Token
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains("/token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"fake-token\"}")
            });

        // 2️⃣ Buscar usuario por email
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains("/users?email=")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent($"[{{\"id\":\"{userId}\"}}]")
            });

        // 3️⃣ Obtener lista de roles (ninguno coincide con el buscado)
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains("/roles") &&
                    !req.RequestUri.ToString().Contains("/users")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[{\"id\":\"888\",\"name\":\"viewer\"}]") // sin el rol "admin"
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var service = new KeycloakService(httpClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            service.AsignarRolAsync(email, rolBuscado, CancellationToken.None));

        Assert.Equal("El rol especificado no existe en Keycloak.", exception.Message);

        // Verifica que se hicieron solo 3 llamadas (token, user, roles)
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(4),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }



    [Fact]
    public async Task AsignarRolAsync_DeberiaLanzarExcepcionCuandoUsuarioNoExiste()
    {
        // Arrange
        var email = "noexiste@example.com";
        var rol = "admin";

        var handlerMock = new Mock<HttpMessageHandler>();

        // 1️⃣ Token
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains("/token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"fake-token\"}")
            });

        // 2️⃣ Buscar usuario (simula lista vacía)
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains("/users?email=")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[]") // Usuario no encontrado
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var service = new KeycloakService(httpClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            service.AsignarRolAsync(email, rol, CancellationToken.None));

        Assert.Equal("Usuario no encontrado en Keycloak.", exception.Message);

        // Verifica que solo se hicieron 2 llamadas (token + búsqueda de usuario)
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(3),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }


   
    [Fact]
    public async Task UpdateUserAsync_DeberiaActualizarUsuarioCorrectamente()
    {
        // Arrange
        var email = "miguel@example.com";
        var nombre = "Miguel";
        var apellido = "Garcia";
        var userId = "123";

        var handlerMock = new Mock<HttpMessageHandler>();

        // 1️⃣ Token
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains("/token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"fake-token\"}")
            });

        // 2️⃣ Buscar usuario por email
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains("/users?email=")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent($"[{{\"id\":\"{userId}\"}}]")
            });

        // 3️⃣ Actualizar datos del usuario
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Put &&
                    req.RequestUri.ToString().Contains($"/users/{userId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var service = new KeycloakService(httpClient);

        // Act
        await service.UpdateUserAsync(email, nombre, apellido, CancellationToken.None);

        // Assert
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(3),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }


    [Fact]
    public async Task UpdateUserAsync_DeberiaLanzarExcepcionCuandoUsuarioNoExiste()
    {
        // Arrange
        var email = "noexiste@example.com";
        var nombre = "Juan";
        var apellido = "Pérez";

        var handlerMock = new Mock<HttpMessageHandler>();

        // 1️⃣ Token
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains("/token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"fake-token\"}")
            });

        // 2️⃣ Buscar usuario por email → no encontrado (respuesta vacía)
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains("/users?email=")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[]") // Usuario no encontrado
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var service = new KeycloakService(httpClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            service.UpdateUserAsync(email, nombre, apellido, CancellationToken.None));

        Assert.Equal("Usuario no encontrado en Keycloak.", exception.Message);

        // Verifica que solo se hicieron 2 llamadas: token y búsqueda
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }


    [Fact]
    public async Task DeleteUser_DeberiaEliminarUsuarioCorrectamente()
    {
        // Arrange
        var userId = "abc-123";

        var handlerMock = new Mock<HttpMessageHandler>();

        // 1️⃣ Token
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains("/token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"fake-token\"}")
            });

        // 2️⃣ Eliminar usuario
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.RequestUri.ToString().Contains($"/users/{userId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var service = new KeycloakService(httpClient);

        // Act
        await service.DeleteUser(userId, CancellationToken.None);

        // Assert
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(2), // Token + Delete
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }



}