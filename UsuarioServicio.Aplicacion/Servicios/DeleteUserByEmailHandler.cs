using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using Microsoft.EntityFrameworkCore;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Dominio.Interfaces;
using UsuarioServicio.Infraestructura.Persistencia;
using UsuarioServicio.Infraestructura.Services;

namespace UsuarioServicio.Aplicacion.Servicios
{
    public class DeleteUserByEmailHandler : IRequestHandler<DeleteUserByEmailCommand, string>
    {
        private readonly ApplicationDbContext _context;
        private readonly IKeycloakAccountService _keycloakService;
        private readonly IRabbitEventPublisher _eventPublisher;

        public DeleteUserByEmailHandler(ApplicationDbContext context, IKeycloakAccountService keycloakService, IRabbitEventPublisher eventPublisher)
        {
            _context = context;
            _keycloakService = keycloakService;
            _eventPublisher = eventPublisher;
        }

        public async Task<string> Handle(DeleteUserByEmailCommand request, CancellationToken cancellationToken)
        {
            // 1. Verifica existencia en base de datos
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
            if (usuario == null)
            {
                throw new Exception($"No se encontró un usuario con el correo: {request.Email}");
            }

            // 2. Elimina usuario en Keycloak
            var userId = await _keycloakService.GetUserIdByEmail(request.Email, cancellationToken);
            if (userId != null)
            {
                await _keycloakService.DeleteUser(userId, cancellationToken);
            }

            // 3. Elimina usuario en base de datos
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync(cancellationToken);

            // 4. Publica evento para eliminar en Mongo
            await _eventPublisher.PublicarUsuarioEliminadoAsync(usuario.Id, usuario.Email, cancellationToken);

            return $"Usuario con correo {request.Email} eliminado exitosamente.";
        }
    }
}
