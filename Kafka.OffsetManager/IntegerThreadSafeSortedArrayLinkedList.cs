using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.OffsetManager
{
    internal sealed class IntegerThreadSafeSortedArrayLinkedList
    {
        public TimeSpan ResetCheckInterval { get; set; } = TimeSpan.FromMilliseconds(500);

        private readonly object _lock = new();
        private readonly IntegerArrayLinkedList _list;
        private readonly SemaphoreSlim _addSemaphore;

        private long? _lastAddedVal;

        public IntegerThreadSafeSortedArrayLinkedList(int capacity)
        {
            _list = new IntegerArrayLinkedList(capacity);
            _addSemaphore = new SemaphoreSlim(capacity, capacity);
        }

        public async Task<int> AddAsync(long val, CancellationToken token = default)
        {
            await _addSemaphore.WaitAsync(token);

            lock (_lock)
            {
                if (val <= _lastAddedVal)
                    throw new ArgumentException(
                        $"Value {val} must be greater than last added value {_lastAddedVal}.", nameof(val));

                _lastAddedVal = val;
                return _list.Add(val);
            }
        }

        public void Remove(int address)
        {
            lock (_lock)
            {
                _list.Remove(address);
            }

            _addSemaphore.Release();
        }

        public long? First()
        {
            lock (_lock)
            {
                return _list.First();
            }
        }

        public long? LastAdded()
        {
            lock (_lock)
            {
                return _lastAddedVal;
            }
        }

        public async Task ResetAsync(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                lock (_lock)
                {
                    if (_list.First() is null)
                    {
                        _lastAddedVal = null;
                        return;
                    }
                }

                await Task.Delay(ResetCheckInterval, token);
            }
        }
    }
}