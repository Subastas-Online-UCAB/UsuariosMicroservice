using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsuarioServicio.Aplicacion.DTOs;

namespace UsuarioServicio.Aplicacion.Interfaces
{
    public interface IHistorialConsultaService
    {
        Task<List<MovimientoUsuarioDTO>> ObtenerHistorialPorEmailAsync(string email, CancellationToken cancellationToken);
    }

}
