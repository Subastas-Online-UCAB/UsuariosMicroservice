using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Dominio.Events
{
    public class PrivilegioEliminadoEvent
    {
        public string RolId { get; set; } = default!;
        public string PrivilegioId { get; set; } = default!;
    }
}
