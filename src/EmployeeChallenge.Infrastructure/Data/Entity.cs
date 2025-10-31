using System.ComponentModel.DataAnnotations;

namespace EmployeeChallenge.Infrastructure.Data;

public interface IAuditable
{
    DateTime CreatedAt { get; }
    DateTime? UpdatedAt { get; }
}

public interface IDocument
{
    Guid Id { get; }
}

public interface IHasVersion
{
    [Timestamp]
    byte[]? Version { get; }
}

public interface IEntity : IDocument { }

public class Document : IDocument
{
    public Guid Id { get; protected init; }
}

public abstract class Entity : IEntity, IHasVersion, IAuditable
{
    public Guid Id { get; protected init; }

    [Timestamp]
    public byte[]? Version { get; protected init; }

    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    public void SetAuditableAdd(DateTime createdAt)
    {
        CreatedAt = createdAt;
        UpdatedAt = null;
    }

    public void SetAuditableModified(DateTime modifiedAt)
    {
        UpdatedAt = modifiedAt;
    }
}
