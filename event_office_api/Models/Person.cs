using EventOfficeApi.Models.Address;
using EventOfficeApi.Models.Church;

namespace EventOfficeApi.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string CompetitionStatus { get; set; }
        public Church.Id Church { get; set; }
        public Address Address { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
    }

}