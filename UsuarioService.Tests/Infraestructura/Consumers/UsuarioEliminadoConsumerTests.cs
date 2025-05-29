using System;
using System.Threading.Tasks;
using MassTransit;
using Moq;
using Xunit;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using UsuarioServicio.Dominio.Events;
using UsuarioServicio.Infraestructura.MongoDB;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;
using UsuarioServicio.Infraestructura.Consumers;

public class UsuarioEliminadoConsumerTests
{
    [Fact]
    public async Task Consume_Should_Delete_Usuario_By_Email_And_Log_Info()
    {
        // Arrange
        var evento = new UsuarioEliminadoEvent
        {
            Email = "miguel@example.com"
        };

        var fakeContext = Mock.Of<ConsumeContext<UsuarioEliminadoEvent>>(c => c.Message == evento);

        var mockDeleteResult = new Mock<DeleteResult>();
        mockDeleteResult.Setup(r => r.DeletedCount).Returns(1);

        var mockCollection = new Mock<IMongoCollection<UsuarioMongo>>();
        mockCollection
            .Setup(c => c.DeleteOneAsync(It.IsAny<FilterDefinition<UsuarioMongo>>(), default))
            .ReturnsAsync(mockDeleteResult.Object)
            .Verifiable();

        var mockDb = new Mock<IMongoDbContext>();
        mockDb.Setup(m => m.Usuarios).Returns(mockCollection.Object);

        var mockLogger = new Mock<ILogger<UsuarioEliminadoConsumer>>();

        var consumer = new UsuarioEliminadoConsumer(mockDb.Object, mockLogger.Object);

        // Act
        await consumer.Consume(fakeContext);

        // Assert
        mockCollection.Verify(c => c.DeleteOneAsync(It.IsAny<FilterDefinition<UsuarioMongo>>(), default), Times.Once);

        mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Usuario eliminado")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task Consume_Should_Log_Warning_When_Usuario_Not_Found()
    {
        // Arrange
        var evento = new UsuarioEliminadoEvent
        {
            Email = "desconocido@example.com"
        };

        var fakeContext = Mock.Of<ConsumeContext<UsuarioEliminadoEvent>>(c => c.Message == evento);

        var mockDeleteResult = new Mock<DeleteResult>();
        mockDeleteResult.Setup(r => r.DeletedCount).Returns(0);

        var mockCollection = new Mock<IMongoCollection<UsuarioMongo>>();
        mockCollection
            .Setup(c => c.DeleteOneAsync(It.IsAny<FilterDefinition<UsuarioMongo>>(), default))
            .ReturnsAsync(mockDeleteResult.Object);

        var mockDb = new Mock<IMongoDbContext>();
        mockDb.Setup(m => m.Usuarios).Returns(mockCollection.Object);

        var mockLogger = new Mock<ILogger<UsuarioEliminadoConsumer>>();

        var consumer = new UsuarioEliminadoConsumer(mockDb.Object, mockLogger.Object);

        // Act
        await consumer.Consume(fakeContext);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, type) =>
                    state.ToString().Contains("No se encontró el usuario a eliminar")), // ✅ Frase completa exacta del log
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once);
    }
}
