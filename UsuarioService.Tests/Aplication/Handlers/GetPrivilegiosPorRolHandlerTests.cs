using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Aplicacion.Queries;
using UsuarioServicio.Aplicacion.Servicios;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Dominio.Interfaces;
using Xunit;

public class GetPrivilegiosPorRolHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnPrivilegios_WhenRolIdExists()
    {
        // Arrange
        var rolId = Guid.NewGuid();

        var privilegios = new List<Privilegio>
        {
            new Privilegio { Id = Guid.NewGuid(), NombreTabla = "Usuarios", Operacion = "Crear" },
            new Privilegio { Id = Guid.NewGuid(), NombreTabla = "Roles", Operacion = "Eliminar" }
        };

        var mockRepo = new Mock<IRolPrivilegioRepository>();
        mockRepo
            .Setup(r => r.ObtenerPrivilegiosPorRolAsync(rolId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(privilegios);

        var handler = new GetPrivilegiosPorRolHandler(mockRepo.Object);
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

        var mockRepo = new Mock<IRolPrivilegioRepository>();
        mockRepo
            .Setup(r => r.ObtenerPrivilegiosPorRolAsync(rolId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Privilegio>());

        var handler = new GetPrivilegiosPorRolHandler(mockRepo.Object);
        var query = new GetPrivilegiosPorRolQuery(rolId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
