using System.Collections.Concurrent;
using Microsoft.AspNetCore.Connections;

namespace WebTransportExample.Services.TransportSessions;

public class TransportSessions : ITransportSessions
{
    private readonly ConcurrentDictionary<Guid, ConnectionContext> _connections = new();

    public ConnectionContext AddSession(Guid sessionId, ConnectionContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (_connections.TryGetValue(sessionId, out _))
        {
            throw new InvalidOperationException("This session already exists");
        }
        var isAddedSession = _connections.TryAdd(sessionId, ctx);
        if (isAddedSession) 
        {
            Console.WriteLine($"Added WebTransport session by GUID: {sessionId}");
        }
        var session = _connections.GetValueOrDefault(sessionId)
            ?? throw new InvalidOperationException("This session was not found.");

        return session;
    }

    public void RemoveSession(Guid sessionId) 
    {
        if (!_connections.TryGetValue(sessionId, out _))
        {
            throw new InvalidOperationException("This session was not found.");
        }
        var isRemovedSession = _connections.Remove(sessionId, out _);
        if (isRemovedSession) 
        {
            Console.WriteLine($"Removed WebTransport session by GUID: {sessionId}");
        }
    }
}