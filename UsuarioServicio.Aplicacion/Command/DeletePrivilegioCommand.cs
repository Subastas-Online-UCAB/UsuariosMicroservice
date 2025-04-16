using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace UsuarioServicio.Aplicacion.Commands
{
    public record DeletePrivilegioCommand(Guid PrivilegioId) : IRequest<bool>;
}
