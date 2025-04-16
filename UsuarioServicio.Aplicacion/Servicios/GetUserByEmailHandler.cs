using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using Microsoft.EntityFrameworkCore;
using UsuarioServicio.Aplicacion.Queries;
using UsuarioServicio.Infraestructura.Persistencia;

namespace UsuarioServicio.Aplicacion.Services
{
    public class GetUserByEmailHandler : IRequestHandler<GetUserByEmailQuery, UserDto>
    {
        private readonly ApplicationDbContext _context;

        public GetUserByEmailHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserDto> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Usuarios
                .Where(u => u.Email == request.Email)
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
                .FirstOrDefaultAsync(cancellationToken);

            return user;
        }
    }
}

