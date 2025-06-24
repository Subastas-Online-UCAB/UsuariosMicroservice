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


        /// <summary>
        /// Asigna un privilegio a un rol específico.
        /// </summary>
        /// <param name="asignacionDto">DTO que contiene los IDs del rol y del privilegio a asignar.</param>
        /// <returns>Confirmación de la asignación.</returns>
        /// <response code="200">Privilegio asignado correctamente.</response>
        /// <response code="400">Datos inválidos.</response>
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AsignarPrivilegioARol([FromBody] AsignarPrivilegioRolDTO asignacionDto)
        {
            var command = new AsignarPrivilegioRolCommand(asignacionDto);
            await _mediator.Send(command);
            return Ok(new { Message = "Privilegio asignado correctamente al Rol." });
        }

        /// <summary>
        /// Elimina la asignación de un privilegio a un rol.
        /// </summary>
        /// <param name="command">Comando que contiene los IDs del rol y del privilegio.</param>
        /// <returns>True si la asignación fue eliminada correctamente.</returns>
        /// <response code="200">Asignación eliminada correctamente.</response>
        /// <response code="404">No se encontró la asignación a eliminar.</response>
        [HttpDelete("unassign")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> EliminarAsignacion([FromBody] EliminarAsignacionPrivilegioCommand command)
        {
            var resultado = await _mediator.Send(command);
            return Ok(resultado);
        }
    }
}
