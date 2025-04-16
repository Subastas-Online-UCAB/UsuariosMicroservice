using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Aplicacion.DTOs
{
    public class AsignarPrivilegioRolDTO
    {
        public Guid RolId { get; set; }
        public Guid PrivilegioId { get; set; }
    }
}
