namespace Kafka.OffsetManagement;

/// <summary>
/// Manages out of order offsets.
/// </summary>
public class OffsetManager : IDisposable
{
    /// <summary>
    /// Interval between unsuccessfull reset tries.
    /// </summary>
    public TimeSpan ResetCheckInterval { get; set; } = TimeSpan.FromMilliseconds(500);

    private readonly object _lock = new();
    private readonly IntegerArrayLinkedList _unackedOffsets;
    private readonly SemaphoreSlim _addSemaphore;

    private long? _lastOffset;
    private bool _disposed;

    /// <summary>
    /// Creates offset manager.
    /// </summary>
    /// <param name="maxOutstanding">Max number of unacknowledged offsets at the same time.</param>
    public OffsetManager(int maxOutstanding)
    {
        _unackedOffsets = new IntegerArrayLinkedList(maxOutstanding);
        _addSemaphore = new SemaphoreSlim(maxOutstanding, maxOutstanding);
    }

    /// <summary>
    /// Returns offset acknowledgement ID
    /// that can later be used to acknowledge the offset.
    /// Waits if offset manager has maxOutstanding unacknowledged offsets.
    /// </summary>
    public async Task<AckId> GetAckIdAsync(long offset, CancellationToken token = default)
    {
        await _addSemaphore.WaitAsync(token);

        lock (_lock)
        {
            UpdateLastOffset(offset);
            return _unackedOffsets.Add(offset);
        }
    }

    /// <summary>
    /// Acknowledges.
    /// </summary>
    public void Ack(AckId ackId)
    {
        lock (_lock)
        {
            _unackedOffsets.Remove(ackId);
        }

        _addSemaphore.Release();
    }

    /// <summary>
    /// Marks offset as acknowledged. Can only be used in a sequential manner.
    /// </summary>
    public void MarkAsAcked(long offset)
    {
        lock (_lock)
        {
            UpdateLastOffset(offset);
        }
    }

    /// <summary>
    /// Returns offset that can be safely commited. 
    /// Returns null if no offset can be commited safely.
    /// </summary>
    public long? GetCommitOffset()
    {
        lock (_lock)
        {
            return _unackedOffsets.First() ?? _lastOffset + 1;
        }
    }

    /// <summary>
    /// Waits until there are no unacknowledged offsets 
    /// and resets current manager instance to the initial state.
    /// </summary>
    public async Task ResetAsync(CancellationToken token = default)
    {
        while (!token.IsCancellationRequested)
        {
            lock (_lock)
            {
                if (TryReset())
                    return;
            }

            await Task.Delay(ResetCheckInterval, token);
        }
    }

    private void UpdateLastOffset(long offset)
    {
        if (offset <= _lastOffset)
            throw KafkaOffsetManagementException.OffsetOutOfOrder(
                $"Offset {offset} must be greater than last added offset {_lastOffset}.");

        _lastOffset = offset;
    }

    private bool TryReset()
    {
        if (_unackedOffsets.First() is not null)
            return false;

        _lastOffset = null;
        return true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _addSemaphore.Dispose();
        }

        _disposed = true;
    }

    ~OffsetManager()
    {
        Dispose(false);
    }
}
