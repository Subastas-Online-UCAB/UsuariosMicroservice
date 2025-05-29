using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver; // ✅ Este es el que necesitas
using UsuarioServicio.Infraestructura.MongoDB.Documentos;

namespace UsuarioServicio.Infraestructura.MongoDB
{
    

    public interface IMongoDbContext
    {
        IMongoCollection<UsuarioMongo> Usuarios { get; }
        IMongoCollection<RolMongo> Roles { get; }
        IMongoCollection<RolPrivilegioMongo> RolesPrivilegios { get; }
        IMongoDatabase Database { get; }
        IMongoCollection<T> GetCollection<T>(string nombreColeccion);

        IMongoCollection<MovimientoUsuarioMongo> Movimientos { get; }

    }
}

