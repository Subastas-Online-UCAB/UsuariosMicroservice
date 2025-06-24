using MediatR;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Dominio.Events;
using UsuarioServicio.Dominio.Excepciones;
using UsuarioServicio.Dominio.Interfaces;

namespace UsuarioServicio.Aplicacion.Servicios
{
    public class RegistrarMovimientoHandler : IRequestHandler<RegistrarMovimientoCommand, string>
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IRabbitEventPublisher _eventPublisher;

        public RegistrarMovimientoHandler(
            IUsuarioRepository usuarioRepository,
            IRabbitEventPublisher eventPublisher)
        {
            _usuarioRepository = usuarioRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<string> Handle(RegistrarMovimientoCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _usuarioRepository.ObtenerPorEmailAsync(request.Email, cancellationToken);
            if (usuario == null)
                throw new UsuarioNoEncontradoException(request.Email);

            var evento = new MovimientoRegistradoEvent
            {
                UsuarioId = usuario.Id,
                Accion = request.Accion,
                Detalles = request.Detalles,
                FechaHora = DateTime.UtcNow
            };

            await _eventPublisher.PublicarEventoAsync(evento, cancellationToken);

            return "Movimiento enviado correctamente para ser procesado.";
        }
    }
}