using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace UsuarioServicio.Aplicacion.Commands
{
    public class DeleteUserByEmailCommand : IRequest<string>
    {
        public string Email { get; set; }

        public DeleteUserByEmailCommand(string email)
        {
            Email = email;
        }
    }
}

