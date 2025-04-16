using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Aplicacion.Queries;

namespace UsuarioServicio.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "Administrador")]
    [Route("api/[controller]")]
    public class RolController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RolController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CrearRol([FromBody] CrearRolDTO rolDto)
        {
            var command = new CrearRolCommand(rolDto);
            var id = await _mediator.Send(command);
            return Ok(new { Id = id, Message = "Rol creado correctamente" });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _mediator.Send(new GetAllRolesQuery());
            return Ok(roles);
        }


        [HttpDelete("roles/{rolId}")]
        public async Task<IActionResult> DeleteRol(Guid rolId)
        {
            await _mediator.Send(new DeleteRolCommand { RolId = rolId });
            return Ok("Rol eliminado exitosamente.");
        }

        [HttpGet("{rolId}/privilegios")]
        public async Task<IActionResult> GetPrivilegiosDeRol(Guid rolId)
        {
            var result = await _mediator.Send(new GetPrivilegiosPorRolQuery(rolId));
            return Ok(result);
        }

        [HttpPut("rolUsuario")]
        public async Task<IActionResult> ModificarRol([FromBody] ModificarRolUsuarioCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }




    }



}
