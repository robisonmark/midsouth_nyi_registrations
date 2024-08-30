using System;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using EventOfficeApi.Models.Registrant;

namespace EventOfficeApi.Models
{
    public class Student
    {
        [Required]
        public string ParticipantRole { get; set; } = "Student" // student/chaperone/volunteer

        [Required]
        public string CompetitionStatus { get; set; } // competing/spectation enum

        public string MedicalConditions { get; set; }

        public string DietaryRestrictions { get; set; }

        public Array[string] Allergies { get; set; }

        public Array[string] FoodAllergies { get; set; }

        public Array[string] Medications { get; set; } // possible { medicine: xx; dose: xx }

        [Required]
        public string GuardianFirstName { get; set; }

        [Required]
        public string GuardianLastName { get; set; }

        [Required]
        public string GuardianHomePhone { get; set; }

        [Required]
        public string GuardianWorkPhone { get; set; }

        [Required]
        public string GuardianContactPhone { get; set; } // possible id of above?

        [Required]
        public string InsuranceCompany { get; set; }

        [Required]
        public string PolicyId { get; set; }
    }

}

// Particpant
// [Required]
// required public string CompetitionStatus { get; set; }
