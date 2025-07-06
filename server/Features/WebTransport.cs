using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;
using MessagePack;

namespace WebTransportExample.Features.WebTransport;

public static class WebTransport
{
    public static void MapWebTransport(this WebApplication app)
    {
        app.Map("/wt", HandleWebTransport);
    }

    private async static Task HandleWebTransport(HttpContext ctx)
    {
        var logger = ctx.RequestServices.GetRequiredService<ILogger<IHttpWebTransportFeature>>();
        var wt = ctx.Features.GetRequiredFeature<IHttpWebTransportFeature>();

        if (wt is null)
        {
            logger.LogCritical("Failed getting asp feature {featureName}", nameof(IHttpWebTransportFeature));
            throw new InvalidOperationException(nameof(IHttpWebTransportFeature));
        }

        if (!wt.IsWebTransportRequest)
        {
            logger.LogCritical("Request is not web transport type");
            return;
        }
        var session = await wt.AcceptAsync(ctx.RequestAborted);

        logger.LogInformation("Web Transport session accepted");

        ConnectionContext? stream = null;
        IStreamDirectionFeature? direction = null;
        while (true)
        {
            stream = await session.AcceptStreamAsync(ctx.RequestAborted);
            if (stream is null)
                break;

            direction = stream.Features.GetRequiredFeature<IStreamDirectionFeature>();
            if (direction.CanRead && direction.CanWrite)
                break;

            else
                await stream.DisposeAsync();
        }

        var inputPipe = stream!.Transport.Input;
        var outputPipe = stream!.Transport.Output;

        while (true)
        {
            var result = await inputPipe.ReadAsync(ctx.RequestAborted);
            if (result.IsCompleted || result.Buffer.Length == 0)
                break;
            var message = MessagePackSerializer.Deserialize<string>(result.Buffer);
            Console.WriteLine(message);
            var newMessage = MessagePackSerializer.Serialize(message.GetType(), "PONG");
            await outputPipe.WriteAsync(newMessage, ctx.RequestAborted);
            await Task.Delay(300);
        }

        await inputPipe.CompleteAsync();

        await outputPipe.FlushAsync(ctx.RequestAborted);
    }
}