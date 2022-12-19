using MongoDB.Bson;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace GraffLicenceManager.DB
{
    [System.Serializable]
    public class ProjectStateHanlder
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public int Id { get; set; }
        public string projectName { get; set; }
        public List<State> states { get; set; }
    }
}
