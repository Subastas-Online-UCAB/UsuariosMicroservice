using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsuarioServicio.Dominio.Events;
using UsuarioServicio.Infraestructura.MongoDB;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;

namespace UsuarioServicio.Infraestructura.Consumers
{
    public class PrivilegioAsignadoConsumer : IConsumer<PrivilegioAsignadoEvent>
    {
        private readonly MongoDbContext _mongo;

        public PrivilegioAsignadoConsumer(MongoDbContext mongo)
        {
            _mongo = mongo;
        }

        public async Task Consume(ConsumeContext<PrivilegioAsignadoEvent> context)
        {
            var doc = new RolPrivilegioMongo
            {
                RolId = context.Message.RolId.ToString(),
                PrivilegioId = context.Message.PrivilegioId.ToString(),
            };

            await _mongo.RolesPrivilegios.InsertOneAsync(doc);
        }
    }

}
