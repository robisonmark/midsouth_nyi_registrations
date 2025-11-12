using EventOfficeApi.RoBrosAddressesService.Models;

namespace RoBrosRegistrantsService.Models
{
    public class Church
    {
        public Guid? Id { get; set; }
        required public string Name { get; set; }
        public Address? Address { get; set; }

        // These should be put in by the Base Service. Look into how to do this.
        // For now, just set them in the Controller.
        public string? createdBy { get; set; }
        public DateTime? createdAt { get; set; }
        public string? updatedBy { get; set; }
        public DateTime? updatedAt { get; set; }

        // Do we need small church/large church division?
        public Church()
        {
            Id = Guid.NewGuid();
            Name = string.Empty;
        }
    }
}