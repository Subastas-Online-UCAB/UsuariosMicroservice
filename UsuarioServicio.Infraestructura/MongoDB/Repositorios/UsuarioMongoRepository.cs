using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Driver;
using UsuarioServicio.Dominio.DTOs;
using UsuarioServicio.Dominio.Interfaces;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;

namespace UsuarioServicio.Infraestructura.MongoDB.Repositorios
{
    public class UsuarioMongoRepository : IUsuarioMongoRepository
    {
        private readonly IMongoCollection<UsuarioMongo> _coleccion;

        public UsuarioMongoRepository(MongoDbContext context)
        {
            _coleccion = context.Database.GetCollection<UsuarioMongo>("Usuarios");
        }

        public async Task<List<UsuarioMongoDto>> ObtenerTodosAsync(CancellationToken cancellationToken)
        {
            var documentos = await _coleccion.Find(_ => true).ToListAsync(cancellationToken);

            return documentos.Select(d => new UsuarioMongoDto
            {
                Id = d.UsuarioId,
                Nombre = d.Nombre,
                Apellido = d.Apellido,
                Email = d.Email,
                FechaCreacion = d.FechaCreacion,
                Telefono = d.Telefono,
                Direccion = d.Direccion,
                RolId = Guid.Parse(d.RolId),
            }).ToList();
        }
    }
}