namespace EventOfficeApi.Models
{
    public class Church
    {
        public int Id { get; set; }
        required public string Name { get; set; }
        public IAddress? Address { get; set; }
    }

}