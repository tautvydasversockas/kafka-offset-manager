using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.OffsetManager
{
    public sealed class OffsetManager : IDisposable
    {
        private readonly IntegerThreadSafeSortedArrayLinkedList _unackedOffsets;

        public OffsetManager(int maxOutstanding)
        {
            _unackedOffsets = new IntegerThreadSafeSortedArrayLinkedList(maxOutstanding);
        }

        public OffsetManager(int maxOutstanding, TimeSpan resetCheckInterval)
            : this(maxOutstanding)
        {
            _unackedOffsets.ResetCheckInterval = resetCheckInterval;
        }

        public async Task<AckId> GetAckIdAsync(long offset, CancellationToken token = default)
        {
            return await _unackedOffsets.AddAsync(offset, token);
        }

        public void Ack(AckId ackId)
        {
            _unackedOffsets.Remove(ackId);
        }

        public long? GetCommitOffset()
        {
            var firstUnackedOffset = _unackedOffsets.First();
            return firstUnackedOffset is null
                ? _unackedOffsets.LastAdded()
                : firstUnackedOffset - 1;
        }

        public Task ResetAsync(CancellationToken token = default)
        {
            return _unackedOffsets.ResetAsync(token);
        }

        public void Dispose()
        {
            _unackedOffsets.Dispose();
        }
    }
}