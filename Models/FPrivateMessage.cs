using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace forum_aspcore.Models
{
    public class FPrivateMessage
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string MessageID { get; set; }
        public string SenderID { get; set; }
        public string SenderUsername { get; set; }
        public string RecipientID { get; set; }
        public string RecipientUsername { get; set; }
        public string Content { get; set; }
        public bool Status { get; set; } // Read / Unread
        public DateTime DateSent { get; set; }
    }
}
