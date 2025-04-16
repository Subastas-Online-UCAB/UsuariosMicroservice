using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace UsuarioServicio.Aplicacion.Commands
{
    public class UpdateUserCommand : IRequest<bool>
    {
        public string Email { get; set; } // Usamos el correo para buscar
        public string Nombre { get; set; }
        public string Apellido { get; set; }

        public string? Telefono { get; set; } 
        public string? Direccion { get; set; }
    }
}

