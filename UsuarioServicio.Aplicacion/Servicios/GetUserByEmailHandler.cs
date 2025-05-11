using MediatR;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Aplicacion.Queries;
using UsuarioServicio.Infraestructura.MongoDB;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UsuarioServicio.Dominio.Entidades;

namespace UsuarioServicio.Aplicacion.Handlers
{
    public class GetUserByEmailHandler : IRequestHandler<GetUserByEmailQuery, UserDto>
    {
        private readonly MongoDbContext _mongoDbContext;

        public GetUserByEmailHandler(MongoDbContext mongoDbContext)
        {
            _mongoDbContext = mongoDbContext;
        }

        public async Task<UserDto> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
        {
            var usuarioMongo = await _mongoDbContext.Usuarios
                .Find(u => u.Email == request.Email)
                .FirstOrDefaultAsync(cancellationToken);

            if (usuarioMongo == null)
                return null;

            var rolesMongo = await _mongoDbContext.Roles
                .Find(_ => true)
                .ToListAsync(cancellationToken);

            var rolMongo = rolesMongo.FirstOrDefault(r => r.Id.ToString() == usuarioMongo.RolId);

            var rol = rolMongo != null
                ? new Rol
                {
                    Id = Guid.Parse(rolMongo.Id),
                    Nombre = rolMongo.Nombre,
                    Descripcion = rolMongo.Descripcion
                }
                : null;

            return new UserDto
            {
                Id = usuarioMongo.UsuarioId,
                Nombre = usuarioMongo.Nombre,
                Apellido = usuarioMongo.Apellido,
                Email = usuarioMongo.Email,
                FechaCreacion = usuarioMongo.FechaCreacion,
                Telefono = usuarioMongo.Telefono,
                Direccion = usuarioMongo.Direccion,
                Rol = rol
            };
        }
    }
}
