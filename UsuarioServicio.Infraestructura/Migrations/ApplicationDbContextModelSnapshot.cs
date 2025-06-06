﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using UsuarioServicio.Infraestructura.Persistencia;

#nullable disable

namespace UsuarioServicio.Infraestructura.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("UsuarioServicio.Dominio.Entidades.MovimientoUsuario", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Accion")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Detalles")
                        .HasColumnType("text");

                    b.Property<DateTime>("FechaHora")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("UsuarioId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.ToTable("MovimientosUsuario");
                });

            modelBuilder.Entity("UsuarioServicio.Dominio.Entidades.Privilegio", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("NombreTabla")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Operacion")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Privilegios");
                });

            modelBuilder.Entity("UsuarioServicio.Dominio.Entidades.Rol", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Descripcion")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("UsuarioServicio.Dominio.Entidades.RolPrivilegio", b =>
                {
                    b.Property<Guid>("RolId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("PrivilegioId")
                        .HasColumnType("uuid");

                    b.HasKey("RolId", "PrivilegioId");

                    b.HasIndex("PrivilegioId");

                    b.ToTable("RolPrivilegios");
                });

            modelBuilder.Entity("UsuarioServicio.Dominio.Entidades.Usuario", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Apellido")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Direccion")
                        .HasMaxLength(250)
                        .HasColumnType("character varying(250)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)");

                    b.Property<DateTime>("FechaCreacion")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("RolId")
                        .HasColumnType("uuid");

                    b.Property<string>("Telefono")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.HasIndex("RolId");

                    b.ToTable("Usuarios", (string)null);
                });

            modelBuilder.Entity("UsuarioServicio.Dominio.Entidades.RolPrivilegio", b =>
                {
                    b.HasOne("UsuarioServicio.Dominio.Entidades.Privilegio", "Privilegio")
                        .WithMany("RolPrivilegios")
                        .HasForeignKey("PrivilegioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("UsuarioServicio.Dominio.Entidades.Rol", "Rol")
                        .WithMany("RolPrivilegios")
                        .HasForeignKey("RolId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Privilegio");

                    b.Navigation("Rol");
                });

            modelBuilder.Entity("UsuarioServicio.Dominio.Entidades.Usuario", b =>
                {
                    b.HasOne("UsuarioServicio.Dominio.Entidades.Rol", "Rol")
                        .WithMany()
                        .HasForeignKey("RolId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Rol");
                });

            modelBuilder.Entity("UsuarioServicio.Dominio.Entidades.Privilegio", b =>
                {
                    b.Navigation("RolPrivilegios");
                });

            modelBuilder.Entity("UsuarioServicio.Dominio.Entidades.Rol", b =>
                {
                    b.Navigation("RolPrivilegios");
                });
#pragma warning restore 612, 618
        }
    }
}
