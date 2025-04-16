using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Aplicacion.Command
{
    public class ModificarRolUsuarioCommand : IRequest<bool>
    {
        public string Email { get; set; } = default!;
        public Guid NuevoRolId { get; set; }
    }
}
