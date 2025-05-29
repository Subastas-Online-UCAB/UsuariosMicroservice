using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Aplicacion.DTOs
{
    public class MovimientoUsuarioDTO
    {
        public string Accion { get; set; } = null!;
        public DateTime FechaHora { get; set; }
        public string? Detalles { get; set; }
    }

}
