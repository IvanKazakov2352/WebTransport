using System.Collections.Concurrent;
using Microsoft.AspNetCore.Connections;

namespace WebTransportExample.Services.TransportSessions;

public class TransportSessions : ITransportSessions
{
    private readonly ConcurrentDictionary<Guid, ConnectionContext> _connections = new();

    public void AddSession(Guid sessionId, ConnectionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void RemoveSession(Guid sessionId) 
    {
        throw new NotImplementedException();
    }
}