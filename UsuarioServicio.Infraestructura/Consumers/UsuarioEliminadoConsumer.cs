using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsuarioServicio.Dominio.Events;
using UsuarioServicio.Infraestructura.MongoDB;
using MassTransit;
using Microsoft.Extensions.Logging;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;
using MongoDB.Driver;

namespace UsuarioServicio.Infraestructura.Consumers
{
    public class UsuarioEliminadoConsumer : IConsumer<UsuarioEliminadoEvent>
{
    private readonly IMongoDbContext _mongoDbContext;
    private readonly ILogger<UsuarioEliminadoConsumer> _logger;

    public UsuarioEliminadoConsumer(IMongoDbContext mongoDbContext, ILogger<UsuarioEliminadoConsumer> logger)
    {
        _mongoDbContext = mongoDbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UsuarioEliminadoEvent> context)
    {
        var email = context.Message.Email;

        var filtro = Builders<UsuarioMongo>.Filter.Eq(u => u.Email, email);
        var result = await _mongoDbContext.Usuarios.DeleteOneAsync(filtro);

        if (result.DeletedCount > 0)
            _logger.LogInformation($"Usuario eliminado: {email}");
        else
            _logger.LogWarning($"No se encontró el usuario a eliminar: {email}");
    }
}

}

