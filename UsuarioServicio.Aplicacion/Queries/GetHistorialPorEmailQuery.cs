using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Dominio.DTOs;

namespace UsuarioServicio.Aplicacion.Queries
{
    

    public class GetHistorialPorEmailQuery : IRequest<List<MovimientoMongoDto>>
    {
        public string Email { get; }

        public GetHistorialPorEmailQuery(string email)
        {
            Email = email;
        }
    }

}
