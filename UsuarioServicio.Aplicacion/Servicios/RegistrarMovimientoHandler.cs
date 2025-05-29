using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Dominio.Events;
using UsuarioServicio.Infraestructura.Persistencia;

namespace UsuarioServicio.Aplicacion.Servicios
{
    public class RegistrarMovimientoHandler : IRequestHandler<RegistrarMovimientoCommand, string>
    {
        private readonly ApplicationDbContext _context;
        private readonly IRabbitEventPublisher _eventPublisher;

        public RegistrarMovimientoHandler(ApplicationDbContext context, IRabbitEventPublisher eventPublisher)
        {
            _context = context;
            _eventPublisher = eventPublisher;
        }

        public async Task<string> Handle(RegistrarMovimientoCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (usuario == null)
                throw new Exception("Usuario no encontrado.");

            var movimiento = new MovimientoUsuario
            {
                UsuarioId = usuario.Id,
                Accion = request.Accion,
                Detalles = request.Detalles,
                FechaHora = DateTime.UtcNow
            };

            _context.MovimientosUsuario.Add(movimiento);
            await _context.SaveChangesAsync(cancellationToken);

            await _eventPublisher.PublicarEventoAsync(
                new MovimientoRegistradoEvent
                {
                    UsuarioId = usuario.Id,
                    Accion = request.Accion,
                    Detalles = request.Detalles,
                    FechaHora = DateTime.UtcNow
                },
                cancellationToken);

            return "Movimiento registrado correctamente.";
        }
    }
}
