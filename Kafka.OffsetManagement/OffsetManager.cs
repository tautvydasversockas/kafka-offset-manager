using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.OffsetManagement
{
    public sealed class OffsetManager : IDisposable
    {
        public TimeSpan ResetCheckInterval { get; set; } = TimeSpan.FromMilliseconds(500);

        private readonly object _lock = new();
        private readonly IntegerArrayLinkedList _unackedOffsets;
        private readonly SemaphoreSlim _addSemaphore;

        private long? _lastAddedOffset;

        public OffsetManager(int maxOutstanding)
        {
            _unackedOffsets = new IntegerArrayLinkedList(maxOutstanding);
            _addSemaphore = new SemaphoreSlim(maxOutstanding, maxOutstanding);
        }

        public async Task<AckId> GetAckIdAsync(long offset, CancellationToken token = default)
        {
            await _addSemaphore.WaitAsync(token);

            lock (_lock)
            {
                if (offset <= _lastAddedOffset)
                    throw KafkaOffsetManagementException.OffsetOutOfOrder(
                        $"Offset {offset} must be greater than last added offset {_lastAddedOffset}.");

                _lastAddedOffset = offset;
                return _unackedOffsets.Add(offset);
            }
        }

        public void Ack(AckId ackId)
        {
            lock (_lock)
            {
                _unackedOffsets.Remove(ackId);
            }

            _addSemaphore.Release();
        }

        public void MarkAsAcked(long offset)
        {
            lock (_lock)
            {
                if (offset <= _lastAddedOffset)
                    throw KafkaOffsetManagementException.OffsetOutOfOrder(
                        $"Offset {offset} must be greater than last added offset {_lastAddedOffset}.");

                _lastAddedOffset = offset;
            }
        }

        public long? GetCommitOffset()
        {
            lock (_lock)
            {
                return _unackedOffsets.First() ?? _lastAddedOffset + 1;
            }
        }

        public async Task ResetAsync(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                lock (_lock)
                {
                    if (_unackedOffsets.First() is null)
                    {
                        _lastAddedOffset = null;
                        return;
                    }
                }

                await Task.Delay(ResetCheckInterval, token);
            }
        }

        public void Dispose()
        {
            _addSemaphore.Dispose();
        }
    }
}