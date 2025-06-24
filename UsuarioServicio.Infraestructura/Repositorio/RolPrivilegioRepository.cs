using Microsoft.EntityFrameworkCore;
using UsuarioServicio.Dominio.DTOs;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Dominio.Interfaces;
using UsuarioServicio.Infraestructura.Persistencia;

namespace UsuarioServicio.Infraestructura.Repositorio
{
    public class RolPrivilegioRepository : IRolPrivilegioRepository
    {
        private readonly ApplicationDbContext _context;

        public RolPrivilegioRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RolPrivilegio?> ObtenerAsignacionAsync(Guid rolId, Guid privilegioId, CancellationToken cancellationToken)
        {
            return await _context.RolPrivilegios
                .FirstOrDefaultAsync(rp => rp.RolId == rolId && rp.PrivilegioId == privilegioId, cancellationToken);
        }

        public async Task EliminarAsignacionAsync(RolPrivilegio asignacion, CancellationToken cancellationToken)
        {
            _context.RolPrivilegios.Remove(asignacion);
            await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task<List<Privilegio>> ObtenerPrivilegiosPorRolAsync(Guid rolId, CancellationToken cancellationToken)
        {
            return await _context.RolPrivilegios
                .Where(rp => rp.RolId == rolId)
                .Include(rp => rp.Privilegio)
                .Select(rp => rp.Privilegio)
                .ToListAsync(cancellationToken);
        }

    }
}