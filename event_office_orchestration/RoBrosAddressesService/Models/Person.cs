using System;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using EventOfficeApi.Models;

namespace EventOfficeApi.Models
{
    public class Person
    {
        public Guid Id { get; set; }

        [Required]
        required public string GivenName { get; set; }

        [Required]
        required public string FamilyName { get; set; }

        [Required]
        required public string CompetitionStatus { get; set; }

        [Required]
        required public Church Church { get; set; }

        [Required]
        required public IAddress Address { get; set; }

        public string? Mobile { get; set; }

        public string? Email { get; set; }
    }

}