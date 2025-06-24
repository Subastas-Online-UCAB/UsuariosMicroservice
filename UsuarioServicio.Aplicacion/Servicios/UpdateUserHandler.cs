using MediatR;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Dominio.Excepciones;
using UsuarioServicio.Dominio.Interfaces;
using UsuarioServicio.Dominio.Entidades;

namespace UsuarioServicio.Aplicacion.Servicios
{
    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, bool>
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IKeycloakAccountService _keycloakService;
        private readonly IRabbitEventPublisher _rabbitPublisher;

        public UpdateUserHandler(
            IUsuarioRepository usuarioRepository,
            IKeycloakAccountService keycloakService,
            IRabbitEventPublisher rabbitPublisher)
        {
            _usuarioRepository = usuarioRepository;
            _keycloakService = keycloakService;
            _rabbitPublisher = rabbitPublisher;
        }

        public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _usuarioRepository.ObtenerPorEmailAsync(request.Email, cancellationToken);
            if (usuario == null)
                throw new UsuarioNoEncontradoException(request.Email);

            // Detectar cambios que deben sincronizarse con Keycloak
            bool nombreCambia = usuario.Nombre != request.Nombre;
            bool apellidoCambia = usuario.Apellido != request.Apellido;

            // Actualizar entidad
            usuario.Nombre = request.Nombre;
            usuario.Apellido = request.Apellido;
            usuario.Telefono = request.Telefono;
            usuario.Direccion = request.Direccion;

            await _usuarioRepository.ActualizarAsync(usuario, cancellationToken);

            // Sincronizar con Keycloak solo si es necesario
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
