using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UsuarioServicio.Infraestructura.MongoDB.Documentos
{
    public class RolPrivilegioMongo
    {

        public string RolId { get; set; }
        public string PrivilegioId { get; set; }
    }
}

