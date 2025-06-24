using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsuarioServicio.Dominio.DTOs;
using UsuarioServicio.Dominio.Entidades;

namespace UsuarioServicio.Dominio.Interfaces
{
    public interface IRolPrivilegioRepository
    {
        Task<List<Privilegio>> ObtenerPrivilegiosPorRolAsync(Guid rolId, CancellationToken cancellationToken);
        Task<RolPrivilegio?> ObtenerAsignacionAsync(Guid rolId, Guid privilegioId, CancellationToken cancellationToken);
        Task EliminarAsignacionAsync(RolPrivilegio asignacion, CancellationToken cancellationToken);
    }
}
