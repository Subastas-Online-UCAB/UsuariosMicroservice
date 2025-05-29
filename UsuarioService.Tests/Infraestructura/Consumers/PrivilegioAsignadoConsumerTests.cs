using Xunit;
using Moq;
using System.Threading.Tasks;
using MassTransit;
using UsuarioServicio.Infraestructura.Consumers;
using UsuarioServicio.Dominio.Events;
using UsuarioServicio.Infraestructura.MongoDB;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;
using MongoDB.Driver;

public class PrivilegioAsignadoConsumerTests
{
    [Fact]
    public async Task Consume_Should_Insert_RolPrivilegioMongo_In_Mongo()
    {
        // Arrange
        var evento = new PrivilegioAsignadoEvent
        {
            RolId = Guid.NewGuid(),
            PrivilegioId = Guid.NewGuid()
        };

        var fakeContext = Mock.Of<ConsumeContext<PrivilegioAsignadoEvent>>(c => c.Message == evento);

        var mockCollection = new Mock<IMongoCollection<RolPrivilegioMongo>>();
        mockCollection.Setup(x => x.InsertOneAsync(It.IsAny<RolPrivilegioMongo>(), null, default))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var mockDb = new Mock<IMongoDbContext>();
        mockDb.Setup(m => m.RolesPrivilegios).Returns(mockCollection.Object);

        var consumer = new PrivilegioAsignadoConsumer(mockDb.Object);

        // Act
        await consumer.Consume(fakeContext);

        // Assert
        mockCollection.Verify(x => x.InsertOneAsync(
            It.Is<RolPrivilegioMongo>(doc =>
                doc.RolId == evento.RolId.ToString() &&
                doc.PrivilegioId == evento.PrivilegioId.ToString()),
            null, default), Times.Once);
    }
}