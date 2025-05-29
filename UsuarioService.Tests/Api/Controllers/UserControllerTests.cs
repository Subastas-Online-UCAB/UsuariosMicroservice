using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;
using MediatR;
using UsuarioServicio.Aplicacion.DTOs;
using UsuarioServicio.Aplicacion.DTOs.Reponses;
using UsuarioServicio.Aplicacion.Queries;
using UsuarioServicio.Infraestructura.Services;
using UsuarioServicio.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using UsuarioServicio.Dominio.Interfaces;
using UsuarioServicio.Aplicacion.Queries;
using Moq.Language.Flow;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Dominio.DTOs;


namespace UsuarioServicio.Api.Tests.Controllers
{
    public class UserControllerTests
    {
        [Fact]
        public async Task RegisterUser_Should_Return_Ok_With_Id_And_Message()
        {
            // Arrange
            var fakeUserId = Guid.NewGuid();
            var command = new RegisterUserCommand
            {
                Nombre = "Juan",
                Apellido = "Pérez",
                Email = "juan@example.com",
                Password = "123456",
                Telefono = "1234567890",
                Direccion = "Calle 1",
                RolId = Guid.NewGuid()
            };

            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeUserId);

            var mockKeycloak = new Mock<IKeycloakAccountService>();
            var controller = new UserController(mockMediator.Object, mockKeycloak.Object);

            // Act
            var result = await controller.RegisterUser(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<RegisterUserResponse>(okResult.Value);

            Assert.Equal(fakeUserId, response.Id);
            Assert.Equal("User registered successfully!", response.Message);
        }


        [Fact]
        public async Task GetUserByEmail_Should_Return_Ok_When_User_Exists()
        {
            // Arrange
            var email = "test@example.com";
            var fakeUser = new UsuarioMongoDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Juan",
                Apellido = "Pérez",
                Email = email,
                Telefono = "1234567890",
                Direccion = "Calle 1"
            };

            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(m => m.Send(It.Is<GetUserByEmailQuery>(q => q.Email == email), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeUser);

            var mockKeycloak = new Mock<IKeycloakAccountService>();
            var controller = new UserController(mockMediator.Object, mockKeycloak.Object);

            // Act
            var result = await controller.GetUserByEmail(email);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsType<UsuarioMongoDto>(okResult.Value);
            Assert.Equal(email, returnedUser.Email);
        }

        [Fact]
        public async Task GetUserByEmail_Should_Return_NotFound_When_User_Does_Not_Exist()
        {
            // Arrange
            var email = "noexiste@example.com";

            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(m => m.Send(It.Is<GetUserByEmailQuery>(q => q.Email == email), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UsuarioMongoDto?)null); // Simula que no encontró al usuario

            var mockKeycloak = new Mock<IKeycloakAccountService>();
            var controller = new UserController(mockMediator.Object, mockKeycloak.Object);

            // Act
            var result = await controller.GetUserByEmail(email);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsAssignableFrom<object>(notFoundResult.Value);

            // Puedes validar el mensaje si quieres:
            var message = response.GetType().GetProperty("Message")?.GetValue(response, null)?.ToString();
            Assert.Equal("User not found", message);
        }


        [Fact]
        public async Task GetAllUsers_Should_Return_List()
        {
            //arrange
            var list = new List<UsuarioMongoDto>{

                new UsuarioMongoDto { Id = Guid.NewGuid(), Nombre = "Juan", Apellido = "Pérez", Email = "juan@example.com" },
                new UsuarioMongoDto { Id = Guid.NewGuid(), Nombre = "Ana", Apellido = "Gómez", Email = "ana@example.com" }

            };

            var mockKeyCloack = new Mock<IKeycloakAccountService>();

            var mockMediator = new Mock<IMediator>();

            mockMediator.Setup(m => m.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(list);
            var controlador = new UserController(mockMediator.Object, mockKeyCloack.Object);

            //act 

            var result = await controlador.GetAllUsers();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUsers = Assert.IsAssignableFrom<List<UsuarioMongoDto>>(okResult.Value);

            Assert.Equal(2, returnedUsers.Count);

        }

        [Fact]
        public async Task GetAllUsers_Should_Return_Empty_List_When_There_Are_No_Users()
        {
            // Arrange
            var emptyList = new List<UsuarioMongoDto>();
            var mockMediator = new Mock<IMediator>();
            var mockKeycloak = new Mock<IKeycloakAccountService>();

            mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyList);

            var controller = new UserController(mockMediator.Object, mockKeycloak.Object);

            // Act
            var result = await controller.GetAllUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<List<UsuarioMongoDto>>(okResult.Value);
            Assert.Empty(returned);
        }


        [Fact]
        public async Task DeleteUserByEmail_Should_Return_Ok_With_Result()
        {
            //arrange 

            var email = "prueba@gmail.com";
            var expectedMessage = "Usuario eliminado correctamente.";

            var mockKeycloack = new Mock<IKeycloakAccountService>();
            var mockMediator = new Mock<IMediator>();

            mockMediator.Setup(m =>
                    m.Send(It.Is<DeleteUserByEmailCommand>(q => q.Email == email), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedMessage);

            var controller = new UserController(mockMediator.Object, mockKeycloack.Object); 

            //act  
            var result = await controller.DeleteUserByEmail(email);


            var okResult = Assert.IsType<OkObjectResult>(result);
            var message = Assert.IsType<string>(okResult.Value);
            Assert.Equal(expectedMessage, message);

        }

        [Fact]
        public async Task UpdateUser_Should_Send_Command_And_Return_Ok()
        {
            var fakeUser = new UpdateUserCommand()
            {
                Nombre = "Juan",
                Apellido = "Pérez",
                Email = "test@example.com",
                Telefono = "1234567890",
                Direccion = "Calle 1"
            };


            var mockKeycloack = new Mock<IKeycloakAccountService>();
            var mockMediator = new Mock<IMediator>();

            mockMediator.Setup(r => r.Send(It.Is<UpdateUserCommand>(u => u.Email == fakeUser.Email), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller = new UserController(mockMediator.Object, mockKeycloack.Object);

            //act

            var result = await controller.UpdateUser(fakeUser);


            //assert 

            var okResult = Assert.IsType<OkObjectResult>(result);
            var message = Assert.IsType<MessageResponse>(okResult.Value);
            Assert.Equal("Usuario actualizado correctamente.", message.Message);
        }

        [Fact]
        public async Task ResetPassword_Should_Call_Service_And_Return_Ok()
        {
            // Arrange
            var email = "test@example.com";
            var mockMediator = new Mock<IMediator>();
            var mockKeycloak = new Mock<IKeycloakAccountService>();

            // Simula que el método se ejecuta correctamente
            mockKeycloak
                .Setup(k => k.SendResetPasswordEmailAsync(email, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var controller = new UserController(mockMediator.Object, mockKeycloak.Object);

            // Act
            var result = await controller.ResetPassword(email);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var message = Assert.IsType<string>(okResult.Value);
            Assert.Equal("Password reset email sent", message);

            // Verifica que el método fue llamado correctamente
            mockKeycloak.Verify(k => k.SendResetPasswordEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ObtenerHistorial_ShouldReturnOk_WithHistorialData()
        {
            // Arrange
            var email = "test@example.com";
            var historial = new List<MovimientoMongoDto>
            {
                new MovimientoMongoDto { Accion = "Login", FechaHora = DateTime.UtcNow, Detalles = "Ingreso desde Chrome" },
                new MovimientoMongoDto { Accion = "Logout", FechaHora = DateTime.UtcNow.AddMinutes(-5), Detalles = "Cierre de sesión" }
            };

            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(m => m.Send(It.Is<GetHistorialPorEmailQuery>(q => q.Email == email), It.IsAny<CancellationToken>()))
                .ReturnsAsync(historial);

            var controller = new UserController(mockMediator.Object, null); // null para IKeycloakAccountService si no se usa

            // Act
            var result = await controller.ObtenerHistorial(email);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsAssignableFrom<List<MovimientoMongoDto>>(okResult.Value);

            Assert.Equal(2, value.Count);
            Assert.Contains(value, x => x.Accion == "Login");
            Assert.Contains(value, x => x.Accion == "Logout");
        }

        [Fact]
        public async Task RegistrarMovimiento_ShouldReturnOk_WhenCommandIsValid()
        {
            // Arrange
            var command = new RegistrarMovimientoCommand(
                email: "usuario@example.com",
                accion: "LOGIN",
                detalles: "Inicio de sesión desde Chrome"
            );

            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync("Movimiento registrado correctamente.");

            var controller = new UserController(mockMediator.Object, null); // null si no usas IKeycloakAccountService

            // Act
            var result = await controller.RegistrarMovimiento(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Movimiento registrado correctamente.", okResult.Value);
        }

    }
}
