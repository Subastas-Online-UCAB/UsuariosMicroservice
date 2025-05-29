using MongoDB.Driver;
using UsuarioServicio.Infraestructura.MongoDB;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;

public class MongoDbContextFake : IMongoDbContext
{
    public IMongoCollection<UsuarioMongo> Usuarios { get; }
    public IMongoCollection<RolMongo> Roles { get; }
    public IMongoCollection<RolPrivilegioMongo> RolesPrivilegios { get; }
    public IMongoCollection<MovimientoUsuarioMongo> Movimientos { get; }

    public IMongoDatabase Database { get; }

    public MongoDbContextFake(IMongoDatabase database)
    {
        Database = database;

        Usuarios = database.GetCollection<UsuarioMongo>("Usuarios");
        Roles = database.GetCollection<RolMongo>("Roles");
        RolesPrivilegios = database.GetCollection<RolPrivilegioMongo>("RolesPrivilegios");
        Movimientos = database.GetCollection<MovimientoUsuarioMongo>("Movimientos");
    }

    public IMongoCollection<T> GetCollection<T>(string nombreColeccion)
    {
        return Database.GetCollection<T>(nombreColeccion);
    }
}