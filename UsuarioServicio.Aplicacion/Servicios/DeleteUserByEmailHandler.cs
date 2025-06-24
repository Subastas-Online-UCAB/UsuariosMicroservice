using MediatR;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Dominio.Excepciones;
using UsuarioServicio.Dominio.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace UsuarioServicio.Aplicacion.Servicios
{
    public class DeleteUserByEmailHandler : IRequestHandler<DeleteUserByEmailCommand, string>
    {
        private readonly IUsuarioMongoRepository _mongoReader;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IKeycloakAccountService _keycloakService;
        private readonly IRabbitEventPublisher _eventPublisher;

        public DeleteUserByEmailHandler(
            IUsuarioMongoRepository mongoReader,
            IUsuarioRepository usuarioRepository,
            IKeycloakAccountService keycloakService,
            IRabbitEventPublisher eventPublisher)
        {
            _mongoReader = mongoReader;
            _usuarioRepository = usuarioRepository;
            _keycloakService = keycloakService;
            _eventPublisher = eventPublisher;
        }

        public async Task<string> Handle(DeleteUserByEmailCommand request, CancellationToken cancellationToken)
        {
            // 1. Verificar si existe en Mongo
            var usuarioDto = await _mongoReader.ObtenerPorEmailAsync(request.Email, cancellationToken);
            if (usuarioDto == null)
                throw new UsuarioNoEncontradoException($"No se encontró un usuario con el correo: {request.Email}");

            // 2. Buscar en PostgreSQL para que EF pueda eliminarlo
            var usuarioPostgres = await _usuarioRepository.ObtenerPorIdAsync(usuarioDto.Id, cancellationToken);
            if (usuarioPostgres == null)
                throw new UsuarioNoEncontradoEnPostgresException(usuarioDto.Id);

            // 3. Eliminar en Keycloak si existe
            var userId = await _keycloakService.GetUserIdByEmail(request.Email, cancellationToken);
            if (userId != null)
                await _keycloakService.DeleteUser(userId, cancellationToken);

            // 4. Eliminar desde PostgreSQL
            await _usuarioRepository.EliminarAsync(usuarioPostgres, cancellationToken);

            // 5. Publicar evento
            await _eventPublisher.PublicarUsuarioEliminadoAsync(usuarioPostgres.Id, request.Email, cancellationToken);

            return $"Usuario con correo {request.Email} eliminado exitosamente.";
        }
    }
}
