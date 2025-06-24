using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Aplicacion.Servicios;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Dominio.Excepciones;
using UsuarioServicio.Dominio.Interfaces;
using Xunit;

public class EliminarAsignacionPrivilegioHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDeleteAssignmentAndPublishEvent_WhenExists()
    {
        // Arrange
        var rolId = Guid.NewGuid();
        var privilegioId = Guid.NewGuid();

        var asignacion = new RolPrivilegio
        {
            RolId = rolId,
            PrivilegioId = privilegioId
        };

        var mockRepo = new Mock<IRolPrivilegioRepository>();
        var mockPublisher = new Mock<IRabbitEventPublisher>();

        mockRepo
            .Setup(r => r.ObtenerAsignacionAsync(rolId, privilegioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(asignacion);

        mockRepo
            .Setup(r => r.EliminarAsignacionAsync(asignacion, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mockPublisher
            .Setup(p => p.PublicarPrivilegioEliminadoAsync(
                rolId.ToString(), privilegioId.ToString(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new EliminarAsignacionPrivilegioHandler(mockRepo.Object, mockPublisher.Object);
        var command = new EliminarAsignacionPrivilegioCommand
        {
            RolId = rolId,
            PrivilegioId = privilegioId
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        mockRepo.Verify(r => r.EliminarAsignacionAsync(asignacion, It.IsAny<CancellationToken>()), Times.Once);
        mockPublisher.Verify(p => p.PublicarPrivilegioEliminadoAsync(
            rolId.ToString(), privilegioId.ToString(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenAssignmentDoesNotExist()
    {
        // Arrange
        var rolId = Guid.NewGuid();
        var privilegioId = Guid.NewGuid();

        var mockRepo = new Mock<IRolPrivilegioRepository>();
        var mockPublisher = new Mock<IRabbitEventPublisher>();

        mockRepo
            .Setup(r => r.ObtenerAsignacionAsync(rolId, privilegioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RolPrivilegio?)null);

        var handler = new EliminarAsignacionPrivilegioHandler(mockRepo.Object, mockPublisher.Object);
        var command = new EliminarAsignacionPrivilegioCommand
        {
            RolId = rolId,
            PrivilegioId = privilegioId
        };

        // Act & Assert
        await Assert.ThrowsAsync<AsignacionNoEncontradaException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
