namespace BallsWar.Events;

public class EventBus
{
    private readonly Queue<object> _events = new();

    public void Publish<T>(T evt) where T : notnull => _events.Enqueue(evt);

    public IReadOnlyList<object> Drain()
    {
        if (_events.Count == 0) return Array.Empty<object>();
        var list = new List<object>(_events.Count);
        while (_events.Count > 0) list.Add(_events.Dequeue());
        return list;
    }

    public void Clear() => _events.Clear();
}
