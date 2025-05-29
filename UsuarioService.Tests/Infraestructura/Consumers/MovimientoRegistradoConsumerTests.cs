using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using MassTransit;
using MongoDB.Driver;
using UsuarioServicio.Infraestructura.Consumers;
using UsuarioServicio.Dominio.Events;
using UsuarioServicio.Infraestructura.MongoDB;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;

public class MovimientoRegistradoConsumerTests
{
    [Fact]
    public async Task Consume_Deberia_Insertar_MovimientoUsuarioMongo()
    {
        // Arrange
        var evento = new MovimientoRegistradoEvent
        {
            UsuarioId = Guid.NewGuid(),
            Accion = "Login",
            Detalles = "Inicio exitoso",
            FechaHora = DateTime.UtcNow
        };

        var fakeContext = Mock.Of<ConsumeContext<MovimientoRegistradoEvent>>(c => c.Message == evento);

        var mockCollection = new Mock<IMongoCollection<MovimientoUsuarioMongo>>();
        mockCollection
            .Setup(c => c.InsertOneAsync(It.IsAny<MovimientoUsuarioMongo>(), null, default))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var mockContext = new Mock<IMongoDbContext>();
        mockContext.Setup(m => m.Movimientos).Returns(mockCollection.Object);

        var consumer = new MovimientoRegistradoConsumer(mockContext.Object);

        // Act
        await consumer.Consume(fakeContext);

        // Assert
        mockCollection.Verify(c => c.InsertOneAsync(
            It.Is<MovimientoUsuarioMongo>(m =>
                m.UsuarioId == evento.UsuarioId.ToString() &&
                m.Accion == evento.Accion &&
                m.Detalles == evento.Detalles &&
                m.FechaHora == evento.FechaHora),
            null, default), Times.Once);
    }
}