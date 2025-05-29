using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Dominio.Events
{
    public class PrivilegioAsignadoEvent
    {
        public Guid RolId { get; set; }
        public Guid PrivilegioId { get; set; }
    }
}
