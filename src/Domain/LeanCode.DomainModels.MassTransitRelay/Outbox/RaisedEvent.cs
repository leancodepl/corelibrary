using System;
using System.Diagnostics;
using LeanCode.DomainModels.Model;
using LeanCode.Time;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.MassTransitRelay.Outbox;

public sealed class RaisedEvent
{
    public const int MaxEventTypeLength = 500;

    public Guid Id { get; private set; }
    public DateTime DateOcurred { get; private set; }
    public string EventType { get; private set; }
    public string Payload { get; private set; }
    public RaisedEventMetadata Metadata { get; private set; }

    public bool WasPublished { get; set; }

    public static RaisedEvent Create(object evt, RaisedEventMetadata metadata, string serializedEvent)
    {
        var raisedEvt = new RaisedEvent
        {
            Metadata = metadata,
            Payload = serializedEvent,
            EventType = evt.GetType().FullName!,
        };

        if (evt is IDomainEvent domainEvent)
        {
            raisedEvt.Id = domainEvent.Id;
            raisedEvt.DateOcurred = domainEvent.DateOccurred;
        }
        else
        {
            raisedEvt.Id = Guid.NewGuid();
            raisedEvt.DateOcurred = TimeProvider.Now;
        }

        return raisedEvt;
    }

    // For tests
    public RaisedEvent(Guid id, RaisedEventMetadata metadata, DateTime dateOcurred, bool wasPublished, string eventType, string payload)
    {
        Id = id;
        Metadata = metadata;
        DateOcurred = dateOcurred;
        WasPublished = wasPublished;
        EventType = eventType;
        Payload = payload;
    }

    private RaisedEvent()
    {
        EventType = null!;
        Payload = null!;
        Metadata = null!;
    }

    public static void Configure(ModelBuilder builder)
    {
        builder.Entity<RaisedEvent>(cfg =>
        {
            cfg.HasKey(e => e.Id).IsClustered(false);
            cfg.HasIndex(e => new { e.DateOcurred, e.WasPublished }).IsClustered(true);
            cfg.OwnsOne(e => e.Metadata, inner =>
            {
                inner.Property(e => e.ActivityContext)
                    .HasConversion(new ActivityContextEFConverter());
            });

            cfg.Property(e => e.Id)
                .ValueGeneratedNever();

            cfg.Property(e => e.EventType)
                .HasMaxLength(MaxEventTypeLength);
        });
    }
}

public class RaisedEventMetadata
{
    public ActivityContext? ActivityContext { get; set; }
    public Guid? ConversationId { get; set; }
}
