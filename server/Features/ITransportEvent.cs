using MessagePack;

namespace WebTransportExample.Features;

[MessagePackObject(true)]
public class TransportEvent<T>
{
    [Key("sessionId")]
    public required Guid SessionId;

    [Key("payload")]
    public T? Payload;
}
