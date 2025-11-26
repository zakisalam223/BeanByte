using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace forum_aspcore.Models
{
    public class FTag
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string TagID { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}