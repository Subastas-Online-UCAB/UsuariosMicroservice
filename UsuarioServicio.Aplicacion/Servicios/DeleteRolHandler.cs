using MediatR;
using Microsoft.EntityFrameworkCore;
using UsuarioServicio.Aplicacion.Commands;
using UsuarioServicio.Infraestructura.Services;
using UsuarioServicio.Infraestructura.Persistencia;
using UsuarioServicio.Aplicacion.Command;

namespace UsuarioServicio.Aplicacion.Handlers
{
    public class DeleteRolHandler : IRequestHandler<DeleteRolCommand, bool>
    {
        private readonly ApplicationDbContext _context;
        private readonly KeycloakService _keycloakService;

        public DeleteRolHandler(ApplicationDbContext context, KeycloakService keycloakService)
        {
            _context = context;
            _keycloakService = keycloakService;
        }

        public async Task<bool> Handle(DeleteRolCommand request, CancellationToken cancellationToken)
        {
            // ✅ Buscar el rol
            var rol = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == request.RolId, cancellationToken);

            if (rol == null)
                throw new Exception("El rol no existe.");

            // ✅ Verificar si hay usuarios con ese rol
            var usuariosConRol = await _context.Usuarios
                .AnyAsync(u => u.RolId == rol.Id, cancellationToken);

            if (usuariosConRol)
                throw new Exception("No se puede eliminar un rol asignado a usuarios.");

            // ✅ Eliminar de Keycloak primero
            await _keycloakService.DeleteRoleAsync(rol.Nombre, cancellationToken);

            // ✅ Eliminar de la base de datos
            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
