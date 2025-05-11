using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MassTransit;
using MongoDB.Bson;
using MongoDB.Driver;
using UsuarioServicio.Dominio.Events;
using UsuarioServicio.Infraestructura.MongoDB;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;

public class PrivilegioEliminadoConsumer : IConsumer<PrivilegioEliminadoEvent>
{
    private readonly MongoDbContext _mongo;

    public PrivilegioEliminadoConsumer(MongoDbContext mongo)
    {
        _mongo = mongo;
    }

    public async Task Consume(ConsumeContext<PrivilegioEliminadoEvent> context)
    {
        var filtro = Builders<RolPrivilegioMongo>.Filter.Eq(r => r.RolId, context.Message.RolId) &
             Builders<RolPrivilegioMongo>.Filter.Eq(r => r.PrivilegioId, context.Message.PrivilegioId);

        await _mongo.RolesPrivilegios.DeleteOneAsync(filtro);

    }
}
