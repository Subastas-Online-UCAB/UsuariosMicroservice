using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsuarioServicio.Dominio.DTOs;


namespace UsuarioServicio.Dominio.Interfaces
{
    public interface IUsuarioMongoRepository
    {
        Task<List<UsuarioMongoDto>> ObtenerTodosAsync(CancellationToken cancellationToken);
        Task<UsuarioMongoDto> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken);

    }
}

