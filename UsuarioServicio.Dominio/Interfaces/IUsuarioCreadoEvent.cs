using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Dominio.Interfaces
{
    public interface IUsuarioCreadoEvent
    {
        Guid UsuarioId { get; }
        string Nombre { get; }
        string Apellido { get; }
        string Email { get; }
        DateTime FechaCreacion { get; }
    }
}
