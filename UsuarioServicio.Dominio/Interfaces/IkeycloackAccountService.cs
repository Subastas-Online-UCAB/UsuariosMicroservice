using System.Threading;
using System.Threading.Tasks;

namespace UsuarioServicio.Dominio.Interfaces
{
    public interface IKeycloakAccountService
    {
        Task SendResetPasswordEmailAsync(string email, CancellationToken cancellationToken);
        Task UpdateUserAsync(string email, string firstName, string lastName, CancellationToken cancellationToken);
        Task DeleteUser(string userId, CancellationToken cancellationToken);

        Task<string?> GetUserIdByEmail(string email, CancellationToken cancellationToken);


    }
}