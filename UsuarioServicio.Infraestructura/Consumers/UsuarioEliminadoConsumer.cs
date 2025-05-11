using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsuarioServicio.Dominio.Events;
using UsuarioServicio.Infraestructura.MongoDB;
using MassTransit;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;
using MongoDB.Driver;

namespace UsuarioServicio.Infraestructura.Consumers
{
    public class UsuarioEliminadoConsumer : IConsumer<UsuarioEliminadoEvent>
    {
        private readonly MongoDbContext _mongoDbContext;

        public UsuarioEliminadoConsumer(MongoDbContext mongoDbContext)
        {
            _mongoDbContext = mongoDbContext;
        }

        public async Task Consume(ConsumeContext<UsuarioEliminadoEvent> context)
        {
            var email = context.Message.Email;

            var filtro = Builders<UsuarioMongo>.Filter.Eq(u => u.Email, email);
            await _mongoDbContext.Usuarios.DeleteOneAsync(filtro);
        }
    }
}

