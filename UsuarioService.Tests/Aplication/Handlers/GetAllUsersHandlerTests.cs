using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using UsuarioServicio.Aplicacion.Queries;
using UsuarioServicio.Aplicacion.Servicios;
using UsuarioServicio.Dominio.DTOs;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Dominio.Interfaces;
using Xunit;

public class GetAllUsersHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_List_Of_Users_With_Their_RoleId()
    {
        // Arrange
        var usuarios = new List<UsuarioMongoDto>
        {
            new UsuarioMongoDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Miguel",
                Apellido = "García",
                Email = "miguel@example.com",
                FechaCreacion = DateTime.UtcNow,
                Telefono = "123456789",
                Direccion = "Calle A",
                RolId = Guid.NewGuid()
            },
            new UsuarioMongoDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Ana",
                Apellido = "Ramos",
                Email = "ana@example.com",
                FechaCreacion = DateTime.UtcNow,
                Telefono = "987654321",
                Direccion = "Calle B",
                RolId = Guid.NewGuid()
            }
        };

        var mockRepo = new Mock<IUsuarioMongoRepository>();
        mockRepo.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuarios);

        var handler = new GetAllUsersHandler(mockRepo.Object);
        var request = new GetAllUsersQuery();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, u => Assert.False(string.IsNullOrWhiteSpace(u.Nombre)));
        Assert.All(result, u => Assert.NotEqual(Guid.Empty, u.Id));
    }
}
