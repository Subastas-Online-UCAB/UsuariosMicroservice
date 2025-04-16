using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using Microsoft.EntityFrameworkCore;
using UsuarioServicio.Infraestructura.Persistencia;
using UsuarioServicio.Aplicacion.Commands;

namespace UsuarioServicio.Aplicacion.Servicios
{
    public class DeletePrivilegioHandler : IRequestHandler<DeletePrivilegioCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public DeletePrivilegioHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeletePrivilegioCommand request, CancellationToken cancellationToken)
        {
            var privilegio = await _context.Privilegios
                .Include(p => p.RolPrivilegios)
                .FirstOrDefaultAsync(p => p.Id == request.PrivilegioId, cancellationToken);

            if (privilegio == null)
                throw new Exception("Privilegio no encontrado.");

            // 🚫 Validación: no permitir eliminar si tiene roles asignados
            if (privilegio.RolPrivilegios.Any())
                throw new Exception("No se puede eliminar el privilegio porque está asignado a uno o más roles.");

            _context.Privilegios.Remove(privilegio);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

