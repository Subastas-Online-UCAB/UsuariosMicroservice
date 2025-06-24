using MediatR;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Aplicacion.Queries;
using UsuarioServicio.Dominio.Interfaces;

namespace UsuarioServicio.Aplicacion.Servicios
{
    public class GetPrivilegiosPorRolHandler : IRequestHandler<GetPrivilegiosPorRolQuery, List<PrivilegioDTO>>
    {
        private readonly IRolPrivilegioRepository _repository;

        public GetPrivilegiosPorRolHandler(IRolPrivilegioRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<PrivilegioDTO>> Handle(GetPrivilegiosPorRolQuery request, CancellationToken cancellationToken)
        {
            var privilegios = await _repository.ObtenerPrivilegiosPorRolAsync(request.RolId, cancellationToken);

            return privilegios.Select(p => new PrivilegioDTO
            {
                Id = p.Id,
                Nombre = p.NombreTabla,
                Descripcion = p.Operacion
            }).ToList();
        }
    }
}