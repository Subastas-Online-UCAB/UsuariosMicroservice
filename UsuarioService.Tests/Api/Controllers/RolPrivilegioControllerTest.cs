using Xunit;
using Moq;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UsuarioServicio.Api.Controllers;
using UsuarioServicio.Aplicacion.Command;
using UsuarioServicio.Aplicacion.DTOs;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System.Text.Json;

namespace UsuarioServicio.Tests.Api.Controllers
{
    public class RolPrivilegioControllerTests
    {
        [Fact]
        public async Task AsignarPrivilegioARol_Should_Return_Ok()
        {
            // Arrange
            var dto = new AsignarPrivilegioRolDTO
            {
                RolId = Guid.NewGuid(),
                PrivilegioId = Guid.NewGuid()
            };

            var mockMediator = new Mock<IMediator>();

            mockMediator
                .Setup(m => m.Send(It.IsAny<AsignarPrivilegioRolCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            var controller = new RolPrivilegioController(mockMediator.Object);

            // Act
            var result = await controller.AsignarPrivilegioARol(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var doc = JsonDocument.Parse(json);
            var message = doc.RootElement.GetProperty("Message").GetString();

            Assert.Equal("Privilegio asignado correctamente al Rol.", message);
        }

        [Fact]
        public async Task EliminarAsignacion_Should_Return_Ok_With_Result()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>();
            var controller = new RolPrivilegioController(mockMediator.Object);

            var fakeCommand = new EliminarAsignacionPrivilegioCommand
            {
                RolId = Guid.NewGuid(),
                PrivilegioId = Guid.NewGuid()
            };

            var expectedResult = true;

            mockMediator
                .Setup(m => m.Send(fakeCommand, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await controller.EliminarAsignacion(fakeCommand);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualValue = Assert.IsType<bool>(okResult.Value);
            Assert.True(actualValue);
        }

    }
}