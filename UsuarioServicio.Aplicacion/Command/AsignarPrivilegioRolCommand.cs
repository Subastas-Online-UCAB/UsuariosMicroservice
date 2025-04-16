using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using UsuarioServicio.Aplicacion.DTOs;

namespace UsuarioServicio.Aplicacion.Command
{
    public class AsignarPrivilegioRolCommand : IRequest<Unit>
    {
        public AsignarPrivilegioRolDTO Asignacion { get; set; }

        public AsignarPrivilegioRolCommand(AsignarPrivilegioRolDTO asignacion)
        {
            Asignacion = asignacion;
        }
    }
}

