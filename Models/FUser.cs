using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace forum_aspcore.Models
{
    public class FUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserID { get; set; }

        public string? AdminID { get; set; }       // Made nullable
        public string? UCID { get; set; }             // Made nullable
        public int? ProfRating { get; set; }       // Made nullable
        public string? Course { get; set; }      // Can remain as is (nullable)
        public string? Title { get; set; }         // Can remain as is (nullable)
        public DateTime? JoinDate { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }


        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? Signature { get; set; }
        public string? Bio { get; set; }
        public int? RepPower { get; set; }       // Made nullable if not required
        public int? RepPoints { get; set; }      // Made nullable if not required
        public string? Degree { get; set; }
        public int? YearOfDegree { get; set; }   // Made nullable
        public int? InfractionPoints { get; set; } // Made nullable

        public ObjectId? GFSID_PFP { get; set; } // Made nullable

        public bool IsBanned { get; set; } = false; // new

    }
}
