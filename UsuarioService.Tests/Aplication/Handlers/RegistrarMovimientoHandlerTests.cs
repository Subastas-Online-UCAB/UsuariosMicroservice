using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Aplicacion.Servicios;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Dominio.Events;
using UsuarioServicio.Dominio.Excepciones;
using UsuarioServicio.Dominio.Interfaces;
using Xunit;

public class RegistrarMovimientoHandlerTests
{
    [Fact]
    public async Task Handle_ShouldPublishMovimientoEvent_WhenUsuarioExists()
    {
        // Arrange
        var email = "test@example.com";
        var usuarioId = Guid.NewGuid();

        var usuario = new Usuario
        {
            Id = usuarioId,
            Email = email,
            Nombre = "Juan",
            Apellido = "Pérez",
            PasswordHash = "1234hash"
        };

        var mockUsuarioRepo = new Mock<IUsuarioRepository>();
        mockUsuarioRepo
            .Setup(r => r.ObtenerPorEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var mockPublisher = new Mock<IRabbitEventPublisher>();

        var command = new RegistrarMovimientoCommand(email, "Login", "Inicio de sesión");
        var handler = new RegistrarMovimientoHandler(mockUsuarioRepo.Object, mockPublisher.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Movimiento enviado correctamente para ser procesado.", result);

        mockPublisher.Verify(p => p.PublicarEventoAsync(
            It.Is<MovimientoRegistradoEvent>(e =>
                e.UsuarioId == usuarioId &&
                e.Accion == "Login" &&
                e.Detalles == "Inicio de sesión"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUsuarioDoesNotExist()
    {
        // Arrange
        var email = "noexiste@example.com";
        var command = new RegistrarMovimientoCommand(email, "Login", "Intento fallido");

        var mockUsuarioRepo = new Mock<IUsuarioRepository>();
        mockUsuarioRepo
            .Setup(r => r.ObtenerPorEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        var mockPublisher = new Mock<IRabbitEventPublisher>();
        var handler = new RegistrarMovimientoHandler(mockUsuarioRepo.Object, mockPublisher.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UsuarioNoEncontradoException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal(email, ex.Message); // ✅ Comparación corregida
        mockPublisher.Verify(p => p.PublicarEventoAsync(It.IsAny<MovimientoRegistradoEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

}
