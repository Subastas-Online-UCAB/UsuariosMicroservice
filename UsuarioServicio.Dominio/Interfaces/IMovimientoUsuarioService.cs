using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsuarioServicio.Dominio.DTOs;
using UsuarioServicio.Dominio.Entidades;

namespace UsuarioServicio.Dominio.Interfaces
{
    public interface IMovimientoUsuarioService
    {
        Task RegistrarMovimientoAsync(Guid usuarioId, string accion, string? detalles = null, CancellationToken cancellationToken = default);

        

    }
}
