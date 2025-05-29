using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Dominio.Events
{
    public class MovimientoRegistradoEvent
    {
        public Guid UsuarioId { get; set; }
        public string Accion { get; set; } = null!;
        public string? Detalles { get; set; }
        public DateTime FechaHora { get; set; }
    }
}
