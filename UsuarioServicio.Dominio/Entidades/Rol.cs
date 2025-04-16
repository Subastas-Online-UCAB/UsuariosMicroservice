using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Dominio.Entidades
{
    public class Rol
    {
        public Guid Id { get; set; } // Primary Key
        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        // Relaciones
        public ICollection<RolPrivilegio> RolPrivilegios { get; set; }
    }
}
