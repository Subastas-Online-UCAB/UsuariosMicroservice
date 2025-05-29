using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Dominio.DTOs;
using UsuarioServicio.Dominio.Entidades;

namespace UsuarioServicio.Aplicacion.Queries
{
    public class GetAllUsersQuery : IRequest<List<UsuarioMongoDto>>
    {
    }

}

