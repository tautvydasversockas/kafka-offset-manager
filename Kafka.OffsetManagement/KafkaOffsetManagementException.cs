namespace Kafka.OffsetManagement;

/// <summary>
/// Offset management exception.
/// </summary>
[Serializable]
public class KafkaOffsetManagementException : Exception
{
    /// <summary>
    /// Returns offset out of order exception.
    /// </summary>
    public static KafkaOffsetManagementException OffsetOutOfOrder(string message) =>
        new(KafkaOffsetManagementErrorCode.OffsetOutOfOrder, message);

    /// <summary>
    /// Offset management error code.
    /// </summary>
    public KafkaOffsetManagementErrorCode ErrorCode { get; }

    internal KafkaOffsetManagementException(KafkaOffsetManagementErrorCode errorCode, string message)
       : base(message)
    {
        ErrorCode = errorCode;
    }
}
