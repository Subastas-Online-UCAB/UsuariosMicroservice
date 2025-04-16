using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsuarioServicio.Aplicacion.DTOs;

namespace UsuarioServicio.Aplicacion.Command
{
    public class CrearPrivilegioCommand : IRequest<Guid>
    {
        public CrearPrivilegioDTO Privilegio { get; set; }

        public CrearPrivilegioCommand(CrearPrivilegioDTO privilegio)
        {
            Privilegio = privilegio;
        }
    }
}
