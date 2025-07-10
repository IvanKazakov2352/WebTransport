using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace WebTransportExample.Services.Cert;

public class CertService : ICertService
{
    public string GetCertHash()
    {
        var cert = X509CertificateLoader.LoadPkcs12FromFile("../ssl/cert.pfx", "localhost");
        var hash = SHA256.HashData(cert.RawData);
        var certStr = Convert.ToBase64String(hash);
        return certStr;
    }
}