using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using UsuarioServicio.Dominio.DTOs;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Dominio.Interfaces;
using UsuarioServicio.Infraestructura.MongoDB;

namespace UsuarioServicio.Infraestructura.MongoDB.Repositorios
{
    public class HistorialMongoRepository : IHistorialMovimientoRepository
    {
        private readonly IMongoDbContext _mongoDb;

        public HistorialMongoRepository(IMongoDbContext mongoDb)
        {
            _mongoDb = mongoDb;
        }

        public async Task<List<MovimientoMongoDto>> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken)
        {
            // Buscar el usuario por Email en la colección de usuarios de Mongo
            var usuario = await _mongoDb.Usuarios
                .Find(u => u.Email == email)
                .FirstOrDefaultAsync(cancellationToken);

            if (usuario == null)
                throw new Exception("Usuario no encontrado en Mongo.");

            // Buscar movimientos por UsuarioId
            var movimientos = await _mongoDb.Movimientos
                .Find(m => m.UsuarioId == usuario.UsuarioId.ToString())
                .SortByDescending(m => m.FechaHora)
                .ToListAsync(cancellationToken);

            // Mapear al DTO
            return movimientos.Select(m => new MovimientoMongoDto
            {
                Accion = m.Accion,
                Detalles = m.Detalles,
                FechaHora = m.FechaHora
            }).ToList();
        }

        public async Task GuardarMovimientoAsync(MovimientoUsuario movimiento, CancellationToken cancellationToken)
        {
           
        }

    }
}