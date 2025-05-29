using System;
using System.Threading.Tasks;
using MassTransit;
using Moq;
using MongoDB.Driver;
using UsuarioServicio.Dominio.Events;
using UsuarioServicio.Infraestructura.Consumers;
using UsuarioServicio.Infraestructura.MongoDB;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;
using Microsoft.Extensions.Logging;
using Xunit;

public class UsuarioActualizadoConsumerTests
{
    [Fact]
    public async Task Consume_Should_Update_Usuario_And_Log_Information()
    {
        // Arrange
        var evento = new UsuarioActualizadoEvent
        {
            UsuarioId = Guid.NewGuid(),
            Nombre = "Miguel",
            Apellido = "Garcia",
            Telefono = "1234567890",
            Direccion = "123 Calle Falsa"
        };

        var fakeContext = Mock.Of<ConsumeContext<UsuarioActualizadoEvent>>(c => c.Message == evento);

        var mockUsuariosCollection = new Mock<IMongoCollection<UsuarioMongo>>();
        var mockUpdateResult = new Mock<UpdateResult>();
        mockUpdateResult.Setup(r => r.ModifiedCount).Returns(1); // Simula que sí actualizó

        mockUsuariosCollection
            .Setup(c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<UsuarioMongo>>(),
                It.IsAny<UpdateDefinition<UsuarioMongo>>(),
                null,
                default))
            .ReturnsAsync(mockUpdateResult.Object);

        var mockMongoDbContext = new Mock<IMongoDbContext>();
        mockMongoDbContext.Setup(m => m.Usuarios).Returns(mockUsuariosCollection.Object);

        var mockLogger = new Mock<ILogger<UsuarioActualizadoConsumer>>();

        var consumer = new UsuarioActualizadoConsumer(mockMongoDbContext.Object, mockLogger.Object);

        // Act
        await consumer.Consume(fakeContext);

        // Assert 
        mockUsuariosCollection.Verify(c => c.UpdateOneAsync(
            It.IsAny<FilterDefinition<UsuarioMongo>>(),
            It.IsAny<UpdateDefinition<UsuarioMongo>>(),
            null,
            default), Times.Once);

        mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Usuario actualizado")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task Consume_Should_Log_Warning_When_Usuario_Not_Found()
    {
        // Arrange
        var evento = new UsuarioActualizadoEvent
        {
            UsuarioId = Guid.NewGuid(),
            Nombre = "Miguel",
            Apellido = "Garcia",
            Telefono = "0000000000",
            Direccion = "Dirección Desconocida"
        };

        var fakeContext = Mock.Of<ConsumeContext<UsuarioActualizadoEvent>>(c => c.Message == evento);

        var mockUsuariosCollection = new Mock<IMongoCollection<UsuarioMongo>>();
        var mockUpdateResult = new Mock<UpdateResult>();
        mockUpdateResult.Setup(r => r.ModifiedCount).Returns(0); // ❌ No se actualizó nada

        mockUsuariosCollection
            .Setup(c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<UsuarioMongo>>(),
                It.IsAny<UpdateDefinition<UsuarioMongo>>(),
                null,
                default))
            .ReturnsAsync(mockUpdateResult.Object);

        var mockMongoDbContext = new Mock<IMongoDbContext>();
        mockMongoDbContext.Setup(m => m.Usuarios).Returns(mockUsuariosCollection.Object);

        var mockLogger = new Mock<ILogger<UsuarioActualizadoConsumer>>();

        var consumer = new UsuarioActualizadoConsumer(mockMongoDbContext.Object, mockLogger.Object);

        // Act
        await consumer.Consume(fakeContext);

        // Assert
        mockLogger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No se encontró el usuario")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

}
