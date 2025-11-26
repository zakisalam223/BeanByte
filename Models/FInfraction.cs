using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace forum_aspcore.Models
{
    public class FInfraction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string InfractionID { get; set; }

        [Required]
        [BsonRepresentation(BsonType.ObjectId)]
        [Display(Name = "User ID")]
        public string GivenToUserID { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string GivenByUserID { get; set; }

        [Display(Name = "Date Given")]
        public DateTime DateGiven { get; set; }

        [Required]
        public string Reason { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Infraction points must be between 1 and 10.")]
        [Display(Name = "Infraction Points")]
        public int InfPointsGiven { get; set; }
    }
}
