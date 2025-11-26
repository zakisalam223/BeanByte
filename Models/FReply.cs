using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace forum_aspcore.Models
{
    public class FReply
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ReplyID { get; set; }
        [Required]
        public DateTime DatePosted { get; set; }
        public string ThreadID { get; set; }
        public string SectionID { get; set; }
        [Required]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID { get; set; }
        [Required]
        public string Content { get; set; }
    }
}