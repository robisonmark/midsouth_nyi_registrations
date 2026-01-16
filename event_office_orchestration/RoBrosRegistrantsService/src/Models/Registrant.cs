using System;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using EventOfficeApi.RoBrosAddressesService.Models;
using RoBrosRegistrantsService.Models;

namespace RoBrosRegistrantsService.Models
{
    public class Registrant
    {
        public Guid Id { get; set; }


        [Required]
        public string CompetitionStatus { get; set; } = "Spectating"; // competing/spectating enum
        [Required]
        public string ParticipantRole { get; set; } = "Chaperone"; // competing/spectating enum


        [Required]
        required public string GivenName { get; set; }

        [Required]
        required public string FamilyName { get; set; }

        [Required]
        required public Church Church { get; set; } // Can be supplied as full object or as a string name in JSON
        
        public Guid? ChurchId { get; set; }

        public string YouthLeaderEmail { get; set; } // part of church?

        public string YouthLeaderFirstName { get; set; } // part of church?

        public string YouthLeaderLastName { get; set; }  // part of church?

        // [Required]
        required public CreateAddressRequest Address { get; set; }

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
