using System;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EventOfficeApi.Student
{
    public class Competitor
    {
        public Guid Id { get; set; }

        [Required]
        required public Guid RegistrantId { get; set; }

        [Required]
        required public int District { get; set; }
        [Required]
        required public string CompetitionStatus { get; set; } = "Competiting"; // competing/spectation enum

        [Required]
        required public bool Quzzing { get; set; } = false;

        public string[] ArtCategories { get; set; }

        public string[] CreativeMinistriesCategories { get; set; }

        public string[] CreativeWritingCategories { get; set; }

        public string[] SpeechCategories { get; set; }

        public string[] AcademicCategories { get; set; }

        public string[] VocalMusicCategories { get; set; }

        public string[] InstrumentalMusicCategories { get; set; }

        public string[] IndividualSportCategories { get; set; }

        public string[] TeamSportCategories { get; set; }

        [Required]
        required public bool AttendingTNTatTNU { get; set; }
    }

}
