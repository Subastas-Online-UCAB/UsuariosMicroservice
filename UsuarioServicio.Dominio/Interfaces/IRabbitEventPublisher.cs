using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Dominio.Events;

public interface IRabbitEventPublisher
{
    Task PublicarUsuarioCreadoAsync(Usuario usuario, CancellationToken cancellationToken);
    Task PublicarUsuarioActualizadoAsync(Usuario usuario, CancellationToken cancellationToken);
    Task PublicarUsuarioEliminadoAsync(Guid usuarioId, string email, CancellationToken cancellationToken);
    Task PublicarPrivilegioAsignadoAsync(PrivilegioAsignadoEvent evento, CancellationToken cancellationToken);
    Task PublicarPrivilegioEliminadoAsync(string rolId, string privilegioId, CancellationToken cancellationToken);
    Task PublicarEventoAsync<T>(T evento, CancellationToken cancellationToken);



}
