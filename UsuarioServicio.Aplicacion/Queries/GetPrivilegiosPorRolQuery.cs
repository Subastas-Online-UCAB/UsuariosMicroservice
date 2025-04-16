using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using UsuarioServicio.Aplicacion.DTOs;

namespace UsuarioServicio.Aplicacion.Queries
{
    public class GetPrivilegiosPorRolQuery : IRequest<List<PrivilegioDTO>>
    {
        public Guid RolId { get; set; }

        public GetPrivilegiosPorRolQuery(Guid rolId)
        {
            RolId = rolId;
        }
    }
}
