using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Aplicacion.Command
{
    public class DeleteRolCommand : IRequest<bool>
    {
        public Guid RolId { get; set; }
    }
}
