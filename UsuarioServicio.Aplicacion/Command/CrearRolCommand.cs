using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsuarioServicio.Aplicacion.DTOs;

namespace UsuarioServicio.Aplicacion.Command
{
    public class CrearRolCommand : IRequest<Guid>
    {
        public CrearRolDTO Rol { get; set; }

        public CrearRolCommand(CrearRolDTO rol)
        {
            Rol = rol;
        }
    }
}
