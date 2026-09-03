using System.Collections.Concurrent;
using System.Threading.Channels;

namespace BewerbungsSeite.Services;

public interface IContactMessageNotificationQueue
{
    void Enqueue(Guid contactMessageId);

    IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken cancellationToken);

    void Release(Guid contactMessageId);
}

public class ContactMessageNotificationQueue : IContactMessageNotificationQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();
    private readonly ConcurrentDictionary<Guid, byte> _inFlight = new();

    public void Enqueue(Guid contactMessageId)
    {
        if (_inFlight.TryAdd(contactMessageId, 0))
        {
            _channel.Writer.TryWrite(contactMessageId);
        }
    }

    public IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);

    public void Release(Guid contactMessageId) => _inFlight.TryRemove(contactMessageId, out _);
}
