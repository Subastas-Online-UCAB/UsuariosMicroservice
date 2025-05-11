using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using UsuarioServicio.Aplicacion.Queries;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Infraestructura.MongoDB;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;
using MongoDB.Driver;
using UsuarioServicio.Dominio.DTOs;
using UsuarioServicio.Dominio.Entidades;
using Keycloak.Net.Models.Roles;
using Keycloak.Net.Models.RealmsAdmin;
using System.Data;

namespace UsuarioServicio.Aplicacion.Handlers
{
    public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
    {
        private readonly MongoDbContext _mongoDbContext;

        public GetAllUsersHandler(MongoDbContext mongoDbContext)
        {
            _mongoDbContext = mongoDbContext;
        }

        public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var usuariosMongo = await _mongoDbContext.Usuarios.Find(_ => true).ToListAsync(cancellationToken);
            var rolesMongo = await _mongoDbContext.Roles.Find(_ => true).ToListAsync(cancellationToken);

            var usuarios = usuariosMongo.Select(u =>
            {
                var rol = rolesMongo.FirstOrDefault(r => r.Id.ToString() == u.RolId?.Trim());


                Console.WriteLine($"Buscando RolId: {u.RolId}");
                foreach (var r in rolesMongo)
                {
                    Console.WriteLine($"Rol.Id: {r.Id} - {r.Id.ToString()}");
                }

                return new UserDto
                {
                    Id = u.UsuarioId,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Email = u.Email,
                    FechaCreacion = u.FechaCreacion,
                    Telefono = u.Telefono,
                    Direccion = u.Direccion,
                    Rol = rol != null ? new Rol
                    {
                        Id = Guid.Parse(rol.Id),
                        Nombre = rol.Nombre,
                        Descripcion = rol.Descripcion
                    } : null
                };
            }).ToList();

            return usuarios; // ✅ Este return es el que faltaba afuera
        }
    }
}
