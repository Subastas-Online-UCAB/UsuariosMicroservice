using MediatR;
using UsuarioServicio.Aplicacion.Queries;
using UsuarioServicio.Dominio.DTOs;
using UsuarioServicio.Dominio.Interfaces;

namespace UsuarioServicio.Aplicacion.Servicios
{
    public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, List<UsuarioMongoDto>>
    {
        private readonly IUsuarioMongoRepository _repository;

        public GetAllUsersHandler(IUsuarioMongoRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UsuarioMongoDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var usuarios = await _repository.ObtenerTodosAsync(cancellationToken);
            return usuarios;
        }
    }
}