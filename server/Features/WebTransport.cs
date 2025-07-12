using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;
using MessagePack;
using System.IO.Pipelines;
using WebTransportExample.Services.TransportSessions;

namespace WebTransportExample.Features.WebTransport;

public static class WebTransport
{
    public static void MapWebTransport(this WebApplication app)
    {
        app.Map("/wt", HandleWebTransport);
    }

    private async static Task HandleWebTransport(HttpContext ctx)
    {
        var wt = ctx.Features.GetRequiredFeature<IHttpWebTransportFeature>();

        if (wt is null)
        {
            throw new InvalidOperationException($"Failed getting asp feature {nameof(IHttpWebTransportFeature)}");
        }

        if (!wt.IsWebTransportRequest)
        {
            throw new InvalidOperationException("Request is not WebTransport type");
        }
        var session = await wt.AcceptAsync(ctx.RequestAborted);

        Console.WriteLine("WebTransport session accepted");

        var stream = await session.AcceptStreamAsync(ctx.RequestAborted);
        if (stream is null) 
        {
            throw new InvalidOperationException("Unable to create a stream");
        }
        var transportSessions = ctx.RequestServices.GetRequiredService<ITransportSessions>();

        if (transportSessions is null)
        {
            throw new InvalidOperationException($"Failed getting asp service {nameof(ITransportSessions)}");
        }
        var sessionId = Guid.NewGuid();
        var sessionContext = transportSessions.AddSession(sessionId, stream);

        var output = sessionContext.Transport.Output;
        var input = sessionContext.Transport.Input;

        Console.WriteLine("WebTransport streaming started");

        await Task.WhenAll(
            HandleReadableStream(ctx, input, output, sessionContext, sessionId),
            HandleWritableStream(ctx, output, sessionContext, sessionId)
        );

        await output.CompleteAsync();
        Console.WriteLine("Writable stream completed");

        await input.CompleteAsync();
        Console.WriteLine("Readable stream completed");

        await sessionContext.DisposeAsync();
        transportSessions.RemoveSession(sessionId);
        Console.WriteLine("WebTransport streaming ended");

        session.Abort(101);
        Console.WriteLine("WebTransport session aborted");
    }

    private static async Task HandleWritableStream(HttpContext ctx, PipeWriter pipe, ConnectionContext stream, Guid sessionId)
    {
        try
        {
            var direction = stream.Features.GetRequiredFeature<IStreamDirectionFeature>();
            Console.WriteLine("Writable stream started");

            if (direction is null)
            {
                throw new InvalidOperationException($"Failed getting asp feature {nameof(IStreamDirectionFeature)}");
            }
            if (!direction.CanWrite)
            {
                throw new InvalidOperationException("Not writing option for this stream");
            }

            var initialMessage = new TransportEvent
            {
                SessionId = sessionId,
                MessageType = MessageTypes.INITIAL_MESSAGE,
            };

            var message = MessagePackSerializer.Serialize(initialMessage, null, ctx.RequestAborted);
            await pipe.WriteAsync(message, ctx.RequestAborted);
            await pipe.FlushAsync(ctx.RequestAborted);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Operation is canceled. SessionID: {sessionId}");
            ctx.Abort();
        }
        catch (InvalidOperationException ex) 
        {
            Console.WriteLine(ex.Message);
            ctx.Abort();
        }
    }

    private static async Task HandleReadableStream(HttpContext ctx, PipeReader reader, PipeWriter writer, ConnectionContext stream, Guid sessionId)
    {
        try
        {
            var direction = stream.Features.GetRequiredFeature<IStreamDirectionFeature>();
            Console.WriteLine("Readable stream started");

            if (direction is null)
            {
                throw new InvalidOperationException($"Failed getting asp feature {nameof(IStreamDirectionFeature)}");
            }
            if (!direction.CanRead)
            {
                throw new InvalidOperationException("Not writing option for this stream");
            }

            while (!ctx.RequestAborted.IsCancellationRequested)
            {
                try
                {
                    var result = await reader.ReadAsync(ctx.RequestAborted);
                    if (result.IsCompleted || result.Buffer.Length == 0)
                    {
                        reader.AdvanceTo(result.Buffer.End);
                        ctx.Abort();
                        break;
                    }
                    var message = MessagePackSerializer.Deserialize<TransportEvent>(
                        result.Buffer, 
                        null, 
                        ctx.RequestAborted
                    );

                    Console.WriteLine($"SessionId: {message.SessionId}, MessageType: {message.MessageType}");

                    if(message.MessageType == MessageTypes.PING_PONG)
                    {
                        var messageData = new TransportEvent
                        {
                            SessionId = sessionId,
                            MessageType = MessageTypes.PING_PONG,
                            Payload = MessagePackSerializer.Serialize("PONG", null, ctx.RequestAborted)
                        };
                        var pongMessage = MessagePackSerializer.Serialize(messageData, null, ctx.RequestAborted);
                        await writer.WriteAsync(pongMessage, ctx.RequestAborted);
                        await writer.FlushAsync(ctx.RequestAborted);
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Error reading message");
                    ctx.Abort();
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("Invalid operation exception");
                    ctx.Abort();
                }
                catch (ConnectionResetException)
                {
                    Console.WriteLine("Connection reset exception");
                    ctx.Abort();
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Operation is canceled. SessionID: {sessionId}");
            ctx.Abort();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
            ctx.Abort();
        }
    }
}