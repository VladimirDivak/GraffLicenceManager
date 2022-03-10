using MongoDB.Bson;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace GraffLicenceManager.DB
{
    public class Admin
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string   id          { get; set; }
        public string   login       { get; set; }
        public string   password    { get; set; }
    }
}