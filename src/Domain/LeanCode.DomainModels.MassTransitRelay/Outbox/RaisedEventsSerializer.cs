using System;

namespace LeanCode.DomainModels.MassTransitRelay.Outbox
{
    public interface IRaisedEventsSerializer
    {
        object ExtractEvent(RaisedEvent evt);
        RaisedEvent WrapEvent(object evt);
    }
}
