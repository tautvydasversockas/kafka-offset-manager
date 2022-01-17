namespace Kafka.OffsetManagement;

public struct AckId : IEquatable<AckId>
{
    public int Value { get; }

    public AckId(int value)
    {
        Value = value;
    }

    public static implicit operator int(AckId ackId)
    {
        return ackId.Value;
    }

    public static implicit operator AckId(int value)
    {
        return new(value);
    }

    public override bool Equals(object? obj)
    {
        return obj is AckId ackId && Equals(ackId);
    }

    public bool Equals(AckId other)
    {
        return other.Value == Value;
    }

    public static bool operator ==(AckId a, AckId b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(AckId a, AckId b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
