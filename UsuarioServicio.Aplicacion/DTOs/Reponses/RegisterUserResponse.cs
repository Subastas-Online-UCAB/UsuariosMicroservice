using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Aplicacion.DTOs.Reponses
{
    public class RegisterUserResponse
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
    }
}
