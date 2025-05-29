using Moq;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Aplicacion.Interfaces;
using UsuarioServicio.Aplicacion.Queries;
using UsuarioServicio.Aplicacion.Servicios;
using UsuarioServicio.Dominio.DTOs;
using UsuarioServicio.Dominio.Interfaces;

public class GetHistorialPorEmailHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnHistorial_WhenUserExists()
    {
        var email = "test@example.com";

        var expected = new List<MovimientoMongoDto>
        {
            new MovimientoMongoDto
            {
                Accion = "LOGIN",
                FechaHora = DateTime.UtcNow,
                Detalles = "Desde navegador"
            }
        };

        var consultaMock = new Mock<IHistorialMovimientoRepository>();
        consultaMock.Setup(s => s.ObtenerPorEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new GetHistorialPorEmailHandler(consultaMock.Object);
        var result = await handler.Handle(new GetHistorialPorEmailQuery(email), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("LOGIN", result[0].Accion);
    }
}