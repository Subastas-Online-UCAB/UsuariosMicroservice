using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Dominio.Excepciones
{
    public class UsuarioNoEncontradoEnPostgresException : Exception
    {
        public UsuarioNoEncontradoEnPostgresException(Guid id)
            : base($"No se encontró el usuario en base de datos relacional con ID: {id}") { }
    }
}
