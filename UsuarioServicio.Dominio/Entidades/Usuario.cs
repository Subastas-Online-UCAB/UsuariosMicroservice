using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;


namespace UsuarioServicio.Dominio.Entidades
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public string? Telefono { get; set; }
        public string? Direccion { get; set; }

        // Relación con Rol
        public Guid RolId { get; set; } // FK hacia Rol
        public Rol Rol { get; set; } // Propiedad de navegación

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Método estático para centralizar la creación de Usuario
        public static Usuario Crear(string nombre, string apellido, string email, string passwordPlano, string telefono, string direccion, Guid rolId)
        {
            return new Usuario
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                Apellido = apellido,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordPlano),
                Telefono = telefono,
                Direccion = direccion,
                RolId = rolId,
                FechaCreacion = DateTime.UtcNow
            };
        }
    }
}