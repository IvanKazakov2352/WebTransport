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
            Console.WriteLine("Failed getting asp feature {featureName}", nameof(IHttpWebTransportFeature));
            throw new InvalidOperationException(nameof(IHttpWebTransportFeature));
        }

        if (!wt.IsWebTransportRequest)
        {
            Console.WriteLine("Request is not web transport type");
            return;
        }
        var session = await wt.AcceptAsync(ctx.RequestAborted);

        Console.WriteLine("WebTransport session accepted");

        var stream = await session.AcceptStreamAsync(ctx.RequestAborted);
        if (stream is null) 
        {
            Console.WriteLine("Unable to create a stream");
            return;
        }
        var transportSessions = ctx.RequestServices.GetRequiredService<ITransportSessions>();

        if (transportSessions is null)
        {
            Console.WriteLine("Failed getting asp service {featureName}", nameof(ITransportSessions));
            throw new InvalidOperationException(nameof(ITransportSessions));
        }
        var sessionId = Guid.NewGuid();
        transportSessions.AddSession(sessionId, stream);

        var sessionContext = transportSessions.GetSession(sessionId) 
            ?? throw new InvalidOperationException("This session was not found.");

        var output = sessionContext.Transport.Output;
        var input = sessionContext.Transport.Input;

        Console.WriteLine("WebTransport streaming started");

        await Task.WhenAll(
            HandleReadableStream(ctx, input, sessionContext),
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
        var direction = stream.Features.GetRequiredFeature<IStreamDirectionFeature>();
        Console.WriteLine("Writable stream started");

        if (direction is null)
        {
            Console.WriteLine("Failed getting asp feature {featureName}", nameof(IStreamDirectionFeature));
            return;
        }
        if (!direction.CanWrite)
        {
            Console.WriteLine("Not writing option for this stream");
            return;
        }

        var tEvent = new TransportEvent<string> {
            SessionId = sessionId,
            Payload = "SessionId"
        };

        var message = MessagePackSerializer.Serialize(tEvent, null, ctx.RequestAborted);
        await pipe.WriteAsync(message, ctx.RequestAborted);
        await pipe.FlushAsync(ctx.RequestAborted);
    }

    private static async Task HandleReadableStream(HttpContext ctx, PipeReader pipe, ConnectionContext stream)
    {
        var direction = stream.Features.GetRequiredFeature<IStreamDirectionFeature>();
        Console.WriteLine("Readable stream started");

        if (direction is null)
        {
            Console.WriteLine("Failed getting asp feature {featureName}", nameof(IStreamDirectionFeature));
            return;
        }
        if (!direction.CanRead)
        {
            Console.WriteLine("Not reading option for this stream");
            return;
        }

        while (!ctx.RequestAborted.IsCancellationRequested) 
        {
            try
            {
                var result = await pipe.ReadAsync(ctx.RequestAborted);
                if (result.IsCompleted || result.Buffer.Length == 0)
                {
                    pipe.AdvanceTo(result.Buffer.End);
                    ctx.Abort();
                    break;
                }
                var message = MessagePackSerializer.Deserialize<string>(result.Buffer, null, ctx.RequestAborted);
                Console.WriteLine(message);
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
            catch(ConnectionResetException)
            {
                Console.WriteLine("Connection inactive exception");
                ctx.Abort();
            }
        }
    }
}