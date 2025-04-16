using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UsuarioServicio.Dominio.Entidades;

namespace UsuarioServicio.Infraestructura.Persistencia
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Rol> Roles { get; set; }
        public DbSet<Privilegio> Privilegios { get; set; }
        public DbSet<RolPrivilegio> RolPrivilegios { get; set; }


        // Esta es la tabla que vamos a crear en la base de datos
        public DbSet<Usuario> Usuarios { get; set; }

        // Opcional, pero profesional: configuramos la tabla
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de la tabla Usuario (esto ya lo tienes)
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.PasswordHash)
                    .IsRequired();

                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Telefono)
                    .HasMaxLength(50);

                entity.Property(e => e.Direccion)
                    .HasMaxLength(250);


                // 🔥 Relación FK con Rol
                entity.HasOne(u => u.Rol)
                    .WithMany()
                    .HasForeignKey(u => u.RolId)
                    .OnDelete(DeleteBehavior.Restrict); // Opcional: evitar que se borren en cascada
            });

            // 🔥 Agregas aquí la configuración de RolPrivilegio (muchos a muchos)

            modelBuilder.Entity<RolPrivilegio>()
                .HasKey(rp => new { rp.RolId, rp.PrivilegioId });

            modelBuilder.Entity<RolPrivilegio>()
                .HasOne(rp => rp.Rol)
                .WithMany(r => r.RolPrivilegios)
                .HasForeignKey(rp => rp.RolId);

            modelBuilder.Entity<RolPrivilegio>()
                .HasOne(rp => rp.Privilegio)
                .WithMany(p => p.RolPrivilegios)
                .HasForeignKey(rp => rp.PrivilegioId);
        }

    }
}
