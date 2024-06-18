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