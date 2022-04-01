using LeanCode.Time;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.MassTransitRelay.Inbox
{
    public class ConsumedMessage
    {
        public Guid MessageId { get; private set; }
        public DateTime DateConsumed { get; private set; }
        public string ConsumerType { get; private set; }
        public string MessageType { get; private set; }

        private ConsumedMessage()
        {
            ConsumerType = null!;
            MessageType = null!;
        }

        public ConsumedMessage(Guid messageId, DateTime dateConsumed, string consumerType, string messageType)
        {
            MessageId = messageId;
            DateConsumed = dateConsumed;
            ConsumerType = consumerType;
            MessageType = messageType;
        }

        public static ConsumedMessage Create<TConsumer, TMessage>(ConsumerConsumeContext<TConsumer, TMessage> context)
            where TConsumer : class
            where TMessage : class
        {
            return new ConsumedMessage
            {
                MessageId = context.MessageId ?? throw new InvalidOperationException("Message does not have an id"),
                ConsumerType = typeof(TConsumer).FullName!,
                MessageType = typeof(TMessage).FullName!,
                DateConsumed = TimeProvider.Now,
            };
        }

        public static void Configure(ModelBuilder model)
        {
            model.Entity<ConsumedMessage>(cfg =>
            {
                cfg.HasKey(e => new { e.MessageId, e.ConsumerType });
                cfg.HasIndex(e => e.DateConsumed);

                cfg.Property(e => e.MessageId)
                    .ValueGeneratedNever();

                cfg.Property(e => e.ConsumerType)
                    .HasMaxLength(500);

                cfg.Property(e => e.MessageType)
                    .HasMaxLength(500);
            });
        }
    }
}
