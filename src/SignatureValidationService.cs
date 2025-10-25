// File: SignatureValidationService.cs
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
                var cert = X509Certificate.CreateFromSignedFile(filePath);
                publisherName = cert.Subject;
                return !string.IsNullOrEmpty(publisherName);
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
            if (!GetPublisherInfo(filePath, out publisherName))
            {
                return false;
            }

            try
            {
                var cert = X509Certificate.CreateFromSignedFile(filePath);
                var chain = new X509Chain();
                return chain.Build(new X509Certificate2(cert));
            }
            catch (Exception ex) when (ex is CryptographicException or IOException or UnauthorizedAccessException)
            {
                Debug.WriteLine($"[ERROR] Signature chain validation failed for {filePath}: {ex.Message}");
                return false;
            }
        }
    }
}