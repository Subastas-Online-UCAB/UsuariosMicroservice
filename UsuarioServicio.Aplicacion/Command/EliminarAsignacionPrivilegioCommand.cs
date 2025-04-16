using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Aplicacion.Command
{
    public class EliminarAsignacionPrivilegioCommand : IRequest<bool>
    {
        public Guid RolId { get; set; }
        public Guid PrivilegioId { get; set; }
    }
}
