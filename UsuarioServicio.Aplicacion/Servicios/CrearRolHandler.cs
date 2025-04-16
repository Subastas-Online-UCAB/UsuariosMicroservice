using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using Microsoft.EntityFrameworkCore;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Infraestructura.Persistencia;
using UsuarioServicio.Infraestructura.Services;

namespace UsuarioServicio.Aplicacion.Servicios
{
    public class CrearRolHandler : IRequestHandler<CrearRolCommand, Guid>
    {
        private readonly ApplicationDbContext _context;
        private readonly KeycloakService _keycloakService;

        public CrearRolHandler(ApplicationDbContext context, KeycloakService keycloakService)
        {
            _context = context;
            _keycloakService = keycloakService;
        }

        public async Task<Guid> Handle(CrearRolCommand request, CancellationToken cancellationToken)
        {
            // ✅ Validación: Nombre no vacío
            if (string.IsNullOrWhiteSpace(request.Rol.Nombre))
            {
                throw new ArgumentException("El nombre del rol no puede estar vacío.");
            }

            // ✅ Validación: Rol no duplicado en la BD
            var rolExistente = await _context.Roles
                .AnyAsync(r => r.Nombre == request.Rol.Nombre, cancellationToken);

            if (rolExistente)
            {
                throw new Exception("Ya existe un rol con ese nombre.");
            }

            // ✅ Primero crea el rol en Keycloak
            await _keycloakService.CreateRoleAsync(request.Rol.Nombre, cancellationToken);

            // ✅ Luego crea el rol en la base de datos
            var rol = new Rol
            {
                Id = Guid.NewGuid(),
                Nombre = request.Rol.Nombre,
                Descripcion = request.Rol.Descripcion
            };

            _context.Roles.Add(rol);
            await _context.SaveChangesAsync(cancellationToken);

            return rol.Id;
        }
    }


}

