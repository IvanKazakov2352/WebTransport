using Microsoft.AspNetCore.Mvc;
using WebTransportExample.Features.CertGenerator;
using WebTransportExample.Features.CertResponse;
using System.Security.Cryptography;

namespace WebTransportExample.Controllers.CertManager;

[ApiController]
[Route("api/v1")]
public class CertController : ControllerBase
{
    [HttpGet("cert")]
    public CertResponse GetCert()
    {
        var cert = CertGenerator.GenerateCert();
        var hash = SHA256.HashData(cert.RawData);
        var certStr = Convert.ToBase64String(hash);
        return new CertResponse(certStr);
    }
};
