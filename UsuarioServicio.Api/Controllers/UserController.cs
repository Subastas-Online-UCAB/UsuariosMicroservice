using Microsoft.AspNetCore.Mvc;
using MediatR;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Aplicacion.Services;
using UsuarioServicio.Aplicacion.Queries;
using UsuarioServicio.Aplicacion.Commands;
using UsuarioServicio.Infraestructura.Services;
using Microsoft.AspNetCore.Authorization;

namespace UsuarioServicio.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        private readonly KeycloakService _keycloakService;


        public UserController(IMediator mediator, KeycloakService keycloakService)
        {
            _mediator = mediator;
             _keycloakService = keycloakService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserCommand command)
        {
            var userId = await _mediator.Send(command);
            return Ok(new { Id = userId, Message = "User registered successfully!" });
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("API is working!");
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _mediator.Send(new GetAllUsersQuery());
            return Ok(users);
        }

        [HttpGet("by-email")]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            var user = await _mediator.Send(new GetUserByEmailQuery(email));

            if (user == null)
                return NotFound(new { Message = "User not found" });

            return Ok(user);
        }

        [HttpDelete("delete-by-email/{email}")]
        public async Task<IActionResult> DeleteUserByEmail(string email)
        {
            var result = await _mediator.Send(new DeleteUserByEmailCommand(email));
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserCommand command)
        {
            await _mediator.Send(command);
            return Ok(new { message = "Usuario actualizado correctamente." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] string email)
        {
            await _keycloakService.SendResetPasswordEmailAsync(email, CancellationToken.None);
            return Ok("Password reset email sent");
        }



    }
}
