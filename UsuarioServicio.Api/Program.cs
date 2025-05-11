using UsuarioServicio.Infraestructura.Persistencia;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UsuarioServicio.Aplicacion.Services;
using UsuarioServicio.Infraestructura.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using MassTransit;
using UsuarioServicio.Infraestructura.Consumers;
using UsuarioServicio.Infraestructura.MongoDB;
using UsuarioServicio.Dominio.Interfaces;
using UsuarioServicio.Infraestructura.Eventos;
using UsuarioServicio.Infraestructura.Persistencia.Repositorio;
using UsuarioServicio.Aplicacion.Servicios;


var builder = WebApplication.CreateBuilder(args);

// Configuración de EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(typeof(RegisterUserHandler).Assembly);
//builder.Services.AddMediatR(typeof(GetAllUsersHandler).Assembly);


// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "UsuarioServicio.Api",
        Version = "v1"
    });

    // 🔐 Configuración de seguridad para JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese 'Bearer {token}' en el campo de autorización."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddScoped<UsuarioServicio.Infraestructura.Services.KeycloakService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:8081/realms/microservicio-usuarios";
        options.Audience = "account"; // tu Client ID real
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "preferred_username",
            RoleClaimType = "roles"
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                var realmAccess = context.Principal.FindFirst("realm_access");

                if (realmAccess != null)
                {
                    var parsed = System.Text.Json.JsonDocument.Parse(realmAccess.Value);
                    if (parsed.RootElement.TryGetProperty("roles", out var roles))
                    {
                        foreach (var role in roles.EnumerateArray())
                        {
                            claimsIdentity.AddClaim(new Claim("roles", role.GetString()));
                        }
                    }
                }

                return Task.CompletedTask;
            }
        };
    });

// MONGODB

builder.Services.AddSingleton<MongoDbContext>(sp =>
{
    var connectionString = builder.Configuration["MongoSettings:ConnectionString"];
    var databaseName = builder.Configuration["MongoSettings:Database"];
    return new MongoDbContext(connectionString, databaseName);
});


// MassTransit
builder.Services.AddMassTransit(x =>
{
    // Registra todos los consumers aquí
    x.AddConsumer<UsuarioCreadoConsumer>();
    x.AddConsumer<UsuarioActualizadoConsumer>();
    x.AddConsumer<UsuarioEliminadoConsumer>();
    x.AddConsumer<PrivilegioAsignadoConsumer>();
    x.AddConsumer<PrivilegioEliminadoConsumer>(); 

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h => { });

        cfg.ReceiveEndpoint("usuario-creado-event", e =>
        {
            e.ConfigureConsumer<UsuarioCreadoConsumer>(context);
        });

        cfg.ReceiveEndpoint("usuario-actualizado-queue", e =>
        {
            e.ConfigureConsumer<UsuarioActualizadoConsumer>(context);
        });

        cfg.ReceiveEndpoint("usuario-eliminado-queue", e =>
        {
            e.ConfigureConsumer<UsuarioEliminadoConsumer>(context);
        });

        cfg.ReceiveEndpoint("privilegio-asignado-queue", e =>
        {
            e.ConfigureConsumer<PrivilegioAsignadoConsumer>(context);
        });

        cfg.ReceiveEndpoint("privilegio-eliminado-queue", e =>
        {
            e.ConfigureConsumer<PrivilegioEliminadoConsumer>(context);
        });
    });
});




//x.AddConsumer<UsuarioCreadoConsumer>();


// Repositorios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Keycloak
builder.Services.AddHttpClient<IKeycloakService, KeycloakUserRegistrationService>();

// RabbitMQ publisher
builder.Services.AddScoped<IRabbitEventPublisher, RabbitEventPublisher>();




var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication(); // 🔐 Valida si el usuario está autenticado con su token
app.UseAuthorization();
app.MapControllers();

app.Run();
