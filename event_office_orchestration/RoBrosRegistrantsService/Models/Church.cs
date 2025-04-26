namespace EventOfficeApi.Models
{
    public class Church
    {
        public Guid? Id { get; set; }
        required public string Name { get; set; }
        public Address? Address { get; set; }
    }

}