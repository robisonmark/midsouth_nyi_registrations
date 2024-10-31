using System;
public interface IAddress
{
    string StreetAddress1 { get; set; }
    string? StreetAddress2 { get; set; }
    string Locality { get; set; }
    int PostalCode { get; set; }
    string Country { get; set; }
    string AdministrativeAreaLevel { get; set; }
<<<<<<< HEAD
}

// not sure this is the way to do this
public class Address : IAddress
{
    public string StreetAddress1 { get; set; }
    public string? StreetAddress2 { get; set; }
    public string Locality { get; set; }
    public int PostalCode { get; set; }
    public string Country { get; set; }
    public string AdministrativeAreaLevel { get; set; }
=======
>>>>>>> aaadf4e (Feature/addresses service (#8))
}