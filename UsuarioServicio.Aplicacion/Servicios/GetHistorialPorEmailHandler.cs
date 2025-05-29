using MediatR;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Aplicacion.Queries;
using UsuarioServicio.Aplicacion.Interfaces;
using UsuarioServicio.Dominio.DTOs;
using UsuarioServicio.Dominio.Interfaces;

namespace UsuarioServicio.Aplicacion.Servicios
{
    public class GetHistorialPorEmailHandler : IRequestHandler<GetHistorialPorEmailQuery, List<MovimientoMongoDto>>
    {
        private readonly IHistorialMovimientoRepository _repository;

        public GetHistorialPorEmailHandler(IHistorialMovimientoRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<MovimientoMongoDto>> Handle(GetHistorialPorEmailQuery request, CancellationToken cancellationToken)
        {
            var historial = await _repository.ObtenerPorEmailAsync(request.Email, cancellationToken);

            return historial.Select(m => new MovimientoMongoDto
            {
                Accion = m.Accion,
                FechaHora = m.FechaHora,
                Detalles = m.Detalles
            }).ToList();
        }
    }

}

