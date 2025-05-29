using MongoDB.Driver;
using UsuarioServicio.Dominio.DTOs;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Dominio.Interfaces;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;

namespace UsuarioServicio.Infraestructura.MongoDB.Repositorios
{
    public class UsuarioMongoRepository : IUsuarioMongoRepository
    {
        private readonly IMongoCollection<UsuarioMongo> _usuarios;
        private readonly IMongoCollection<RolMongo> _roles;

        public UsuarioMongoRepository(IMongoDbContext context)
        {
            _usuarios = context.GetCollection<UsuarioMongo>("usuarios");
            _roles = context.GetCollection<RolMongo>("Roles");
        }

        public async Task<List<UsuarioMongoDto>> ObtenerTodosAsync(CancellationToken cancellationToken)
        {
            var usuarios = await _usuarios.Find(_ => true).ToListAsync(cancellationToken);
            var roles = await _roles.Find(_ => true).ToListAsync(cancellationToken);

            return usuarios.Select(u =>
            {
                var rol = roles.FirstOrDefault(r => r.Id == u.RolId);

                return new UsuarioMongoDto
                {
                    Id = u.UsuarioId,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Email = u.Email,
                    FechaCreacion = u.FechaCreacion,
                    Telefono = u.Telefono,
                    Direccion = u.Direccion,
                    RolId = Guid.Parse(u.RolId),
                    Rol = rol != null ? new RolMongoDto
                    {
                        Id = Guid.Parse(rol.Id),
                        Nombre = rol.Nombre
                    } : null
                };
            }).ToList();
        }

        public async Task<UsuarioMongoDto> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken)
        {
            var usuario = await _usuarios.Find(u => u.Email == email).FirstOrDefaultAsync(cancellationToken);

            if (usuario == null)
                return null;

            var roles = await _roles.Find(_ => true).ToListAsync(cancellationToken);

            var rol = roles.FirstOrDefault(r => r.Id == usuario.RolId);

            return new UsuarioMongoDto
            {
                Id = usuario.UsuarioId,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                FechaCreacion = usuario.FechaCreacion,
                Telefono = usuario.Telefono,
                Direccion = usuario.Direccion,
                Rol = rol != null
                    ? new RolMongoDto
                    {
                        Id = Guid.Parse(rol.Id),
                        Nombre = rol.Nombre,
                        Descripcion = rol.Descripcion
                    }
                    : null
            };
        }

    }
}