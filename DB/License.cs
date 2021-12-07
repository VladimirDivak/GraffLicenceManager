using MongoDB.Bson;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace GraffLicenceManager.DB {
    public class License {   
        [BsonId] [BsonRepresentation(BsonType.ObjectId)]
        public string   id                  { get; set; }
        public string   iconPath            { get; set; }
        public string   registrationDate    { get; set; }
        public string   companyName         { get; set; }
        public string   productName         { get; set; }
        public bool     status              { get; set; }
        public int      trialPeriod         { get; set; }
        public int      licensesCounter     { get; set; }
    }
}