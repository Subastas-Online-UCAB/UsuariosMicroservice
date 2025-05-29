using MediatR;
using Microsoft.EntityFrameworkCore;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Aplicacion.Queries;
using UsuarioServicio.Infraestructura.Persistencia;

namespace UsuarioServicio.Aplicacion.Servicios
{
    public class GetAllRolesHandler : IRequestHandler<GetAllRolesQuery, List<RolDTO>>
    {
        private readonly ApplicationDbContext _context;

        public GetAllRolesHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RolDTO>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            return await _context.Roles
                .Select(r => new RolDTO
                {
                    Id = r.Id,
                    Nombre = r.Nombre,
                    Descripcion = r.Descripcion
                })
                .ToListAsync(cancellationToken);
        }
    }
}