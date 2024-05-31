using EventOfficeApi.Models.Address;

namespace EventOfficeApi.Models
{
    public class Church
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
    }

}