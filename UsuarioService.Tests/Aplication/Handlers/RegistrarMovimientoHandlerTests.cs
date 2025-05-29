using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Aplicacion.Servicios;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Dominio.Events;
using UsuarioServicio.Dominio.Interfaces;
using UsuarioServicio.Infraestructura.Persistencia;
using Xunit;

public class RegistrarMovimientoHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRegisterMovimiento_WhenUsuarioExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "RegistrarMovimientoTestDb")
            .Options;

        var email = "test@example.com";
        var usuarioId = Guid.NewGuid();

        using (var context = new ApplicationDbContext(options))
        {
            context.Usuarios.Add(new Usuario
            {
                Id = usuarioId,
                Email = email,
                Nombre = "Juan",             // Campos requeridos
                Apellido = "Pérez",
                PasswordHash = "1234hash"
            });

            await context.SaveChangesAsync();
        }

        var mockEventPublisher = new Mock<IRabbitEventPublisher>();

        var command = new RegistrarMovimientoCommand(email, "Login", "Inicio de sesión");
        string result;

        using (var context = new ApplicationDbContext(options))
        {
            var handler = new RegistrarMovimientoHandler(context, mockEventPublisher.Object);

            // Act
            result = await handler.Handle(command, CancellationToken.None);
        }

        // Assert
        Assert.Equal("Movimiento registrado correctamente.", result);
        mockEventPublisher.Verify(p => p.PublicarEventoAsync(
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
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("UsuarioNoExisteTest")
            .Options;

        var mockPublisher = new Mock<IRabbitEventPublisher>();
        var command = new RegistrarMovimientoCommand("noexiste@example.com", "Login", "Intento fallido");

        using var context = new ApplicationDbContext(options);
        var handler = new RegistrarMovimientoHandler(context, mockPublisher.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("Usuario no encontrado.", ex.Message);
        mockPublisher.Verify(p => p.PublicarEventoAsync(It.IsAny<MovimientoRegistradoEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

}
