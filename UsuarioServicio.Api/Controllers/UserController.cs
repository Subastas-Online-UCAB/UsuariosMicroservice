using Microsoft.AspNetCore.Mvc;
using MediatR;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Aplicacion.Queries;
using UsuarioServicio.Infraestructura.Services;
using Microsoft.AspNetCore.Authorization;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Dominio.Interfaces;
using UsuarioServicio.Aplicacion.DTOs.Reponses;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;

namespace UsuarioServicio.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        private readonly IKeycloakAccountService _keycloakService;


        public UserController(IMediator mediator, IKeycloakAccountService keycloakService)
        {
            _mediator = mediator;
             _keycloakService = keycloakService;
        }


        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="command">Datos del usuario a registrar.</param>
        /// <returns>El ID del usuario creado.</returns>
        /// <response code="201">Usuario creado correctamente.</response>
        /// <response code="400">Datos inválidos o email ya registrado.</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(Guid), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserCommand command)
        {
            var userId = await _mediator.Send(command);
            return Ok(new RegisterUserResponse
            {
                Id = userId,
                Message = "User registered successfully!"
            });
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("API is working!");
        }

        /// <summary>
        /// Obtiene la lista de todos los usuarios registrados.
        /// </summary>
        /// <returns>Una lista de usuarios.</returns>
        /// <response code="200">Lista de usuarios obtenida exitosamente.</response>
        [HttpGet("list")]
        [ProducesResponseType(typeof(List<UserDto>), 200)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _mediator.Send(new GetAllUsersQuery());
            return Ok(users);
        }

        /// <summary>
        /// Obtiene un usuario por su Email.
        /// </summary>
        /// <param name="Email">Email del usuario.</param>
        /// <returns>Los datos del usuario.</returns>
        /// <response code="200">Usuario encontrado.</response>
        /// <response code="404">Usuario no existe.</response>

        [HttpGet("by-email")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            var user = await _mediator.Send(new GetUserByEmailQuery(email));

            if (user == null)
                return NotFound(new { Message = "User not found" });

            return Ok(user);
        }

        /// <summary>
        /// Elimina un usuario por su email.
        /// </summary>
        /// <param name="email">Email del usuario a eliminar.</param>
        /// <returns>Mensaje de confirmación.</returns>
        /// <response code="200">Usuario eliminado correctamente.</response>
        /// <response code="404">Usuario no encontrado.</response>

        [HttpDelete("delete-by-email/{email}")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUserByEmail(string email)
        {
            var result = await _mediator.Send(new DeleteUserByEmailCommand(email));
            return Ok(result);
        }

        /// <summary>
        /// Actualiza los datos de un usuario por su email.
        /// </summary>
        /// <param name="command">Datos actualizados del usuario.</param>
        /// <returns>True si se actualizó correctamente.</returns>
        /// <response code="200">Actualización exitosa.</response>
        /// <response code="404">Usuario no encontrado.</response>
        [HttpPut]

        [HttpPut("update")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserCommand command)
        {
            await _mediator.Send(command);
            return Ok(new MessageResponse("Usuario actualizado correctamente."));
        }


        /// <summary>
        /// Envía un correo de restablecimiento de contraseña al usuario.
        /// </summary>
        /// <param name="email">Email del usuario que desea restablecer su contraseña.</param>
        /// <returns>Mensaje de confirmación.</returns>
        /// <response code="200">Correo de restablecimiento enviado correctamente.</response>
        /// <response code="404">Usuario no encontrado en Keycloak.</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ResetPassword([FromBody] string email)
        {
            await _keycloakService.SendResetPasswordEmailAsync(email, CancellationToken.None);
            return Ok("Password reset email sent");
        }

        /// <summary>
        /// Obtiene el historial de movimientos registrados de un usuario por su email.
        /// </summary>
        /// <param name="email">Email del usuario.</param>
        /// <returns>Lista de movimientos del usuario.</returns>
        /// <response code="200">Historial obtenido exitosamente.</response>
        /// <response code="404">Usuario no encontrado o sin historial.</response>
        [HttpGet("historial/{email}")]
        [ProducesResponseType(typeof(List<MovimientoUsuarioMongo>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ObtenerHistorial(string email)
        {
            var historial = await _mediator.Send(new GetHistorialPorEmailQuery(email));
            return Ok(historial);
        }

        /// <summary>
        /// Registra un nuevo movimiento del usuario (acción realizada).
        /// </summary>
        /// <param name="command">Datos del movimiento a registrar.</param>
        /// <returns>Mensaje de confirmación.</returns>
        /// <response code="200">Movimiento registrado exitosamente.</response>
        /// <response code="404">Usuario no encontrado en base de datos.</response>

        [HttpPost("registrar-movimiento")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RegistrarMovimiento([FromBody] RegistrarMovimientoCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }



    }
}
