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
    public class GetUserByEmailQuery : IRequest<UsuarioMongoDto>
    {
        public string Email { get; set; }

        public GetUserByEmailQuery(string email)
        {
            Email = email;
        }
    }
}

