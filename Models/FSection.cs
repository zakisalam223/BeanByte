using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace forum_aspcore.Models
{
    public class FSection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string SectionID { get; set; }
        public string SectionName { get; set; }
    }
}
