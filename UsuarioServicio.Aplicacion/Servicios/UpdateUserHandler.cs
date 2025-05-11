using MediatR;
using Microsoft.EntityFrameworkCore;
using UsuarioServicio.Aplicacion.Commands;
using UsuarioServicio.Infraestructura.Persistencia;
using UsuarioServicio.Infraestructura.Services;
using UsuarioServicio.Infraestructura.Eventos;


namespace UsuarioServicio.Aplicacion.Handlers
{
    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, bool>
    {
        private readonly ApplicationDbContext _context;
        private readonly KeycloakService _keycloakService;
        private readonly IRabbitEventPublisher _rabbitPublisher;

        public UpdateUserHandler(ApplicationDbContext context, KeycloakService keycloakService, IRabbitEventPublisher rabbitPublisher)
        {
            _context = context;
            _keycloakService = keycloakService;
            _rabbitPublisher = rabbitPublisher;
        }

        public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (usuario == null)
                throw new Exception("Usuario no encontrado.");

            // Verificar si hay cambios en los campos sincronizados con Keycloak
            bool nombreCambia = usuario.Nombre != request.Nombre;
            bool apellidoCambia = usuario.Apellido != request.Apellido;

            // Actualizar datos en la base de datos
            usuario.Nombre = request.Nombre;
            usuario.Apellido = request.Apellido;
            usuario.Telefono = request.Telefono;
            usuario.Direccion = request.Direccion;

            await _context.SaveChangesAsync(cancellationToken);

            // Solo llamar a Keycloak si Nombre o Apellido cambian
            if (nombreCambia || apellidoCambia)
            {
                await _keycloakService.UpdateUserAsync(
                    request.Email,
                    request.Nombre,
                    request.Apellido,
                    cancellationToken
                );
            }

            await _rabbitPublisher.PublicarUsuarioActualizadoAsync(usuario, cancellationToken);


            return true;
        }
    }
}
