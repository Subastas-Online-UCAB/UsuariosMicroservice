using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsuarioServicio.Dominio.Entidades;

namespace UsuarioServicio.Dominio.Interfaces
{
    public interface IUsuarioRepository
    { 
        Task<bool> EmailExisteAsync(string email, CancellationToken cancellationToken);
        Task<Rol?> ObtenerRolPorIdAsync(Guid rolId, CancellationToken cancellationToken);
        Task GuardarUsuarioAsync(Usuario usuario, CancellationToken cancellationToken);

        Task EliminarAsync(Usuario usuario, CancellationToken cancellationToken);
        Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken);

        Task ActualizarAsync(Usuario usuario, CancellationToken cancellationToken);

    }
}
