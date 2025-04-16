using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UsuarioServicio.Infraestructura.Persistencia
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // IMPORTANTE: Usa tu cadena de conexión real aquí
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=UsuariosDB;Username=postgres;Password=161171");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
