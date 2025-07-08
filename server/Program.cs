using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using WebTransportExample.Features.WebTransport;
using System.Security.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.ListenAnyIP(5001, (listenOptions) =>
    {
        Action<HttpsConnectionAdapterOptions> httpsConfig = options =>
            options.SslProtocols = SslProtocols.Tls13;

        listenOptions.UseHttps("cert.pfx", "password", httpsConfig);
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;

        Console.WriteLine("HTTP/3 enabled");
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
