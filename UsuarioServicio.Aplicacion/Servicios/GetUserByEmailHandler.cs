using MediatR;
using System.Threading;
using System.Threading.Tasks;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Aplicacion.Queries;
using UsuarioServicio.Dominio.DTOs;
using UsuarioServicio.Dominio.Interfaces;

namespace UsuarioServicio.Aplicacion.Servicios
{
    public class GetUserByEmailHandler : IRequestHandler<GetUserByEmailQuery, UsuarioMongoDto>
    {
        private readonly IUsuarioMongoRepository _repository;

        public GetUserByEmailHandler(IUsuarioMongoRepository repository)
        {
            _repository = repository;
        }

        public async Task<UsuarioMongoDto> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
        {
            return await _repository.ObtenerPorEmailAsync(request.Email, cancellationToken);
        }
    }

}