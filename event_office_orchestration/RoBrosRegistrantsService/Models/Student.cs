using System;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RoBrosRegistrantsService.Models
{
    public class Student : Registrant
    {
        [Required]
        required public Guid RegistrantId { get; set; }

        [Required]
        public string CompetitionStatus { get; set; } = "Spectating"; // competing/spectating enum

        public Student()
        {
            // Students are participants by default
            ParticipantRole = "Student";
            CompetitionStatus = "Spectating";
        }

    public string? MedicalConditions { get; set; } = string.Empty;

    public string? DietaryRestrictions { get; set; } = string.Empty;

    public string[] Allergies { get; set; } = Array.Empty<string>();

    public string[] FoodAllergies { get; set; } = Array.Empty<string>();

    public string[] Medications { get; set; } = Array.Empty<string>(); // possible { medicine: xx; dose: xx }

    [Required]
    public string GuardianFirstName { get; set; } = string.Empty;

    [Required]
    public string GuardianLastName { get; set; } = string.Empty;

    [Required]
    public string GuardianHomePhone { get; set; } = string.Empty;

    [Required]
    public string GuardianWorkPhone { get; set; } = string.Empty;

    [Required]
    public string GuardianContactPhone { get; set; } = string.Empty; // possible id of above?

    [Required]
    public string InsuranceCompany { get; set; } = string.Empty;

    [Required]
    public string PolicyId { get; set; } = string.Empty;
    }

}

// Particpant
// [Required]
// required public string CompetitionStatus { get; set; }
