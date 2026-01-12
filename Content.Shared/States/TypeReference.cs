namespace Content.Shared.States;

public record struct TypeReference
{
    public string Type { get; }

    public TypeReference(string type)
    {
        Type = type;
    }
    
    public static implicit operator TypeReference(string type) => new(type);
    public static implicit operator string(TypeReference type) => type.Type;
}