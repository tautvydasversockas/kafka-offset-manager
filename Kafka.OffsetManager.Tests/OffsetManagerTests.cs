using FluentAssertions;
using System.Threading.Tasks;
using Xunit;

namespace Kafka.OffsetManager.Tests
{
    public sealed class OffsetManagerTests
    {
        [Fact]
        public async Task Getting_offset_after_consecutively_acknowledged_offsets()
        {
            var sut = new OffsetManager(10);

            var ackId1 = await sut.GetAckIdAsync(1);
            var ackId2 = await sut.GetAckIdAsync(2);
            var ackId3 = await sut.GetAckIdAsync(3);

            sut.Ack(ackId1);
            sut.Ack(ackId2);

            var commitOffset = sut.GetCommitOffset();

            commitOffset.Should().Be(2);
        }

        [Fact]
        public async Task Getting_offset_after_non_consecutively_acknowledged_offsets()
        {
            var sut = new OffsetManager(10);

            var ackId1 = await sut.GetAckIdAsync(1);
            var ackId2 = await sut.GetAckIdAsync(2);
            var ackId3 = await sut.GetAckIdAsync(3);

            sut.Ack(ackId1);
            sut.Ack(ackId3);

            var commitOffset = sut.GetCommitOffset();

            commitOffset.Should().Be(1);
        }

        [Fact]
        public async Task Getting_offset_after_out_of_order_acknowledged_offsets()
        {
            var sut = new OffsetManager(10);

            var ackId1 = await sut.GetAckIdAsync(1);
            var ackId2 = await sut.GetAckIdAsync(2);
            var ackId3 = await sut.GetAckIdAsync(3);

            sut.Ack(ackId3);
            sut.Ack(ackId1);

            var commitOffset = sut.GetCommitOffset();

            commitOffset.Should().Be(1);
        }
    }
}
