namespace Kafka.OffsetManagement;

public class KafkaOffsetManagementException : Exception
{
    public static KafkaOffsetManagementException OffsetOutOfOrder(string message) =>
        new(KafkaOffsetManagementErrorCode.OffsetOutOfOrder, message);

    public KafkaOffsetManagementErrorCode ErrorCode { get; }

    internal KafkaOffsetManagementException(KafkaOffsetManagementErrorCode errorCode, string message)
       : base(message)
    {
        ErrorCode = errorCode;
    }
}
