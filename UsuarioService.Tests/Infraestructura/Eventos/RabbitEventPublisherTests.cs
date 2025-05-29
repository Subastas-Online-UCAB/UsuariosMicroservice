using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Moq;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Dominio.Events;
using UsuarioServicio.Infraestructura.Eventos;
using Xunit;

public class RabbitEventPublisherTests
{
    [Fact]
    public async Task PublicarUsuarioCreadoAsync_Should_Call_Publish_With_Correct_Event()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nombre = "Miguel",
            Apellido = "Garcia",
            Email = "miguel@example.com",
            FechaCreacion = DateTime.UtcNow,
            Telefono = "1234567890",
            Direccion = "Calle 123",
            RolId = Guid.NewGuid()
        };

        var mockEndpoint = new Mock<IPublishEndpoint>();
        var publisher = new RabbitEventPublisher(mockEndpoint.Object);

        // Act
        await publisher.PublicarUsuarioCreadoAsync(usuario, CancellationToken.None);

        // Assert
        mockEndpoint.Verify(e => e.Publish(
            It.Is<UsuarioCreadoEvent>(ev =>
                ev.UsuarioId == usuario.Id &&
                ev.Nombre == usuario.Nombre &&
                ev.Apellido == usuario.Apellido &&
                ev.Email == usuario.Email &&
                ev.FechaCreacion == usuario.FechaCreacion &&
                ev.Telefono == usuario.Telefono &&
                ev.Direccion == usuario.Direccion &&
                ev.RolId == usuario.RolId
            ),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task PublicarUsuarioActualizadoAsync_Should_Call_Publish()
    {
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nombre = "Test",
            Apellido = "User",
            Telefono = "000",
            Direccion = "Dir"
        };

        var mock = new Mock<IPublishEndpoint>();
        var publisher = new RabbitEventPublisher(mock.Object);

        await publisher.PublicarUsuarioActualizadoAsync(usuario, CancellationToken.None);

        mock.Verify(p => p.Publish(It.Is<UsuarioActualizadoEvent>(e =>
            e.UsuarioId == usuario.Id &&
            e.Nombre == usuario.Nombre &&
            e.Apellido == usuario.Apellido &&
            e.Telefono == usuario.Telefono &&
            e.Direccion == usuario.Direccion
        ), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task PublicarUsuarioEliminadoAsync_Should_Call_Publish()
    {
        var userId = Guid.NewGuid();
        var email = "delete@example.com";

        var mock = new Mock<IPublishEndpoint>();
        var publisher = new RabbitEventPublisher(mock.Object);

        await publisher.PublicarUsuarioEliminadoAsync(userId, email, CancellationToken.None);

        mock.Verify(p => p.Publish(It.Is<UsuarioEliminadoEvent>(e =>
            e.UsuarioId == userId &&
            e.Email == email
        ), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task PublicarPrivilegioAsignadoAsync_Should_Call_Publish()
    {
        var evento = new PrivilegioAsignadoEvent
        {
            RolId = Guid.NewGuid(),
            PrivilegioId = Guid.NewGuid()
        };

        var mock = new Mock<IPublishEndpoint>();
        var publisher = new RabbitEventPublisher(mock.Object);

        await publisher.PublicarPrivilegioAsignadoAsync(evento, CancellationToken.None);

        mock.Verify(p => p.Publish(evento, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task PublicarPrivilegioEliminadoAsync_Should_Call_Publish()
    {
        var rolId = "rol123";
        var privilegioId = "priv123";

        var mock = new Mock<IPublishEndpoint>();
        var publisher = new RabbitEventPublisher(mock.Object);

        await publisher.PublicarPrivilegioEliminadoAsync(rolId, privilegioId, CancellationToken.None);

        mock.Verify(p => p.Publish(It.Is<PrivilegioEliminadoEvent>(e =>
            e.RolId == rolId && e.PrivilegioId == privilegioId
        ), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task PublicarEventoAsync_DeberiaPublicarCualquierEvento()
    {
        // Arrange
        var mockPublisher = new Mock<IPublishEndpoint>();

        // Setup para Publish(object, CancellationToken)
        mockPublisher
            .Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var service = new RabbitEventPublisher(mockPublisher.Object);

        var evento = new UsuarioCreadoEvent
        {
            UsuarioId = Guid.NewGuid(),
            Email = "miguel@example.com",
            Nombre = "Miguel"
        };

        // Act
        await service.PublicarEventoAsync(evento, CancellationToken.None);

        // Assert
        mockPublisher.Verify(x =>
                x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

}
