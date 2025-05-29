using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Aplicacion.Queries;
using UsuarioServicio.Aplicacion.Servicios;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Infraestructura.Persistencia;
using Xunit;

public class GetAllRolesHandlerTests
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
    public async Task Handle_Should_Return_All_Roles_As_DTOs()
    {
        // Arrange
        var context = await GetInMemoryDbContextAsync();

        context.Roles.AddRange(
            new Rol { Id = Guid.NewGuid(), Nombre = "Admin", Descripcion = "Rol de administrador" },
            new Rol { Id = Guid.NewGuid(), Nombre = "User", Descripcion = "Rol de usuario" }
        );
        await context.SaveChangesAsync();

        var handler = new GetAllRolesHandler(context);
        var query = new GetAllRolesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Nombre == "Admin");
        Assert.Contains(result, r => r.Nombre == "User");
    }
}