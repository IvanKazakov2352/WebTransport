using Microsoft.AspNetCore.Connections;

namespace WebTransportExample.Services.TransportSessions;

public interface ITransportSessions
{
    ConnectionContext AddSession(Guid sessionId, ConnectionContext ctx);
    void RemoveSession(Guid sessionId);
};