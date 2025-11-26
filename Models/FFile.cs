using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GridFS;

namespace forum_aspcore.Models
{
    public class FFile
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string FileID { get; set; }
        public bool Status { get; set; } // Verified / Unverified
        public string DownloadLink { get; set; }
        public DateTime UploadDate { get; set; }
        public string UploadedBy { get; set; }
        public string Filename { get; set; }
        public string ContentType { get; set; } // New

        public ObjectId GFSID { get; set; } // Grid FS ID which points to the structure containing binary and metadata

    }
}
