using Mongo2Go;
using MongoDB.Driver;
using UsuarioServicio.Dominio.DTOs;
using UsuarioServicio.Infraestructura.MongoDB;
using UsuarioServicio.Infraestructura.MongoDB.Documentos;
using UsuarioServicio.Infraestructura.MongoDB.Repositorios;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class UsuarioMongoRepositoryTests : IDisposable
{
    private readonly MongoDbRunner _runner;
    private readonly IMongoDatabase _database;
    private readonly IMongoDbContext _context;
    private readonly UsuarioMongoRepository _repository;

    public UsuarioMongoRepositoryTests()
    {
        _runner = MongoDbRunner.Start();
        var client = new MongoClient(_runner.ConnectionString);
        _database = client.GetDatabase("TestDb");
        _context = new MongoDbContextFake(_database);
        _repository = new UsuarioMongoRepository(_context);
    }

    [Fact]
    public async Task ObtenerTodosAsync_DeberiaRetornarUsuariosConRol()
    {
        // Arrange
        var rolId = Guid.NewGuid().ToString();

        await _context.GetCollection<RolMongo>("Roles").InsertOneAsync(new RolMongo
        {
            Id = rolId,
            Nombre = "Admin",
            Descripcion = "Administrador del sistema"
        });

        await _context.GetCollection<UsuarioMongo>("usuarios").InsertOneAsync(new UsuarioMongo
        {
            UsuarioId = Guid.NewGuid(),
            Nombre = "Miguel",
            Apellido = "Garcia",
            Email = "miguel@example.com",
            FechaCreacion = DateTime.UtcNow,
            Telefono = "123456789",
            Direccion = "Caracas",
            RolId = rolId
        });

        // Act
        var resultado = await _repository.ObtenerTodosAsync(CancellationToken.None);

        // Assert
        Assert.Single(resultado);
        Assert.Equal("Miguel", resultado[0].Nombre);
        Assert.NotNull(resultado[0].Rol);
        Assert.Equal("Admin", resultado[0].Rol.Nombre);
    }

    [Fact]
    public async Task ObtenerPorEmailAsync_CuandoExiste_DeberiaRetornarUsuarioConRol()
    {
        var rolId = Guid.NewGuid().ToString();

        await _context.GetCollection<RolMongo>("Roles").InsertOneAsync(new RolMongo
        {
            Id = rolId,
            Nombre = "Editor",
            Descripcion = "Puede editar contenido"
        });

        await _context.GetCollection<UsuarioMongo>("usuarios").InsertOneAsync(new UsuarioMongo
        {
            UsuarioId = Guid.NewGuid(),
            Nombre = "Laura",
            Apellido = "Gomez",
            Email = "laura@example.com",
            FechaCreacion = DateTime.UtcNow,
            Telefono = "5551234",
            Direccion = "Valencia",
            RolId = rolId
        });

        var resultado = await _repository.ObtenerPorEmailAsync("laura@example.com", CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal("Laura", resultado.Nombre);
        Assert.NotNull(resultado.Rol);
        Assert.Equal("Editor", resultado.Rol.Nombre);
    }

    [Fact]
    public async Task ObtenerPorEmailAsync_CuandoNoExiste_DeberiaRetornarNull()
    {
        var resultado = await _repository.ObtenerPorEmailAsync("noexiste@example.com", CancellationToken.None);

        Assert.Null(resultado);
    }

    public void Dispose()
    {
        _runner.Dispose();
    }
}
