using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Infraestructura.Persistencia;
using UsuarioServicio.Infraestructura.Persistencia.Repositorio;
using Xunit;

public class UsuarioRepositoryTests
{
    private ApplicationDbContext CrearContextoInMemory()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GuardarUsuarioAsync_Deberia_Guardar_Usuario_Correctamente()
    {
        // Arrange
        var context = CrearContextoInMemory();
        var repository = new UsuarioRepository(context);

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nombre = "Miguel",
            Apellido = "Garcia",
            Email = "miguel@example.com",
            PasswordHash = "hashed123",
            Direccion = "Caracas",
            Telefono = "1234567890",
            RolId = Guid.NewGuid()
        };

        // Act
        await repository.GuardarUsuarioAsync(usuario, CancellationToken.None);

        // Assert
        var usuarioEnDb = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == "miguel@example.com");
        Assert.NotNull(usuarioEnDb);
        Assert.Equal("Miguel", usuarioEnDb.Nombre);
        Assert.Equal("Garcia", usuarioEnDb.Apellido);
    }

    [Fact]
    public async Task EmailExisteAsync_CuandoEmailExiste_DeberiaLanzarExcepcion()
    {
        // Arrange
        var context = CrearContextoInMemory();
        var repository = new UsuarioRepository(context);

        var usuarioExistente = new Usuario
        {
            Id = Guid.NewGuid(),
            Nombre = "Miguel",
            Apellido = "Garcia",
            Email = "miguel@example.com",
            PasswordHash = "hashed123",
            Direccion = "Caracas",
            Telefono = "1234567890",
            RolId = Guid.NewGuid()
        };

        context.Usuarios.Add(usuarioExistente);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await repository.EmailExisteAsync("miguel@example.com", CancellationToken.None);
        });
    }

    [Fact]
    public async Task EmailExisteAsync_CuandoEmailNoExiste_DeberiaRetornarFalse()
    {
        // Arrange
        var context = CrearContextoInMemory();
        var repository = new UsuarioRepository(context);

        // Act
        var resultado = await repository.EmailExisteAsync("nuevo@example.com", CancellationToken.None);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public async Task ObtenerRolPorIdAsync_CuandoRolExiste_DeberiaRetornarRol()
    {
        // Arrange
        var context = CrearContextoInMemory();
        var repository = new UsuarioRepository(context);

        var rolId = Guid.NewGuid();
        var rol = new Rol
        {
            Id = rolId,
            Nombre = "Admin",
            Descripcion = "Rol de administrador"
        };

        context.Roles.Add(rol);
        await context.SaveChangesAsync();

        // Act
        var resultado = await repository.ObtenerRolPorIdAsync(rolId, CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Admin", resultado.Nombre);
    }

    [Fact]
    public async Task ObtenerRolPorIdAsync_CuandoRolNoExiste_DeberiaLanzarExcepcion()
    {
        // Arrange
        var context = CrearContextoInMemory();
        var repository = new UsuarioRepository(context);
        var rolIdInexistente = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await repository.ObtenerRolPorIdAsync(rolIdInexistente, CancellationToken.None);
        });
    }


}