using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Serializers;

namespace UsuarioServicio.Infraestructura.MongoDB.Documentos
{
    public class MovimientoUsuarioMongo
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string UsuarioId { get; set; } = null!; // 👈 CAMBIADO a string
        public string Accion { get; set; } = null!;
        public string? Detalles { get; set; }
        public DateTime FechaHora { get; set; }
    }
}
