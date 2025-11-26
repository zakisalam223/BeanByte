using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace forum_aspcore.Models
{
    public class FReputationLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } 
        public string InitiatorUserID { get; set; } // The user who changes the reputation
        public string TargetUserID { get; set; }    // The user whose reputation is changed
        public DateTime LastChangeDate { get; set; } // The timestamp of the last change
    }
}
