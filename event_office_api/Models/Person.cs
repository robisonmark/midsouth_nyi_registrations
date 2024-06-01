using EventOfficeApi.Models;

namespace EventOfficeApi.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string CompetitionStatus { get; set; }
        public Church Church { get; set; }
        public IAddress Address { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
    }

}