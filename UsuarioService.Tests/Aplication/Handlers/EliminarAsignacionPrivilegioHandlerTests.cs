using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Aplicacion.Servicios;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Infraestructura.Persistencia;
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

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);

        context.RolPrivilegios.Add(asignacion);
        await context.SaveChangesAsync();

        var mockPublisher = new Mock<IRabbitEventPublisher>();
        mockPublisher
            .Setup(p => p.PublicarPrivilegioEliminadoAsync(
                rolId.ToString(),
                privilegioId.ToString(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new EliminarAsignacionPrivilegioHandler(context, mockPublisher.Object);
        var command = new EliminarAsignacionPrivilegioCommand
        {
            RolId = rolId,
            PrivilegioId = privilegioId
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        var deleted = await context.RolPrivilegios
            .FirstOrDefaultAsync(rp => rp.RolId == rolId && rp.PrivilegioId == privilegioId);
        Assert.Null(deleted);

        mockPublisher.Verify(p => p.PublicarPrivilegioEliminadoAsync(
            rolId.ToString(),
            privilegioId.ToString(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenAssignmentDoesNotExist()
    {
        // Arrange
        var rolId = Guid.NewGuid();
        var privilegioId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);

        var mockPublisher = new Mock<IRabbitEventPublisher>();
        var handler = new EliminarAsignacionPrivilegioHandler(context, mockPublisher.Object);

        var command = new EliminarAsignacionPrivilegioCommand
        {
            RolId = rolId,
            PrivilegioId = privilegioId
        };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
