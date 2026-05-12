using Karakol.Domain.Events;

namespace Karakol.Correlation.Engine;

public sealed class EventWindow(TimeSpan windowSize, int relatedEventLimit)
{
    private readonly Queue<SecurityEvent> _events = new();

    public IReadOnlyCollection<SecurityEvent> Events => _events.ToArray();

    public void Add(SecurityEvent securityEvent)
    {
        _events.Enqueue(securityEvent);
        Evict(securityEvent.Timestamp);
    }

    private void Evict(DateTimeOffset? currentTimestamp)
    {
        if (currentTimestamp is null)
        {
            while (_events.Count > relatedEventLimit)
            {
                _events.Dequeue();
            }

            return;
        }

        while (_events.Count > 0)
        {
            var candidate = _events.Peek();
            if (candidate.Timestamp is null || currentTimestamp.Value - candidate.Timestamp.Value <= windowSize)
            {
                break;
            }

            _events.Dequeue();
        }

        while (_events.Count > relatedEventLimit)
        {
            _events.Dequeue();
        }
    }
}
