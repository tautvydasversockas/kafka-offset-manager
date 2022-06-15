namespace Kafka.OffsetManagement;

/// <summary>
/// Offset management error code.
/// </summary>
public enum KafkaOffsetManagementErrorCode
{
    /// <summary>
    /// No error.
    /// </summary>
    NoError = 0,

    /// <summary>
    /// Offset is out of order.
    /// </summary>
    OffsetOutOfOrder = 1
}
