namespace Kafka.OffsetManagement;

internal sealed class IntegerArrayLinkedList
{
    private readonly LinkedList<long> _list = new();
    private readonly LinkedListNode<long>?[] _array;
    private readonly Queue<int> _freeAddresses = new();

    public IntegerArrayLinkedList(int capacity)
    {
        if (capacity < 1)
            throw new ArgumentException("Capacity must be greater than 0.", nameof(capacity));

        _array = new LinkedListNode<long>?[capacity];

        for (var i = 0; i < capacity; i++)
            _freeAddresses.Enqueue(i);
    }

    public int Add(long val)
    {
        if (_freeAddresses.Count is 0)
            throw new InvalidOperationException("The list is full.");

        var node = _list.AddLast(val);
        var address = _freeAddresses.Dequeue();
        _array[address] = node;
        return address;
    }

    public void Remove(int address)
    {
        if (!IsAddressValid(address))
            throw new ArgumentException("Bad address.", nameof(address));

        var node = _array[address];
        if (node is not null)
        {
            _array[address] = null;
            _list.Remove(node);
            _freeAddresses.Enqueue(address);
        }
    }

    public long? First()
    {
        return _list.First?.Value;
    }

    private bool IsAddressValid(int address)
    {
        return address >= 0 && address < _array.Length;
    }
}
