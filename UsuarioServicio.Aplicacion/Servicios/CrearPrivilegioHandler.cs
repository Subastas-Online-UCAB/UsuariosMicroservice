using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using UsuarioServicio.Aplicacion.Command; // 👈 Recuerda, como tu carpeta está como "Command"
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Infraestructura.Persistencia;

namespace UsuarioServicio.Aplicacion.Servicios
{
    public class CrearPrivilegioHandler : IRequestHandler<CrearPrivilegioCommand, Guid>
    {
        private readonly ApplicationDbContext _context;

        public CrearPrivilegioHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CrearPrivilegioCommand request, CancellationToken cancellationToken)
        {
            var privilegio = new Privilegio
            {
                Id = Guid.NewGuid(),
                NombreTabla = request.Privilegio.NombreTabla,
                Operacion = request.Privilegio.Operacion
            };

            _context.Privilegios.Add(privilegio);
            await _context.SaveChangesAsync(cancellationToken);

            return privilegio.Id;
        }
    }
}

