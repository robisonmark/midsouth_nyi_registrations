using System;
public interface IAddress
{
    string StreetAddress1 { get; set; }
    string? StreetAddress2 { get; set; }
    string Locality { get; set; }
    int PostalCode { get; set; }
    string Country { get; set; }
    string AdministrativeAreaLevel { get; set; }
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

    // Constructor to ensure required properties are set
    public Address()
    {
        StreetAddress1 = string.Empty;
        Locality = string.Empty;
        PostalCode = 0;
        Country = string.Empty;
        AdministrativeAreaLevel = string.Empty;
    }
}