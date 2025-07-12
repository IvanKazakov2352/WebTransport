using System.Collections.Concurrent;
using Microsoft.AspNetCore.Connections;

namespace WebTransportExample.Services.TransportSessions;

public class TransportSessions : ITransportSessions
{
    private readonly ConcurrentDictionary<Guid, ConnectionContext> _connections = new();

    public void AddSession(Guid sessionId, ConnectionContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (_connections.TryGetValue(sessionId, out _))
        {
            throw new InvalidOperationException("This session already exists");
        }
        _connections.TryAdd(sessionId, ctx);
        Console.WriteLine($"Added WebTransport session by GUID: {sessionId}");
    }

    public void RemoveSession(Guid sessionId) 
    {
        if (!_connections.TryGetValue(sessionId, out _))
        {
            throw new InvalidOperationException("This session was not found.");
        }
        _connections.Remove(sessionId, out _);
        Console.WriteLine($"Removed WebTransport session by GUID: {sessionId}");
    }

    public ConnectionContext GetSession(Guid sessionId) 
    {
        if (!_connections.TryGetValue(sessionId, out _))
        {
            throw new InvalidOperationException("This session was not found.");
        }
        var ctx = _connections.GetValueOrDefault(sessionId) 
            ?? throw new InvalidOperationException("This session was not found.");

        Console.WriteLine($"Received WebTransport session by GUID: {sessionId}");
        return ctx;
    }
}