using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsuarioServicio.Dominio.Entidades;

namespace UsuarioServicio.Aplicacion.DTOs
{
public class UserDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Email { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public Rol Rol { get; set; } // Puedes cambiar esto a RolDto si deseas desacoplar aún más
}
}
