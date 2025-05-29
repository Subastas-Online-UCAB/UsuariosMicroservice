using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Aplicacion.Queries;
using UsuarioServicio.Aplicacion.Servicios;
using UsuarioServicio.Dominio.Interfaces;
using UsuarioServicio.Dominio.DTOs;
using Xunit;

public class GetUserByEmailHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnUsuarioMongoDto_WhenUserExists()
    {
        // Arrange
        var email = "miguel@example.com";
        var usuarioEsperado = new UsuarioMongoDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Miguel",
            Apellido = "Garcia",
            Email = email,
            FechaCreacion = DateTime.UtcNow,
            Telefono = "123456789",
            Direccion = "Caracas",
            RolId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Rol = new RolMongoDto
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Nombre = "Administrador",
                Descripcion = "Admin del sistema"
            }
        };

        var mockRepo = new Mock<IUsuarioMongoRepository>();
        mockRepo
            .Setup(r => r.ObtenerPorEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuarioEsperado);

        var handler = new GetUserByEmailHandler(mockRepo.Object);
        var query = new GetUserByEmailQuery(email);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(usuarioEsperado.Email, result.Email);
        Assert.Equal(usuarioEsperado.Nombre, result.Nombre);
        Assert.Equal(usuarioEsperado.Rol.Nombre, result.Rol.Nombre);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var email = "noexiste@example.com";

        var mockRepo = new Mock<IUsuarioMongoRepository>();
        mockRepo
            .Setup(r => r.ObtenerPorEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioMongoDto)null);

        var handler = new GetUserByEmailHandler(mockRepo.Object);
        var query = new GetUserByEmailQuery(email);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}
