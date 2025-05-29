using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UsuarioServicio.Aplicacion.Servicios;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Infraestructura.Persistencia;
using Xunit;

public class GetAllPrivilegiosHandlerTests
{
    private async Task<ApplicationDbContext> GetInMemoryDbContextAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    [Fact]
    public async Task Handle_Should_Return_All_Privilegios()
    {
        // Arrange
        var context = await GetInMemoryDbContextAsync();

        context.Privilegios.AddRange(
            new Privilegio { Id = Guid.NewGuid(), Operacion = "Leer", NombreTabla = "Lectura de datos" },
            new Privilegio { Id = Guid.NewGuid(), Operacion = "Escribir", NombreTabla = "Escritura de datos" }
        );
        await context.SaveChangesAsync();

        var handler = new GetAllPrivilegiosHandler(context);
        var query = new GetAllPrivilegiosQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Operacion == "Leer");
        Assert.Contains(result, p => p.Operacion == "Escribir");
    }
}