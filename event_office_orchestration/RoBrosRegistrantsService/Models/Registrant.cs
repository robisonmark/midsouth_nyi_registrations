using System;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using RoBrosRegistrantsService.Models;

namespace RoBrosRegistrantsService.Models
{
    public class Registrant
    {
        public Guid Id { get; set; }

        [Required]
        required public string GivenName { get; set; }

        [Required]
        required public string FamilyName { get; set; }

        [Required]
        required public Church Church { get; set; } // This needs to match a church in the database but should use a string to get it.
        
        public Guid? ChurchId { get; set; }

        [Required]
        public string ParticipantRole { get; set; } 

        public string YouthLeaderEmail { get; set; } // part of church?

        public string YouthLeaderFirstName { get; set; } // part of church?

        public string YouthLeaderLastName { get; set; }  // part of church?

        // [Required]
        required public Address Address { get; set; }

        public Guid AddressId { get; set; }

        public string? Mobile { get; set; }

        public string? Email { get; set; }

        [Required]
        public DateTime Birthday { get; set; }

        public string Gender { get; set; }

        public string ShirtSize { get; set; } // this should be an enum

        public float Price { get; set; }

        [Required]
        required public bool Paid { get; set; }

        public string Notes { get; set; }

        [Required]
        required public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;

        [Required]
        required public string IPAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string CreatedBy { get; set; } = "Mark";

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string UpdatedBy { get; set; } = "Mark";
    }
}

// Particpant
// [Required]
// required public string CompetitionStatus { get; set; }
