using MassTransit;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Dominio.Events;

namespace UsuarioServicio.Infraestructura.Eventos
{
    public class RabbitEventPublisher : IRabbitEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public RabbitEventPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task PublicarUsuarioCreadoAsync(Usuario usuario, CancellationToken cancellationToken)
        {
            var evento = new UsuarioCreadoEvent
            {
                UsuarioId = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                FechaCreacion = usuario.FechaCreacion,
                Telefono = usuario.Telefono,
                Direccion = usuario.Direccion,
                RolId = usuario.RolId,
            };

            await _publishEndpoint.Publish(evento, cancellationToken);
        }

        public async Task PublicarUsuarioActualizadoAsync(Usuario usuario, CancellationToken cancellationToken)
        {
            var evento = new UsuarioActualizadoEvent
            {
                UsuarioId = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Telefono = usuario.Telefono,
                Direccion = usuario.Direccion,
                FechaActualizacion = DateTime.UtcNow
            };

            await _publishEndpoint.Publish(evento, cancellationToken);
        }

        public async Task PublicarUsuarioEliminadoAsync(Guid usuarioId, string email, CancellationToken cancellationToken)
        {
            var evento = new UsuarioEliminadoEvent
            {
                UsuarioId = usuarioId,
                Email = email
            };

            await _publishEndpoint.Publish(evento, cancellationToken);
        }

        public async Task PublicarPrivilegioAsignadoAsync(PrivilegioAsignadoEvent evento, CancellationToken cancellationToken)
        {
            await _publishEndpoint.Publish(evento, cancellationToken);
        }

        public async Task PublicarPrivilegioEliminadoAsync(string rolId, string privilegioId, CancellationToken cancellationToken)
        {
            var evento = new PrivilegioEliminadoEvent
            {
                RolId = rolId,
                PrivilegioId = privilegioId
            };

            await _publishEndpoint.Publish(evento, cancellationToken);
        }

        public async Task PublicarEventoAsync<T>(T evento, CancellationToken cancellationToken)
        {
            await _publishEndpoint.Publish(evento, cancellationToken);
        }


    }
}
