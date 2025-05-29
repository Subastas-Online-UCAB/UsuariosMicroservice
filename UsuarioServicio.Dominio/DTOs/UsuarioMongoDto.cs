using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace UsuarioServicio.Dominio.DTOs
{
    public class UsuarioMongoDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public Guid RolId { get; set; }

        // Nueva propiedad opcional para enriquecer el resultado
        public RolMongoDto? Rol { get; set; }
    }
}
