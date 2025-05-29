using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsuarioServicio.Dominio.Interfaces;
namespace UsuarioServicio.Dominio.Events
{
    public class UsuarioCreadoEvent : IUsuarioCreadoEvent
    {
        public Guid UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public DateTime FechaCreacion { get; set; }

        public string? Telefono { get; set; }
        public string? Direccion { get; set; }

        public Guid RolId { get; set; } // FK hacia Rol

    }
}
