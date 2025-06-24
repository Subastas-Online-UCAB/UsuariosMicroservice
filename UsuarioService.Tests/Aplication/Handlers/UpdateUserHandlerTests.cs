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

public class UpdateUserHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateUserAndSyncWithKeycloakAndPublishEvent()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nombre = "Miguel",
            Apellido = "Viejo",
            Email = "miguel@example.com",
            Telefono = "000",
            Direccion = "Antes",
            PasswordHash = "mocked"
        };

        var command = new UpdateUserCommand
        {
            Email = usuario.Email,
            Nombre = "NuevoNombre",
            Apellido = "NuevoApellido",
            Telefono = "999",
            Direccion = "Nueva Calle"
        };

        var mockRepo = new Mock<IUsuarioRepository>();
        mockRepo.Setup(r => r.ObtenerPorEmailAsync(usuario.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);

        mockRepo.Setup(r => r.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

        var mockKeycloak = new Mock<IKeycloakAccountService>();
        mockKeycloak.Setup(k => k.UpdateUserAsync(command.Email, command.Nombre, command.Apellido, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

        var mockRabbit = new Mock<IRabbitEventPublisher>();
        mockRabbit.Setup(r => r.PublicarUsuarioActualizadoAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask)
                  .Verifiable();

        var handler = new UpdateUserHandler(mockRepo.Object, mockKeycloak.Object, mockRabbit.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(command.Nombre, usuario.Nombre);
        Assert.Equal(command.Apellido, usuario.Apellido);
        Assert.Equal(command.Telefono, usuario.Telefono);
        Assert.Equal(command.Direccion, usuario.Direccion);

        mockRepo.Verify();
        mockKeycloak.Verify();
        mockRabbit.Verify();
    }

    [Fact]
    public async Task Handle_ShouldNotCallKeycloak_WhenNombreAndApellidoAreSame()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nombre = "Miguel",
            Apellido = "Garcia",
            Email = "miguel@example.com",
            Telefono = "000",
            Direccion = "Antes",
            PasswordHash = "hash"
        };

        var command = new UpdateUserCommand
        {
            Email = usuario.Email,
            Nombre = "Miguel",
            Apellido = "Garcia",
            Telefono = "999",
            Direccion = "Nueva"
        };

        var mockRepo = new Mock<IUsuarioRepository>();
        mockRepo.Setup(r => r.ObtenerPorEmailAsync(usuario.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);

        mockRepo.Setup(r => r.ActualizarAsync(usuario, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        var mockKeycloak = new Mock<IKeycloakAccountService>();
        var mockRabbit = new Mock<IRabbitEventPublisher>();
        mockRabbit.Setup(r => r.PublicarUsuarioActualizadoAsync(usuario, It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

        var handler = new UpdateUserHandler(mockRepo.Object, mockKeycloak.Object, mockRabbit.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        mockKeycloak.Verify(k => k.UpdateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        mockRabbit.Verify(r => r.PublicarUsuarioActualizadoAsync(usuario, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUsuarioNotFound()
    {
        // Arrange
        var mockRepo = new Mock<IUsuarioRepository>();
        mockRepo.Setup(r => r.ObtenerPorEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        var mockKeycloak = new Mock<IKeycloakAccountService>();
        var mockRabbit = new Mock<IRabbitEventPublisher>();

        var handler = new UpdateUserHandler(mockRepo.Object, mockKeycloak.Object, mockRabbit.Object);

        var command = new UpdateUserCommand
        {
            Email = "no@existe.com",
            Nombre = "x",
            Apellido = "y",
            Telefono = "0",
            Direccion = "cualquier"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UsuarioNoEncontradoException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Equal("no@existe.com", ex.Message); // O el mensaje personalizado que uses
        mockKeycloak.Verify(k => k.UpdateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        mockRabbit.Verify(r => r.PublicarUsuarioActualizadoAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Never);
    }

}
