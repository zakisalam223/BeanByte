using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace forum_aspcore.Models
{
    public class FThread
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ThreadID { get; set; } 
        public string Title { get; set; }
        public string Description { get; set; }
        public string Topic { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public string? SectionID { get; set; } 
        public string? UserID { get; set; } 
        public List<FReply>? Replies { get; set; } // nullable
        public List<string> TagIDs { get; set; } = new List<string>();
    }
}
