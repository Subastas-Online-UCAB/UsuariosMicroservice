using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Dominio.Events
{
    public class UsuarioEliminadoEvent
    {
        public Guid UsuarioId { get; set; }
        public string Email { get; set; }
    }
}
