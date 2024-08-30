// this could be the same and null other categories.  Then any fill out would change status front end and backend validation (?)
using System;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EventOfficeApi.Student
{
    public class Spectator
    {
        [Required]
        required public int District { get; set; }
        [Required]
        required public string CompetitionStatus { get; set; } = "Spectating"; // competing/spectation enum
    }

}
