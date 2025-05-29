using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mongo2Go;
using MongoDB.Bson;
using MongoDB.Driver;
using UsuarioServicio.Dominio.DTOs;
using UsuarioServicio.Infraestructura.MongoDB;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;
using UsuarioServicio.Infraestructura.MongoDB.Repositorios;
using Xunit;

public class HistorialMongoRepositoryTests : IDisposable
{
    private readonly MongoDbRunner _runner;
    private readonly IMongoDatabase _database;
    private readonly IMongoDbContext _mongoDbContext;
    private readonly HistorialMongoRepository _repository;

    public HistorialMongoRepositoryTests()
    {
        _runner = MongoDbRunner.Start();
        var client = new MongoClient(_runner.ConnectionString);
        _database = client.GetDatabase("TestDb");

        _mongoDbContext = new MongoDbContextFake(_database); // clase auxiliar
        _repository = new HistorialMongoRepository(_mongoDbContext);
    }

    [Fact]
    public async Task ObtenerPorEmailAsync_DeberiaRetornarMovimientosDelUsuario()
    {
        var usuarioId = Guid.NewGuid();

        await _mongoDbContext.Usuarios.InsertOneAsync(new UsuarioMongo
        {
            UsuarioId = usuarioId,
            Email = "miguel@example.com",
            Nombre = "Miguel"
        });

        await _mongoDbContext.Movimientos.InsertManyAsync(new[]
        {
            new MovimientoUsuarioMongo
            {
                Id = ObjectId.GenerateNewId(),
                UsuarioId = usuarioId.ToString(),
                Accion = "Login",
                Detalles = "Inicio de sesión exitoso",
                FechaHora = DateTime.UtcNow.AddMinutes(-10)
            },
            new MovimientoUsuarioMongo
            {
                Id = ObjectId.GenerateNewId(),
                UsuarioId = usuarioId.ToString(),
                Accion = "Cambio Contraseña",
                Detalles = "Se cambió la contraseña",
                FechaHora = DateTime.UtcNow
            }
        });

        var movimientos = await _repository.ObtenerPorEmailAsync("miguel@example.com", CancellationToken.None);

        Assert.Equal(2, movimientos.Count);
        Assert.Equal("Cambio Contraseña", movimientos[0].Accion); // más reciente
    }

    [Fact]
    public async Task ObtenerPorEmailAsync_SiUsuarioNoExiste_DeberiaLanzarExcepcion()
    {
        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await _repository.ObtenerPorEmailAsync("desconocido@example.com", CancellationToken.None);
        });
    }

    public void Dispose()
    {
        _runner.Dispose();
    }
}
