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

        public string[] ArtEvents { get; set; } = Array.Empty<string>();

        public string[] CreativeMinistriesEvents { get; set; } = Array.Empty<string>();

        public string[] CreativeWritingEvents { get; set; } = Array.Empty<string>();

        public string[] SpeechEvents { get; set; } = Array.Empty<string>();

        public string[] AcademicEvents { get; set; } = Array.Empty<string>();

        public string[] VocalMusicEvents { get; set; } = Array.Empty<string>();

        public string[] InstrumentalMusicEvents { get; set; } = Array.Empty<string>();

        public string[] IndividualSportEvents { get; set; } = Array.Empty<string>();

        public string[] TeamSportEvents { get; set; } = Array.Empty<string>();

        [Required]
        required public bool AttendingTNTatTNU { get; set; }
    }

}
