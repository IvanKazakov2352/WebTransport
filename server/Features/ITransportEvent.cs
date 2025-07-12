using MessagePack;

namespace WebTransportExample.Features;

[MessagePackObject(true)]
public class TransportEvent
{
    [Key("sessionId")]
    public required Guid SessionId;

    [Key("messageType")]
    public required MessageTypes MessageType;

    [Key("payload")]
    public byte[]? Payload;
}