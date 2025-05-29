using Xunit;
using Moq;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Dominio.Interfaces;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Aplicacion.Servicios;

public class RegisterUserHandlerTests
{
    [Fact]
    public async Task Should_Register_User_Successfully()
    {
        // Arrange
        var rolId = Guid.NewGuid();
        var fakeRol = new Rol
        {
            Id = rolId,
            Nombre = "Administrador"
        };


        var command = new RegisterUserCommand
        {
            Nombre = "Juan",
            Apellido = "Pérez",
            Email = "juan@example.com",
            Password = "12345",
            Telefono = "1234567890",
            Direccion = "Av. Principal",
            RolId = rolId 
        };

        var mockRepo = new Mock<IUsuarioRepository>();
        var mockKeycloak = new Mock<IKeycloakService>();
        var mockRabbit = new Mock<IRabbitEventPublisher>();

        mockRepo.Setup(r => r.ObtenerRolPorIdAsync(rolId, It.IsAny<CancellationToken>())).ReturnsAsync(fakeRol);
        mockRepo.Setup(r => r.EmailExisteAsync(command.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        mockKeycloak.Setup(k => k.RegistrarUsuarioAsync(command.Nombre, command.Apellido, command.Email, command.Password, It.IsAny<CancellationToken>())).ReturnsAsync("fake-keycloak-id");
        mockKeycloak.Setup(k => k.AsignarRolAsync("fake-keycloak-id", fakeRol.Nombre, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockRepo.Setup(r => r.GuardarUsuarioAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockRabbit.Setup(e => e.PublicarUsuarioCreadoAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new RegisterUserHandler(mockKeycloak.Object, mockRepo.Object, mockRabbit.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        mockRepo.Verify(r => r.GuardarUsuarioAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Once);
        mockRabbit.Verify(r => r.PublicarUsuarioCreadoAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task Should_Throw_When_Email_Already_Exists()
    {
        //arrange

        var command = new RegisterUserCommand
        {
            Nombre = "Pedro",
            Apellido = "Gomez",
            Email = "ya-registrado@example.com",
            Password = "pass123",
            Telefono = "123456",
            Direccion = "Calle 1",
            RolId = Guid.NewGuid()
        };

        var mockRepo = new Mock<IUsuarioRepository>();
        var mockKeycloack = new Mock<IKeycloakService>();
        var mockRabbit = new Mock<IRabbitEventPublisher>();

        mockRepo.Setup(r => r.ObtenerRolPorIdAsync(command.RolId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rol { Id = command.RolId, Nombre = "Admin" });

        mockRepo.Setup(r => r.EmailExisteAsync(command.Email, It.IsAny<CancellationToken>())).ReturnsAsync(true);


        //act
        var handler =  new RegisterUserHandler(mockKeycloack.Object, mockRepo.Object, mockRabbit.Object);

        // act ^^ assert 

        await Assert.ThrowsAsync<Exception>(() =>
            handler.Handle(command, CancellationToken.None));

    }

    [Fact]
    public async Task Should_Throw_And_Not_Save_When_Email_Already_Exists()
    {
        //arrange 

        var command = new RegisterUserCommand()
        {
            Nombre = "Pedro",
            Apellido = "Gomez",
            Email = "ya-registrado@example.com",
            Password = "pass123",
            Telefono = "123456",
            Direccion = "Calle 1",
            RolId = Guid.NewGuid()

        };

        var mockRepo = new Mock<IUsuarioRepository>();
        var mockKeycloack = new Mock<IKeycloakService>();
        var mockRabbit = new Mock<IRabbitEventPublisher>();

        mockRepo.Setup(r => r.ObtenerRolPorIdAsync(command.RolId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Rol() { Id = command.RolId, Nombre = "Admin" });
        mockRepo.Setup(r => r.EmailExisteAsync(command.Email, It.IsAny<CancellationToken>())).ReturnsAsync(true);


        // act

        var handler = new RegisterUserHandler(mockKeycloack.Object, mockRepo.Object, mockRabbit.Object);


        //Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));

       mockRepo.Verify(r=> r.GuardarUsuarioAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Never);
       mockRabbit.Verify(r => r.PublicarUsuarioCreadoAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Never);
       mockKeycloack.Verify(k => k.RegistrarUsuarioAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

    }

    [Fact]
    public async Task Handle_Should_Throw_When_Rol_Does_Not_Exist()
    {
        var command = new RegisterUserCommand()
        {
            Nombre = "Pedro",
            Apellido = "Gomez",
            Email = "ya-registrado@example.com",
            Password = "pass123",
            Telefono = "123456",
            Direccion = "Calle 1",
            RolId = Guid.NewGuid()
        };

        var mockRepo = new Mock<IUsuarioRepository>();
        var mockKeycloak = new Mock<IKeycloakService>();
        var mockRabbit = new Mock<IRabbitEventPublisher>();

        mockRepo.Setup(r => r.ObtenerRolPorIdAsync(command.RolId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Rol?)null);

        var handler = new RegisterUserHandler(mockKeycloak.Object, mockRepo.Object, mockRabbit.Object);

        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
    }
}
