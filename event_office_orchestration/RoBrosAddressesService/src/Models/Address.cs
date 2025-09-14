using System.ComponentModel.DataAnnotations;

namespace EventOfficeApi.RoBrosAddressesService.Models;

public class Address
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(255)]
    public string StreetAddress1 { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? StreetAddress2 { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty; // Locality
    
    [Required]
    [MaxLength(100)]
    public string State { get; set; } = string.Empty; // AdministrativeAreaLevel - how can I map these to match
    
    [Required]
    [MaxLength(20)]
    public string PostalCode { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = "USA";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property for entity mappings - I am not sure this is how to handle this
    public virtual ICollection<AddressEntityMapping> EntityMappings { get; set; } = new List<AddressEntityMapping>();
}

public class AddressEntityMapping
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid AddressId { get; set; }
    
    [Required]
    public Guid EntityId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? AddressType { get; set; } // e.g., "billing", "shipping", "primary"
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual Address Address { get; set; } = null!;
}

public class CreateAddressRequest
{
    [Required]
    public string StreetAddress1 { get; set; } = string.Empty;
    
    public string? StreetAddress2 { get; set; }
    
    [Required]
    public string City { get; set; } = string.Empty; // locality
    
    [Required]
    public string State { get; set; } = string.Empty; // administrative area level
    
    [Required]
    public string PostalCode { get; set; } = string.Empty;
    
    [Required]
    public string Country { get; set; } = "USA";
}

public class UpdateAddressRequest
{
    public string? StreetAddress1 { get; set; }
    public string? StreetAddress2 { get; set; }
    public string? City { get; set; } // locality
    public string? State { get; set; } // administrative area level
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
}