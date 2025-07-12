using Microsoft.AspNetCore.Mvc;
using WebTransportExample.Services.Cert;

namespace WebTransportExample.Controllers;

[Route("[controller]"), ApiController]
public class CertController(ICertService certService) : ControllerBase
{
    [HttpGet("/certHash")]
    public string GetCertHash() => certService.GetCertHash();
}
