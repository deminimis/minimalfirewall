using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace MinimalFirewall
{
    public static class SignatureValidationService
    {
        public static bool GetPublisherInfo(string filePath, out string? publisherName)
        {
            publisherName = null;
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            try
            {
                using (var cert = new X509Certificate2(filePath))
                {
                    publisherName = cert.Subject;
                    return !string.IsNullOrEmpty(publisherName);
                }
            }
            catch (CryptographicException)
            {
                return false;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Debug.WriteLine($"[ERROR] Signature extraction failed for {filePath}: {ex.Message}");
                return false;
            }
        }

        public static bool IsSignatureTrusted(string filePath, out string? publisherName)
        {
            publisherName = null;

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            try
            {
                using (var cert = new X509Certificate2(filePath))
                using (var chain = new X509Chain())
                {
                    publisherName = cert.Subject;
                    if (string.IsNullOrEmpty(publisherName)) return false;

                    chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck; 
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;

                    return chain.Build(cert);
                }
            }
            catch (Exception ex) when (ex is CryptographicException or IOException or UnauthorizedAccessException)
            {
                Debug.WriteLine($"[ERROR] Signature chain validation failed for {filePath}: {ex.Message}");
                return false;
            }
        }
    }
}