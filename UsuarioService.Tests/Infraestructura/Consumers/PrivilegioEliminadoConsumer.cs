using MassTransit;
using UsuarioServicio.Dominio.Events;
using UsuarioServicio.Infraestructura.MongoDB;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Threading;


    public class PrivilegioEliminadoConsumer : IConsumer<PrivilegioEliminadoEvent>
    {
        private readonly IMongoDbContext _mongoDbContext;

        public PrivilegioEliminadoConsumer(IMongoDbContext mongoDbContext)
        {
            _mongoDbContext = mongoDbContext;
        }

        public async Task Consume(ConsumeContext<PrivilegioEliminadoEvent> context)
        {
            var mensaje = context.Message;

            var filter = Builders<RolPrivilegioMongo>.Filter.And(
                Builders<RolPrivilegioMongo>.Filter.Eq(x => x.RolId, mensaje.RolId.ToString()),
                Builders<RolPrivilegioMongo>.Filter.Eq(x => x.PrivilegioId, mensaje.PrivilegioId.ToString())
            );

            await _mongoDbContext.RolesPrivilegios.DeleteOneAsync(filter, cancellationToken: context.CancellationToken);
        }
    }
