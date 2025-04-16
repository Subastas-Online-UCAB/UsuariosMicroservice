using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace UsuarioServicio.Aplicacion.Servicios
{
    public record GetAllPrivilegiosQuery : IRequest<List<Privilegio>>;

    public class GetAllPrivilegiosHandler : IRequestHandler<GetAllPrivilegiosQuery, List<Privilegio>>
    {
        private readonly ApplicationDbContext _context;

        public GetAllPrivilegiosHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Privilegio>> Handle(GetAllPrivilegiosQuery request, CancellationToken cancellationToken)
        {
            return await _context.Privilegios.ToListAsync(cancellationToken);
        }
    }
}
