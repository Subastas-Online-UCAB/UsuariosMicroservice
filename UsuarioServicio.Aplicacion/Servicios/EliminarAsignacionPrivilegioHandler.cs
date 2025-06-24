using MediatR;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Dominio.Excepciones;
using UsuarioServicio.Dominio.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace UsuarioServicio.Aplicacion.Servicios
{
    public class EliminarAsignacionPrivilegioHandler : IRequestHandler<EliminarAsignacionPrivilegioCommand, bool>
    {
        private readonly IRolPrivilegioRepository _repository;
        private readonly IRabbitEventPublisher _eventPublisher;

        public EliminarAsignacionPrivilegioHandler(IRolPrivilegioRepository repository, IRabbitEventPublisher eventPublisher)
        {
            _repository = repository;
            _eventPublisher = eventPublisher;
        }

        public async Task<bool> Handle(EliminarAsignacionPrivilegioCommand request, CancellationToken cancellationToken)
        {
            var asignacion = await _repository.ObtenerAsignacionAsync(request.RolId, request.PrivilegioId, cancellationToken);

            if (asignacion == null)
                throw new AsignacionNoEncontradaException(request.RolId, request.PrivilegioId);

            await _repository.EliminarAsignacionAsync(asignacion, cancellationToken);

            await _eventPublisher.PublicarPrivilegioEliminadoAsync(
                request.RolId.ToString(),
                request.PrivilegioId.ToString(),
                cancellationToken
            );

            return true;
        }
    }
}