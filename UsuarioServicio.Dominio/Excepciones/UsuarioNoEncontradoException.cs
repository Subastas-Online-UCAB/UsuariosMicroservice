using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Dominio.Excepciones
{
    public class UsuarioNoEncontradoException : Exception
    {
        public UsuarioNoEncontradoException(string mensaje) : base(mensaje) { }
    }
}
