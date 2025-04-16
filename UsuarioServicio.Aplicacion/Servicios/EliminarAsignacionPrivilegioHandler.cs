using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;


namespace UsuarioServicio.Aplicacion.Servicios
{
    public class EliminarAsignacionPrivilegioHandler : IRequestHandler<EliminarAsignacionPrivilegioCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public EliminarAsignacionPrivilegioHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(EliminarAsignacionPrivilegioCommand request, CancellationToken cancellationToken)
        {
            var asignacion = await _context.RolPrivilegios
                .FirstOrDefaultAsync(rp => rp.RolId == request.RolId && rp.PrivilegioId == request.PrivilegioId, cancellationToken);

            if (asignacion == null)
                throw new Exception("La asignación no existe.");

            _context.RolPrivilegios.Remove(asignacion);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }

}
