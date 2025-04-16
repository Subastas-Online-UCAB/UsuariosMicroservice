using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Dominio.Entidades
{
    public class RolPrivilegio
    {
        public Guid RolId { get; set; }
        public Rol Rol { get; set; }

        public Guid PrivilegioId { get; set; }
        public Privilegio Privilegio { get; set; }
    }
}
