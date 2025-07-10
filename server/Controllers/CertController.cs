namespace WebTransportExample.Controllers;

using Microsoft.AspNetCore.Mvc;
using WebTransportExample.Services.Cert;

[ApiController]
[Route("[controller]")]
public class CertController(ICertService certService) : ControllerBase
{
    [HttpGet("/certHash")]
    public string GetCertHash() => certService.GetCertHash();
}
