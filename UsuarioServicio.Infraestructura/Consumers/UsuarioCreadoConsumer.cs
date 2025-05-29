using System;
using System.Threading.Tasks;
using MassTransit;
using UsuarioServicio.Dominio.Events;
using UsuarioServicio.Infraestructura.MongoDB;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;
using Microsoft.Extensions.Logging;

namespace UsuarioServicio.Infraestructura.Consumers
{
    public class UsuarioCreadoConsumer : IConsumer<UsuarioCreadoEvent>
    {
        private readonly IMongoDbContext _context;
        private readonly ILogger<UsuarioCreadoConsumer> _logger;

        public UsuarioCreadoConsumer(IMongoDbContext context, ILogger<UsuarioCreadoConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UsuarioCreadoEvent> context)
        {
            try
            {
                var mensaje = context.Message;

                _logger.LogInformation("📩 Evento recibido: UsuarioCreado");
                _logger.LogInformation("Usuario ID: {UsuarioId} | Nombre: {Nombre} {Apellido} | Email: {Email}",
                    mensaje.UsuarioId, mensaje.Nombre, mensaje.Apellido, mensaje.Email);

                var usuarioMongo = new UsuarioMongo
                {
                    UsuarioId = mensaje.UsuarioId,
                    Nombre = mensaje.Nombre,
                    Apellido = mensaje.Apellido,
                    Email = mensaje.Email,
                    FechaCreacion = mensaje.FechaCreacion,
                    Telefono = mensaje.Telefono,
                    Direccion = mensaje.Direccion,
                    RolId = mensaje.RolId.ToString()
                };

                await _context.Usuarios.InsertOneAsync(usuarioMongo);

                _logger.LogInformation("✅ Usuario insertado en MongoDB correctamente: {Email}", mensaje.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al insertar usuario en MongoDB: {Error}", ex.Message);
                throw; // importante: relanza para que MassTransit lo maneje
            }
        }
    }
}
