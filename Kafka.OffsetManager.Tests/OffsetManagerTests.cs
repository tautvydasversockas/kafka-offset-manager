using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Kafka.OffsetManager.Tests
{
    public sealed class OffsetManagerTests
    {
        [Theory]
        [InlineData(new long[] { 5, 6, 7 }, new long[] { 5, 6 }, 6)]
        [InlineData(new long[] { 5, 6, 7 }, new long[] { 5, 7 }, 5)]
        [InlineData(new long[] { 5, 6, 7 }, new long[] { 7, 5 }, 5)]
        public async Task Getting_commitable_offset(long[] offsets, long[] ackOffsets, long expectedOffset)
        {
            using var sut = new OffsetManager(100);

            var offsetAckIds = new Dictionary<long, AckId>();

            foreach (var offset in offsets)
                offsetAckIds[offset] = await sut.GetAckIdAsync(offset);

            foreach (var offset in ackOffsets)
                sut.Ack(offsetAckIds[offset]);

            var commitOffset = sut.GetCommitOffset();

            commitOffset.Should().Be(expectedOffset);
        }
    }
}
