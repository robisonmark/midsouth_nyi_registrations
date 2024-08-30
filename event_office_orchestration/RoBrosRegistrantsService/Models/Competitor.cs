using System;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using EventOfficeApi.Models.Registrant;

namespace EventOfficeApi.Student
{
    public class Competitor
    {
        [Required]
        required public int District { get; set; }
        [Required]
        required public string CompetitionStatus { get; set; } = "Competiting"; // competing/spectation enum

        [Required]
        required public bool Quzzing { get; set; } = false;

        public Array[string] ArtCategories { get; set; }

        public Array[string] CreativeMinistriesCategories { get; set; }

        public Array[string] CreativeWritingCategories { get; set; }

        public Array[string] SpeechCategories { get; set; }

        public Array[string] AcademicCategories { get; set; }

        public Array[string] VocalMusicCategories { get; set; }

        public Array[string] InstrumentalMusicCategories { get; set; }

        public Array[string] IndividualSportCategories { get; set; }

        public Array[string] TeamSportCategories { get; set; }

        [Required]
        required public bool AttendingTNTatTNU { get; set; }
    }

}
