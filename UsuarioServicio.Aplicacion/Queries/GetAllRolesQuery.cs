using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsuarioServicio.Aplicacion.DTOs;

namespace UsuarioServicio.Aplicacion.Queries
{
    public class GetAllRolesQuery : IRequest<List<RolDTO>>
    {
    }
}
