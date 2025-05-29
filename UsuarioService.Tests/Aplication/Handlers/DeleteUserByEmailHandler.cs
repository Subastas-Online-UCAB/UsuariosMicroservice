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
using Xunit;

public class DeleteUserByEmailHandlerTests
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
    public async Task Handle_Should_Delete_User_And_Publish_Event()
    {
        // Arrange
        var context = await GetInMemoryDbContextAsync();

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nombre = "Miguel",
            Apellido = "Garcia",
            Email = "miguel@example.com",
            Telefono = "123",
            Direccion = "Calle",
            PasswordHash = "hash123"
        };

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var command = new DeleteUserByEmailCommand(usuario.Email);


        var keycloakMock = new Mock<IKeycloakAccountService>();
        keycloakMock.Setup(k => k.GetUserIdByEmail(usuario.Email, It.IsAny<CancellationToken>()))
                    .ReturnsAsync("keycloak-user-id");

        keycloakMock.Setup(k => k.DeleteUser("keycloak-user-id", It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

        var rabbitMock = new Mock<IRabbitEventPublisher>();
        rabbitMock.Setup(r => r.PublicarUsuarioEliminadoAsync(usuario.Id, usuario.Email, It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask)
                  .Verifiable();

        var handler = new DeleteUserByEmailHandler(context, keycloakMock.Object, rabbitMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal($"Usuario con correo {usuario.Email} eliminado exitosamente.", result);

        var userInDb = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == usuario.Email);
        Assert.Null(userInDb); // ✅ ya no debe estar

        keycloakMock.Verify(k => k.DeleteUser("keycloak-user-id", It.IsAny<CancellationToken>()), Times.Once);
        rabbitMock.Verify(r => r.PublicarUsuarioEliminadoAsync(usuario.Id, usuario.Email, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_User_Not_Found()
    {
        // Arrange
        var context = await GetInMemoryDbContextAsync(); // sin usuarios

        var command = new DeleteUserByEmailCommand("miguel@example.com");

        var keycloakMock = new Mock<IKeycloakAccountService>();
        var rabbitMock = new Mock<IRabbitEventPublisher>();

        var handler = new DeleteUserByEmailHandler(context, keycloakMock.Object, rabbitMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal($"No se encontró un usuario con el correo: {command.Email}", exception.Message);
    }
}
