using Microsoft.AspNetCore.Http.Features;
using System.Net;

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
            ctx.Response.StatusCode = (int)HttpStatusCode.UpgradeRequired;
            return;
        }
        var session = await wt.AcceptAsync(ctx.RequestAborted);

        logger.LogInformation("Web Transport session accepted");
        var conn = await session.AcceptStreamAsync(ctx.RequestAborted);

        if (conn is null)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.NoContent;
            return;
        }

        logger.LogInformation("Web Transport stream created");

        var outputPipe = conn.Transport.Output;
        var inputPipe = conn.Transport.Input;
        await outputPipe.WriteAsync(new Memory<byte>(new byte[] { 65, 66, 67, 68, 69 }), ctx.RequestAborted);
    }
}