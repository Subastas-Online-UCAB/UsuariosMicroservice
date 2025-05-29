using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Dominio.Entidades
{
    public class MovimientoUsuario
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UsuarioId { get; set; }
        public string Accion { get; set; } = null!;
        public DateTime FechaHora { get; set; } = DateTime.UtcNow;
        public string? Detalles { get; set; } // opcional
    }
}
