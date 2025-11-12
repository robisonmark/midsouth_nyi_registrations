using System;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RoBrosRegistrantsService.Models
{
    public class Competitor : Student
    {
        public Competitor()
        {
            // Competitor is a Student participant by definition
            ParticipantRole = "Student";
            CompetitionStatus = "Competing";
        }

        [Required]
        required public int District { get; set; }

        // Inherits CompetitionStatus from Student; default set in ctor above

        required public bool Quizzing { get; set; } = false;

        public string[] ArtCategories { get; set; } = Array.Empty<string>();

        public string[] CreativeMinistriesCategories { get; set; } = Array.Empty<string>();

        public string[] CreativeWritingCategories { get; set; } = Array.Empty<string>();

        public string[] SpeechCategories { get; set; } = Array.Empty<string>();

        public string[] AcademicCategories { get; set; } = Array.Empty<string>();

        public string[] VocalMusicCategories { get; set; } = Array.Empty<string>();

        public string[] InstrumentalMusicCategories { get; set; } = Array.Empty<string>();

        public string[] IndividualSportCategories { get; set; } = Array.Empty<string>();

        public string[] TeamSportCategories { get; set; } = Array.Empty<string>();

        [Required]
        required public bool AttendingTNTatTNU { get; set; }
    }

}
