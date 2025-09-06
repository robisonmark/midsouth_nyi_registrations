using System;
public interface IAddress
{
    string StreetAddress1 { get; set; }
    string? StreetAddress2 { get; set; }
    string Locality { get; set; }
    int PostalCode { get; set; }
    string Country { get; set; }
    string AdministrativeAreaLevel { get; set; }

    Address NewAddress { set; }
}

// not sure this is the way to do this
public class Address : IAddress
{
    public string StreetAddress1 { get; set; }
    public string? StreetAddress2 { get; set; }
    public string Locality { get; set; } // city
    public int PostalCode { get; set; }
    public string Country { get; set; }
    public string AdministrativeAreaLevel { get; set; } // state

    // SETTER - figure out proper way to do this
    public Address NewAddress()
    {
        address = new Address();
        address.StreetAddress1 = string.Empty;
        address.Locality = string.Empty;
        address.PostalCode = 0;
        address.Country = string.Empty;
        address.AdministrativeAreaLevel = string.Empty;
        return address;
    }
}