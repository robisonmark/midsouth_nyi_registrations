using System;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using EventOfficeApi.Models;

namespace EventOfficeApi.Models
{
    public class Registrant
    {
        public Guid Id { get; set; }

        [Required]
        required public string GivenName { get; set; }

        [Required]
        required public string FamilyName { get; set; }

        [Required]
        public string ParticpantRole { get; set; } // student/chaperone/volunteer

        [Required]
        public string CompetitionStatus { get; set; } // competing/spectation enum

        [Required]
        required public Church Church { get; set; }

        public string YouthLeaderEmail { get; set; } // part of church?

        public string YouthLeaderFirstName { get; set; } // part of church?

        public string YouthLeaderLastName { get; set; }  // part of church?

        [Required]
        required public IAddress Address { get; set; }

        public string? Mobile { get; set; }

        public EmailAddress? Email { get; set; }

        [Required]
        public DateTime Birthday { get; set; }

        public string Gender { get; set; }

        public string ShirtSize { get; set; } // this should be an enum

        public float Price { get; set; }

        [required]
        required public bool Paid { get; set; }

        public string Notes { get; set; }

        [Required]
        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string IPAddress { get; set; }



    }

}

// Particpant
// [Required]
// required public string CompetitionStatus { get; set; }
