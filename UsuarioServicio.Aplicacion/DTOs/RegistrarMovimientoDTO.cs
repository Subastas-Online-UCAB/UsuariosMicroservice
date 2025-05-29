using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Aplicacion.DTOs
{
    public class RegistrarMovimientoDTO
    {
        public string Email { get; set; } = null!;
        public string Accion { get; set; } = null!;
        public string? Detalles { get; set; }
    }
}
