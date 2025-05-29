using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsuarioServicio.Dominio.Events;
using UsuarioServicio.Infraestructura.Consumers;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;
using UsuarioServicio.Infraestructura.MongoDB;

    public class UsuarioCreadoConsumerTests
    {
        [Fact]
        public async Task Consume_Should_Insert_UsuarioMongo_And_Log_Info()
        {
            // Arrange
            var evento = new UsuarioCreadoEvent
            {
                UsuarioId = Guid.NewGuid(),
                Nombre = "Miguel",
                Apellido = "Garcia",
                Email = "miguel@example.com",
                FechaCreacion = DateTime.UtcNow,
                Telefono = "1234567890",
                Direccion = "Calle 123",
                RolId = Guid.NewGuid()
            };

            var fakeContext = Mock.Of<ConsumeContext<UsuarioCreadoEvent>>(c => c.Message == evento);

            var mockCollection = new Mock<IMongoCollection<UsuarioMongo>>();
            mockCollection
                .Setup(c => c.InsertOneAsync(It.IsAny<UsuarioMongo>(), null, default))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var mockDbContext = new Mock<IMongoDbContext>();
            mockDbContext.Setup(m => m.Usuarios).Returns(mockCollection.Object);

            var mockLogger = new Mock<ILogger<UsuarioCreadoConsumer>>();

            var consumer = new UsuarioCreadoConsumer(mockDbContext.Object, mockLogger.Object);

            // Act
            await consumer.Consume(fakeContext);

            // Assert
            mockCollection.Verify(c => c.InsertOneAsync(
                It.Is<UsuarioMongo>(u =>
                    u.UsuarioId == evento.UsuarioId &&
                    u.Nombre == evento.Nombre &&
                    u.Apellido == evento.Apellido &&
                    u.Email == evento.Email &&
                    u.Telefono == evento.Telefono &&
                    u.Direccion == evento.Direccion &&
                    u.RolId == evento.RolId.ToString()
                ),
                null,
                default), Times.Once);

            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Usuario insertado")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }


    [Fact]
    public async Task Consume_Should_Throw_When_Insert_Fails()
    {
        // Arrange
        var evento = new UsuarioCreadoEvent
        {
            UsuarioId = Guid.NewGuid(),
            Nombre = "Error",
            Apellido = "Simulado",
            Email = "error@example.com",
            FechaCreacion = DateTime.UtcNow,
            Telefono = "0000000000",
            Direccion = "Zona fallida",
            RolId = Guid.NewGuid()
        };

        var fakeContext = Mock.Of<ConsumeContext<UsuarioCreadoEvent>>(c => c.Message == evento);

        var mockCollection = new Mock<IMongoCollection<UsuarioMongo>>();
        mockCollection
            .Setup(c => c.InsertOneAsync(It.IsAny<UsuarioMongo>(), null, default))
            .ThrowsAsync(new Exception("Fallo de prueba en MongoDB"));

        var mockDbContext = new Mock<IMongoDbContext>();
        mockDbContext.Setup(m => m.Usuarios).Returns(mockCollection.Object);

        var mockLogger = new Mock<ILogger<UsuarioCreadoConsumer>>();

        var consumer = new UsuarioCreadoConsumer(mockDbContext.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => consumer.Consume(fakeContext));

        mockLogger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error al insertar")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }


}

