// this could be the same and null other categories.  Then any fill out would change status front end and backend validation (?)
using System;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RoBrosRegistrantsService.Student
{
    public class Spectator
    {
        public Guid Id { get; set; }

        [Required]
        required public Guid RegistrantId { get; set; }

        [Required]
        required public int District { get; set; }

        [Required]
        public string ParticipantRole { get; set; } = "student";

        [Required]
        required public string CompetitionStatus { get; set; } = "Spectating"; // competing/spectating enum
    }

}
