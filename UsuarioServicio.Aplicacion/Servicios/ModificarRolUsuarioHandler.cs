using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using Microsoft.EntityFrameworkCore;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Infraestructura.Persistencia;
using UsuarioServicio.Infraestructura.Services;

namespace UsuarioServicio.Aplicacion.Servicios
{
    public class ModificarRolUsuarioHandler : IRequestHandler<ModificarRolUsuarioCommand, bool>
    {
        private readonly ApplicationDbContext _context;
        private readonly KeycloakService _keycloakService;

        public ModificarRolUsuarioHandler(ApplicationDbContext context, KeycloakService keycloakService)
        {
            _context = context;
            _keycloakService = keycloakService;
        }

        public async Task<bool> Handle(ModificarRolUsuarioCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (usuario == null)
                throw new Exception("Usuario no encontrado.");

            var nuevoRol = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == request.NuevoRolId, cancellationToken);

            if (nuevoRol == null)
                throw new Exception("El nuevo rol no existe.");

            usuario.RolId = request.NuevoRolId;

            await _context.SaveChangesAsync(cancellationToken);

            // 🔐 También actualizar el rol en Keycloak
            await _keycloakService.AsignarRolAsync(request.Email, nuevoRol.Nombre, cancellationToken);

            return true;
        }
    }

}
