using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Aplicacion.Servicios;
using UsuarioServicio.Aplicacion.Servicios.UsuarioServicio.Aplicacion.Servicios;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Dominio.Events;
using UsuarioServicio.Infraestructura.Persistencia;
using Xunit;

public class AsignarPrivilegioRolHandlerTests
{
    private async Task<(ApplicationDbContext context, Guid rolId, Guid privilegioId)> GetInMemoryDbContextAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        var context = new ApplicationDbContext(options);

        var rolId = Guid.NewGuid();
        var privilegioId = Guid.NewGuid();

        context.Roles.Add(new Rol
        {
            Id = rolId,
            Nombre = "Admin",
            Descripcion = "Rol administrativo para gestión de usuarios"
        });
        context.Privilegios.Add(new Privilegio
        {
            Id = privilegioId,
            Operacion = "CrearUsuario",
            NombreTabla = "Usuarios" // ✅ Simulación de valor requerido
        });

        await context.SaveChangesAsync();

        return (context, rolId, privilegioId);
    }

    [Fact]
    public async Task Handle_Should_Assign_Privilegio_To_Rol_And_Publish_Event()
    {
        // Arrange
        var (context, rolId, privilegioId) = await GetInMemoryDbContextAsync();

        var mockPublisher = new Mock<IRabbitEventPublisher>();

        var handler = new AsignarPrivilegioRolHandler(context, mockPublisher.Object);

        var command = new AsignarPrivilegioRolCommand(new AsignarPrivilegioRolDTO
        {
            RolId = rolId,
            PrivilegioId = privilegioId
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var exists = await context.RolPrivilegios.AnyAsync(rp =>
            rp.RolId == rolId && rp.PrivilegioId == privilegioId);

        Assert.True(exists);
        Assert.Equal(Unit.Value, result);

        mockPublisher.Verify(p => p.PublicarPrivilegioAsignadoAsync(
            It.Is<PrivilegioAsignadoEvent>(e =>
                e.RolId == rolId && e.PrivilegioId == privilegioId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_Exception_When_Rol_Not_Found()
    {
        // Arrange
        var (context, _, privilegioId) = await GetInMemoryDbContextAsync();

        var mockPublisher = new Mock<IRabbitEventPublisher>();
        var handler = new AsignarPrivilegioRolHandler(context, mockPublisher.Object);

        var command = new AsignarPrivilegioRolCommand(new AsignarPrivilegioRolDTO
        {
            RolId = Guid.NewGuid(), // Rol no existente
            PrivilegioId = privilegioId
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("El Rol o el Privilegio especificado no existen.", exception.Message);
    }

    [Fact]
    public async Task Handle_Should_Throw_Exception_When_Relationship_Already_Exists()
    {
        // Arrange
        var (context, rolId, privilegioId) = await GetInMemoryDbContextAsync();

        context.RolPrivilegios.Add(new RolPrivilegio
        {
            RolId = rolId,
            PrivilegioId = privilegioId
        });
        await context.SaveChangesAsync();

        var mockPublisher = new Mock<IRabbitEventPublisher>();
        var handler = new AsignarPrivilegioRolHandler(context, mockPublisher.Object);

        var command = new AsignarPrivilegioRolCommand(new AsignarPrivilegioRolDTO
        {
            RolId = rolId,
            PrivilegioId = privilegioId
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("El rol ya tiene asignado este privilegio.", exception.Message);
    }


}
