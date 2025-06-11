using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace WebTransportExample.Features.CertGenerator;

public class CertGenerator
{
    public static X509Certificate2 GenerateCert()
    {
        X509Certificate2? cert = null;
        var store = new X509Store("KestrelWebTransportCertificates", StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadWrite);

        if (store.Certificates.Count > 0)
        {
            cert = store.Certificates[^1];
            if (DateTime.Parse(cert.GetExpirationDateString(), null) < DateTimeOffset.UtcNow)
            {
                cert = null;
            }
        }

        if (cert == null)
        {
            var now = DateTimeOffset.UtcNow;
            SubjectAlternativeNameBuilder sanBuilder = new();
            sanBuilder.AddDnsName("localhost");
            using var ec = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            CertificateRequest req = new("CN=localhost", ec, HashAlgorithmName.SHA256);

            req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection{ new("1.3.6.1.5.5.7.3.1") }, false));
            req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
            req.CertificateExtensions.Add(sanBuilder.Build());

            using var crt = req.CreateSelfSigned(now, now.AddDays(14));
            cert = new(crt.Export(X509ContentType.Pfx));
            store.Add(cert);
        }
        store.Close();
        return cert;
    }
};
