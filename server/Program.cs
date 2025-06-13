using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;
using WebTransportExample.Features.WebTransport;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Listen(IPAddress.Any, 9000, listenOptions =>
    {
        if (File.Exists("../caddy/certificate.pfx"))
        {
            listenOptions.UseHttps("../caddy/certificate.pfx", "localhost");
            listenOptions.DisableAltSvcHeader = false;
            listenOptions.Protocols = HttpProtocols.Http3;
        } 
        else
        {
          listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
        }
        listenOptions.UseConnectionLogging();
    });
});

builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
}));

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("AllowAll");
app.UseAuthorization();
app.UseAuthentication();
app.MapControllers();
app.MapWebTransport();

app.Run();
