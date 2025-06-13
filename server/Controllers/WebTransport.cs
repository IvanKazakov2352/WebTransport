using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System.Net;

#pragma warning disable CA2252

namespace WebTransportExample.Controllers.Wt;

[ApiController]
public class WtController : ControllerBase
{
    [HttpGet("wt")]
    public async Task ConnectWebTransport(CancellationToken ct)
    {
        var ctx = HttpContext;
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
        var session = await wt.AcceptAsync(ct);

        logger.LogInformation("Use WebTransport via the newly established session");

        session.Abort(201);
    }
}
