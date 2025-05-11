using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MediatR;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Dominio.Interfaces;

namespace UsuarioServicio.Aplicacion.Services
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Guid>
    {
        private readonly IKeycloakService _keycloakService;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IRabbitEventPublisher _eventPublisher;

        public RegisterUserHandler(
            IKeycloakService keycloakService,
            IUsuarioRepository usuarioRepository,
            IRabbitEventPublisher eventPublisher)
        {
            _keycloakService = keycloakService;
            _usuarioRepository = usuarioRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // 1. Validar rol
            var rol = await _usuarioRepository.ObtenerRolPorIdAsync(request.RolId, cancellationToken);
            if (rol == null)
                throw new Exception("El rol especificado no existe.");


            

            // 2. Validar duplicidad de correo
            var existe = await _usuarioRepository.EmailExisteAsync(request.Email, cancellationToken);
            if (existe)
                throw new Exception("El correo electrónico ya está registrado.");

            // 3. Crear usuario en Keycloak
            var keycloakUserId = await _keycloakService.RegistrarUsuarioAsync(
                request.Nombre, request.Apellido, request.Email, request.Password, cancellationToken);

            // 4. Asignar rol en Keycloak
            await _keycloakService.AsignarRolAsync(keycloakUserId, rol.Nombre, cancellationToken);

            // 5. Crear usuario en base de datos
            var usuario = Usuario.Crear(
                request.Nombre,
                request.Apellido,
                request.Email,
                request.Password,
                request.Telefono,
                request.Direccion,
                request.RolId
            );

            await _usuarioRepository.GuardarUsuarioAsync(usuario, cancellationToken);

            // 6. Publicar evento a RabbitMQ
            await _eventPublisher.PublicarUsuarioCreadoAsync(usuario, cancellationToken);

            return usuario.Id;
        }
    }
}
