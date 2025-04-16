using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Dominio.Entidades
{
    public class Privilegio
    {
        public Guid Id { get; set; } // Primary Key
        public string NombreTabla { get; set; }
        public string Operacion { get; set; }

        // Relaciones
        public ICollection<RolPrivilegio> RolPrivilegios { get; set; }
    }
}
