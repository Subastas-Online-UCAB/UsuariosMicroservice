using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;

namespace UsuarioServicio.Infraestructura.MongoDB
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<UsuarioMongo> Usuarios =>
            _database.GetCollection<UsuarioMongo>("usuarios");

        public IMongoCollection<RolMongo> Roles =>
        _database.GetCollection<RolMongo>("Roles");

        public IMongoCollection<RolPrivilegioMongo> RolesPrivilegios => _database.GetCollection<RolPrivilegioMongo>("RolPrivilegios");




        public IMongoDatabase Database => _database;

    }
}
