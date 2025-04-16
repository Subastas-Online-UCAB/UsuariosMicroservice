using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using Microsoft.EntityFrameworkCore;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Aplicacion.Queries;
using UsuarioServicio.Infraestructura.Persistencia;

namespace UsuarioServicio.Aplicacion.Servicios
{
    public class GetPrivilegiosPorRolHandler : IRequestHandler<GetPrivilegiosPorRolQuery, List<PrivilegioDTO>>
    {
        private readonly ApplicationDbContext _context;

        public GetPrivilegiosPorRolHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PrivilegioDTO>> Handle(GetPrivilegiosPorRolQuery request, CancellationToken cancellationToken)
        {
            var privilegios = await _context.RolPrivilegios
                .Where(rp => rp.RolId == request.RolId)
                .Include(rp => rp.Privilegio)
                .Select(rp => new PrivilegioDTO
                {
                    Id = rp.Privilegio.Id,
                    Nombre = rp.Privilegio.NombreTabla,
                    Descripcion = rp.Privilegio.Operacion
                })
                .ToListAsync(cancellationToken);

            return privilegios;
        }
    }
}
