using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UsuarioServicio.Infraestructura.MongoDB.Documentos
{
    public class UsuarioMongo
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)] // Usamos string para que coincida con el ID de Keycloak o la base relacional
        public Guid UsuarioId { get; set; }

        public string Nombre { get; set; }

        public string Apellido { get; set; }

        public string Email { get; set; }

        public DateTime FechaCreacion { get; set; }

        public string? Telefono { get; set; }
        public string? Direccion { get; set; }

        // Relación con Rol
        public string RolId { get; set; } // FK hacia Rol
    }
}
