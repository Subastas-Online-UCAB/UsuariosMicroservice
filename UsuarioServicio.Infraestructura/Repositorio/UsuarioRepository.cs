using Microsoft.EntityFrameworkCore;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Dominio.Interfaces;

namespace UsuarioServicio.Infraestructura.Persistencia.Repositorio
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ApplicationDbContext _context;

        public UsuarioRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Rol?> ObtenerRolPorIdAsync(Guid rolId, CancellationToken cancellationToken)
        {
            var rol = await _context.Roles.FindAsync(new object[] { rolId }, cancellationToken);
            if (rol == null)
                throw new Exception("El rol especificado no existe.");
            return rol;
        }

        public async Task<bool> EmailExisteAsync(string email, CancellationToken cancellationToken)
        {
            var existe = await _context.Usuarios.AnyAsync(u => u.Email == email, cancellationToken);
            if (existe)
                throw new Exception("El correo electrónico ya está registrado.");
            return false;
        }

        public async Task GuardarUsuarioAsync(Usuario usuario, CancellationToken cancellationToken)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
