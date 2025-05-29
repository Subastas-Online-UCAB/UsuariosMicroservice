using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Aplicacion.Command
{
    public class RegistrarMovimientoCommand : IRequest<string>
    {
        public string Email { get; }
        public string Accion { get; }
        public string? Detalles { get; }

        public RegistrarMovimientoCommand(string email, string accion, string? detalles = null)
        {
            Email = email;
            Accion = accion;
            Detalles = detalles;
        }
    }
}
