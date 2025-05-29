using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsuarioServicio.Dominio.Interfaces
{
    public interface IKeycloakService
    {
        Task<string> RegistrarUsuarioAsync(
            string nombre,
            string apellido,
            string email,
            string password,
            CancellationToken cancellationToken);

        Task AsignarRolAsync(
            string keycloakUserId,
            string rolNombre,
            CancellationToken cancellationToken);

        Task EnviarCorreoVerificacionAsync(string userId, CancellationToken cancellationToken);

        //Task SendResetPasswordEmailAsync(string email, CancellationToken cancellationToken);
    }
}
