using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using Microsoft.EntityFrameworkCore;
using UsuarioServicio.Aplicacion.Queries;
using UsuarioServicio.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace UsuarioServicio.Aplicacion.Services
{
    public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
    {
        private readonly ApplicationDbContext _context;

        public GetAllUsersHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _context.Usuarios
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Email = u.Email,
                    FechaCreacion = u.FechaCreacion,
                    Telefono = u.Telefono,
                    Direccion = u.Direccion,
                    Rol = u.Rol
                })
                .ToListAsync(cancellationToken);

            return users;
        }
    }
}
