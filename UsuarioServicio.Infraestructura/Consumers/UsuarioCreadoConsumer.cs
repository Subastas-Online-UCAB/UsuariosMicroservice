using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using UsuarioServicio.Dominio.Events;
using UsuarioServicio.Dominio.Interfaces;
using UsuarioServicio.Infraestructura.MongoDB;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;

namespace UsuarioServicio.Infraestructura.Consumers
{
    public class UsuarioCreadoConsumer : IConsumer<UsuarioCreadoEvent>
    {
        private readonly MongoDbContext _context;

        public UsuarioCreadoConsumer(MongoDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<UsuarioCreadoEvent> context)
        {
            try
            {
                var mensaje = context.Message;

                Console.WriteLine(" Evento recibido en el consumer:");
                Console.WriteLine($" Usuario ID: {mensaje.UsuarioId}");
                Console.WriteLine($" Nombre: {mensaje.Nombre} {mensaje.Apellido}");
                Console.WriteLine($" Email: {mensaje.Email}");

                var usuarioMongo = new UsuarioMongo
                {
                    UsuarioId = mensaje.UsuarioId,
                    Nombre = mensaje.Nombre,
                    Apellido = mensaje.Apellido,
                    Email = mensaje.Email,
                    FechaCreacion = mensaje.FechaCreacion,
                    Telefono = mensaje.Telefono,
                    Direccion = mensaje.Direccion,
                    RolId = mensaje.RolId.ToString(),
                };

                await _context.Usuarios.InsertOneAsync(usuarioMongo);
                Console.WriteLine("✅ Insertado en MongoDB correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en el consumidor:");
                Console.WriteLine(ex.ToString());
                throw; // importante: relanza para que MassTransit lo sepa
            }
        }
    }

}

