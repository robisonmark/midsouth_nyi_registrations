using System;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EventOfficeApi.Models
{
    public class Student : Registrant
    {
<<<<<<< HEAD
=======
        public Guid Id { get; set; }

        [Required]
        required public Guid RegistrantId { get; set; }

        [Required]
        public string ParticpantRole { get; set; } = "Student"; // student/chaperone/volunteer

>>>>>>> aaadf4e (Feature/addresses service (#8))
        [Required]
        public string CompetitionStatus { get; set; } // competing/spectation enum

        public string MedicalConditions { get; set; }

        public string DietaryRestrictions { get; set; }

        public string[] Allergies { get; set; }

        public string[] FoodAllergies { get; set; }

        public string[] Medications { get; set; } // possible { medicine: xx; dose: xx }

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
