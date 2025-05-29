using Xunit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Infraestructura.Persistencia;
using UsuarioServicio.Infraestructura.Services;

public class MovimientoUsuarioServiceTests
{
    private ApplicationDbContext CrearDbContextEnMemoria()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task RegistrarMovimientoAsync_DeberiaGuardarMovimiento()
    {
        // Arrange
        var context = CrearDbContextEnMemoria();
        var service = new MovimientoUsuarioService(context);

        var usuarioId = Guid.NewGuid();
        var accion = "Cambio Contraseña";
        var detalles = "Desde ajustes";

        // Act
        await service.RegistrarMovimientoAsync(usuarioId, accion, detalles, CancellationToken.None);

        // Assert
        var movimiento = await context.MovimientosUsuario.FirstOrDefaultAsync();
        Assert.NotNull(movimiento);
        Assert.Equal(usuarioId, movimiento.UsuarioId);
        Assert.Equal("Cambio Contraseña", movimiento.Accion);
        Assert.Equal("Desde ajustes", movimiento.Detalles);
    }
}