// SignatureValidationService.cs
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace MinimalFirewall
{
    public static class SignatureValidationService
    {

        /// Checks if a file has a digital signature and extracts the publisher name.
        /// This is a lenient check and does not validate the trust chain.
 
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
                // File is not signed
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Signature extraction failed for {filePath}: {ex.Message}");
                return false;
            }
        }


        /// Checks if a file has a digital signature that is valid and trusted by the local machine.
        /// This is a strict check.

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
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Signature chain validation failed for {filePath}: {ex.Message}");
                return false;
            }
        }
    }
}