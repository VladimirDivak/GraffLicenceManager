using MongoDB.Bson;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace GraffLicenceManager.DB {
    public class Computer {
        [BsonId] [BsonRepresentation(BsonType.ObjectId)]
        public string   id              { get; set; }
        public string   hardwareId      { get; set; }
        public string   sessionId       { get; set; }
        public string   productName     { get; set; }
        public string   activationDate  { get; set; }
        public string   machineName     { get; set; }
        public string   localUserName   { get; set; }
        public string   geolocation     { get; set; }
        public bool     isActive        { get; set; }
        public bool     isBanned        { get; set; }
        public string   lastActivity    { get; set; }
    }
}
 