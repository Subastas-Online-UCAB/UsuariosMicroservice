using MediatR;
using Microsoft.AspNetCore.Mvc;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Aplicacion.DTOs;

namespace UsuarioServicio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolPrivilegioController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RolPrivilegioController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> AsignarPrivilegioARol([FromBody] AsignarPrivilegioRolDTO asignacionDto)
        {
            var command = new AsignarPrivilegioRolCommand(asignacionDto);
            await _mediator.Send(command);
            return Ok(new { Message = "Privilegio asignado correctamente al Rol." });
        }

        [HttpDelete("unassign")]
        public async Task<IActionResult> EliminarAsignacion([FromBody] EliminarAsignacionPrivilegioCommand command)
        {
            var resultado = await _mediator.Send(command);
            return Ok(resultado);
        }
    }
}
