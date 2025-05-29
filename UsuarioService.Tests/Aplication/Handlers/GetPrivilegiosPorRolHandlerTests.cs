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

public class GetPrivilegiosPorRolHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnPrivilegios_WhenRolIdExists()
    {
        // Arrange
        var rolId = Guid.NewGuid();

        var privilegio1 = new Privilegio
        {
            Id = Guid.NewGuid(),
            NombreTabla = "Usuarios",
            Operacion = "Crear"
        };

        var privilegio2 = new Privilegio
        {
            Id = Guid.NewGuid(),
            NombreTabla = "Roles",
            Operacion = "Eliminar"
        };

        var rolPrivilegio1 = new RolPrivilegio
        {
            RolId = rolId,
            PrivilegioId = privilegio1.Id,
            Privilegio = privilegio1
        };

        var rolPrivilegio2 = new RolPrivilegio
        {
            RolId = rolId,
            PrivilegioId = privilegio2.Id,
            Privilegio = privilegio2
        };

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);

        context.Privilegios.AddRange(privilegio1, privilegio2);
        context.RolPrivilegios.AddRange(rolPrivilegio1, rolPrivilegio2);
        await context.SaveChangesAsync();

        var handler = new GetPrivilegiosPorRolHandler(context);
        var query = new GetPrivilegiosPorRolQuery(rolId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Nombre == "Usuarios" && p.Descripcion == "Crear");
        Assert.Contains(result, p => p.Nombre == "Roles" && p.Descripcion == "Eliminar");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenRolIdHasNoPrivilegios()
    {
        // Arrange
        var rolId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);

        var handler = new GetPrivilegiosPorRolHandler(context);
        var query = new GetPrivilegiosPorRolQuery(rolId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
