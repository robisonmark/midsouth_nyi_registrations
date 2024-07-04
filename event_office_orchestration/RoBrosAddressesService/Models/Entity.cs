namespace EventOfficeApi.Models
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public string CreatedBy { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public string UpdatedBy { get; protected set; }
    public int Version { get; protected set; }
    public string EntityData { get; protected set; }

    protected BaseEntity(string createdBy)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        Version = 0;
    }

    public void Update(string updatedBy, string entityData)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        EntityData = entityData;
        Version++;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is BaseEntity))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        var other = (BaseEntity)obj;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
