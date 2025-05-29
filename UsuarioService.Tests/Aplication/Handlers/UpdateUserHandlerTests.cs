using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Aplicacion.Servicios;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Dominio.Interfaces;
using UsuarioServicio.Infraestructura.Eventos;
using UsuarioServicio.Infraestructura.Persistencia;
using UsuarioServicio.Infraestructura.Services;
using Xunit;

public class UpdateUserHandlerTests
{
    private async Task<ApplicationDbContext> GetInMemoryDbContextAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    [Fact]
    public async Task Handle_Should_Update_User_And_Sync_With_Keycloak_And_Publish_Event()
    {
        // Arrange
        var context = await GetInMemoryDbContextAsync();

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nombre = "Miguel",
            Apellido = "Viejo",
            Email = "miguel@example.com",
            Telefono = "000",
            Direccion = "Antes",
            PasswordHash = "mockedhash123" // ← lo agregas para evitar el error
        };

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var command = new UpdateUserCommand
        {
            Email = usuario.Email,
            Nombre = "NuevoNombre",  // <- Cambio
            Apellido = "NuevoApellido",  // <- Cambio
            Telefono = "999",
            Direccion = "Nueva Calle"
        };

        var mockKeycloak = new Mock<IKeycloakAccountService>();
        mockKeycloak.Setup(k => k.UpdateUserAsync(
            command.Email, command.Nombre, command.Apellido, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var rabbitMock = new Mock<IRabbitEventPublisher>();
        rabbitMock.Setup(r => r.PublicarUsuarioActualizadoAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var handler = new UpdateUserHandler(context, mockKeycloak.Object, rabbitMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        var updated = await context.Usuarios.FirstAsync(u => u.Email == command.Email);
        Assert.Equal(command.Nombre, updated.Nombre);
        Assert.Equal(command.Apellido, updated.Apellido);
        Assert.Equal(command.Telefono, updated.Telefono);
        Assert.Equal(command.Direccion, updated.Direccion);

        mockKeycloak.Verify(k => k.UpdateUserAsync(command.Email, command.Nombre, command.Apellido, It.IsAny<CancellationToken>()), Times.Once);
        rabbitMock.Verify(r => r.PublicarUsuarioActualizadoAsync(It.Is<Usuario>(u => u.Email == command.Email), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Not_Call_Keycloak_When_Nombre_And_Apellido_Do_Not_Change()
    {
        // Arrange
        var context = await GetInMemoryDbContextAsync();

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nombre = "Miguel",
            Apellido = "Garcia",
            Email = "miguel@example.com",
            Telefono = "000",
            Direccion = "Vieja Dirección",
            PasswordHash = "mockedhash123"
        };

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var command = new UpdateUserCommand
        {
            Email = usuario.Email,
            Nombre = "Miguel", // ❌ No cambia
            Apellido = "Garcia", // ❌ No cambia
            Telefono = "999",
            Direccion = "Nueva Dirección"
        };

        var mockKeycloak = new Mock<IKeycloakAccountService>();
        var mockRabbit = new Mock<IRabbitEventPublisher>();
        mockRabbit.Setup(r => r.PublicarUsuarioActualizadoAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new UpdateUserHandler(context, mockKeycloak.Object, mockRabbit.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);

        // ❌ Keycloak NO debe llamarse
        mockKeycloak.Verify(k => k.UpdateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        // ✅ Rabbit sí se debe llamar
        mockRabbit.Verify(r => r.PublicarUsuarioActualizadoAsync(It.Is<Usuario>(u => u.Email == command.Email), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_User_Not_Found()
    {
        // Arrange
        var context = await GetInMemoryDbContextAsync(); // Sin agregar usuarios

        var command = new UpdateUserCommand
        {
            Email = "inexistente@example.com",
            Nombre = "Nuevo",
            Apellido = "Nombre",
            Telefono = "1111",
            Direccion = "Calle falsa"
        };

        var mockKeycloak = new Mock<IKeycloakAccountService>();
        var mockRabbit = new Mock<IRabbitEventPublisher>();

        var handler = new UpdateUserHandler(context, mockKeycloak.Object, mockRabbit.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("Usuario no encontrado.", exception.Message);
    }

}
