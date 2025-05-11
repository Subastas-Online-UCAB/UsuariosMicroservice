using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MassTransit;
using Microsoft.Extensions.Logging;
using UsuarioServicio.Dominio.Events;
using UsuarioServicio.Infraestructura.MongoDB;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;
using MongoDB.Driver;

namespace UsuarioServicio.Infraestructura.Consumers
{
    public class UsuarioActualizadoConsumer : IConsumer<UsuarioActualizadoEvent>
    {
        private readonly MongoDbContext _mongoDbContext;
        private readonly ILogger<UsuarioActualizadoConsumer> _logger;

        public UsuarioActualizadoConsumer(MongoDbContext mongoDbContext, ILogger<UsuarioActualizadoConsumer> logger)
        {
            _mongoDbContext = mongoDbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UsuarioActualizadoEvent> context)
        {
            var evento = context.Message;

            var update = Builders<UsuarioMongo>.Update
                .Set(u => u.Nombre, evento.Nombre)
                .Set(u => u.Apellido, evento.Apellido)
                .Set(u => u.Telefono, evento.Telefono)
                .Set(u => u.Direccion, evento.Direccion);

            var result = await _mongoDbContext.Usuarios.UpdateOneAsync(
                u => u.UsuarioId == evento.UsuarioId,
                update
            );

            if (result.ModifiedCount > 0)
                _logger.LogInformation($"Usuario actualizado en MongoDB: {evento.UsuarioId}");
            else
                _logger.LogWarning($"No se encontró el usuario en MongoDB: {evento.UsuarioId}");
        }
    }
}
