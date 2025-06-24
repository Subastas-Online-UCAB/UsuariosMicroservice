using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsuarioServicio.Dominio.DTOs;
using UsuarioServicio.Dominio.Entidades;

namespace UsuarioServicio.Dominio.Interfaces
{
    public interface IHistorialMovimientoRepository
    {
        Task<List<MovimientoMongoDto>> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken);

        Task GuardarMovimientoAsync(MovimientoUsuario movimiento, CancellationToken cancellationToken);
    }
}

