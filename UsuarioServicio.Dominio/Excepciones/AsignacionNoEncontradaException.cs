using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Dominio.Excepciones
{
    public class AsignacionNoEncontradaException : Exception
    {
        public AsignacionNoEncontradaException(Guid rolId, Guid privilegioId)
            : base($"No se encontró una asignación entre el Rol {rolId} y el Privilegio {privilegioId}.") { }
    }
}
