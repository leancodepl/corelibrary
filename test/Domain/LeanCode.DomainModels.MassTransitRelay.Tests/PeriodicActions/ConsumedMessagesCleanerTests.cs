using System;
using System.Threading.Tasks;
using LeanCode.DomainModels.MassTransitRelay.Inbox;
using LeanCode.Time;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.PeriodicActions
{
    public class ConsumedMessagesCleanerTests : DbTestBase
    {
        private static readonly DateTime Now = new(2021, 4, 13, 12, 0, 0);
        private readonly ConsumedMessagesCleaner cleaner;

        public ConsumedMessagesCleanerTests()
        {
            FixedTimeProvider.SetTo(Now);
            cleaner = new(DbContext);
        }

        [Fact]
        public async Task Removes_old_consumed_message_entry()
        {
            var msgId = await PrepareConsumedMessageAsync(Now.AddDays(-5));

            await cleaner.ExecuteAsync(default);

            Assert.False(await ExistsAsync(msgId));
        }

        [Fact]
        public async Task Does_not_remove_fresh_consumed_message_entry()
        {
            var msgId = await PrepareConsumedMessageAsync(Now.AddMinutes(-5));

            await cleaner.ExecuteAsync(default);

            Assert.True(await ExistsAsync(msgId));
        }

        private Task<bool> ExistsAsync(Guid msgId) => DbContext.ConsumedMessages.AnyAsync(m => m.MessageId == msgId);

        private async Task<Guid> PrepareConsumedMessageAsync(DateTime dateConsumed)
        {
            var msg = new ConsumedMessage(Guid.NewGuid(), dateConsumed, "TestConsumer", "TestMessage");

            DbContext.ConsumedMessages.Add(msg);
            await DbContext.SaveChangesAsync();

            return msg.MessageId;
        }
    }
}
