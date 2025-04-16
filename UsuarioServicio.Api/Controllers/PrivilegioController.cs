using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Aplicacion.Commands;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Aplicacion.Servicios;

namespace UsuarioServicio.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "Administrador")]
    [Route("api/[controller]")]
    public class PrivilegioController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PrivilegioController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CrearPrivilegio([FromBody] CrearPrivilegioDTO privilegioDto)
        {
            var command = new CrearPrivilegioCommand(privilegioDto);
            var id = await _mediator.Send(command);
            return Ok(new { Id = id, Message = "Privilegio creado correctamente" });
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllPrivilegiosQuery());
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _mediator.Send(new DeletePrivilegioCommand(id));
            if (!success)
                return NotFound();

            return NoContent();
        }

    }
}
