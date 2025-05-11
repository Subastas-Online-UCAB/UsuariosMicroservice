using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Aplicacion.Servicios
{
    using global::UsuarioServicio.Aplicacion.Command;
    using global::UsuarioServicio.Dominio.Entidades;
    using global::UsuarioServicio.Dominio.Events;
    using global::UsuarioServicio.Infraestructura.Persistencia;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    namespace UsuarioServicio.Aplicacion.Servicios
    {
        public class AsignarPrivilegioRolHandler : IRequestHandler<AsignarPrivilegioRolCommand, Unit>
        {
            private readonly ApplicationDbContext _context;
            private readonly IRabbitEventPublisher _rabbitPublisher;

            public AsignarPrivilegioRolHandler(ApplicationDbContext context, IRabbitEventPublisher rabbitPublisher)
            {
                _context = context;
                _rabbitPublisher = rabbitPublisher;
            }

            public async Task<Unit> Handle(AsignarPrivilegioRolCommand request, CancellationToken cancellationToken)
            {
                var asignacion = request.Asignacion;

                // Validamos que existan tanto el rol como el privilegio antes de asignar
                var rol = await _context.Roles.FindAsync(asignacion.RolId);
                var privilegio = await _context.Privilegios.FindAsync(asignacion.PrivilegioId);

                if (rol == null || privilegio == null)
                {
                    throw new Exception("El Rol o el Privilegio especificado no existen.");
                }

                // Creamos la relación
                var rolPrivilegio = new RolPrivilegio
                {
                    RolId = asignacion.RolId,
                    PrivilegioId = asignacion.PrivilegioId
                };

                var existeRelacion = await _context.RolPrivilegios
                .AnyAsync(rp => rp.RolId == request.Asignacion.RolId && rp.PrivilegioId == request.Asignacion.PrivilegioId, cancellationToken);

                if (existeRelacion)
                {
                    throw new Exception("El rol ya tiene asignado este privilegio.");
                }


                _context.RolPrivilegios.Add(rolPrivilegio);
                await _context.SaveChangesAsync(cancellationToken);

                // 🔄 Publica el evento a RabbitMQ
                await _rabbitPublisher.PublicarPrivilegioAsignadoAsync(new PrivilegioAsignadoEvent
                {
                    RolId = request.Asignacion.RolId,
                    PrivilegioId = request.Asignacion.PrivilegioId
                }, cancellationToken);

                return Unit.Value;
            }
        }
    }

}
