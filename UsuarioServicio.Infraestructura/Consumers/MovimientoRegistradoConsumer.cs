using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Dominio.Events;
using UsuarioServicio.Infraestructura.MongoDB;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;

namespace UsuarioServicio.Infraestructura.Consumers
{
    public class MovimientoRegistradoConsumer : IConsumer<MovimientoRegistradoEvent>
    {
        private readonly IMongoDbContext _mongoDbContext;

        public MovimientoRegistradoConsumer(IMongoDbContext mongoDbContext)
        {
            _mongoDbContext = mongoDbContext;
        }

        public async Task Consume(ConsumeContext<MovimientoRegistradoEvent> context)
        {
            Console.WriteLine(" Consumer triggered!");
            var mensaje = context.Message;

            var documento = new MovimientoUsuarioMongo
            {
                UsuarioId = context.Message.UsuarioId.ToString(),
                Accion = mensaje.Accion,
                Detalles = mensaje.Detalles,
                FechaHora = mensaje.FechaHora
            };

            await _mongoDbContext.Movimientos.InsertOneAsync(documento);
        }
    }
}
