using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Dominio.DTOs
{
    public class MovimientoMongoDto
    {
        public string Accion { get; set; } = null!;
        public string? Detalles { get; set; }
        public DateTime FechaHora { get; set; }
    }
}
