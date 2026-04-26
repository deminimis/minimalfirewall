using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace MinimalFirewall
{
    public static class SignatureValidationService
    {
        private static readonly Guid WINTRUST_ACTION_GENERIC_VERIFY_V2 = new("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");

        [DllImport("wintrust.dll", ExactSpelling = true, SetLastError = false, CharSet = CharSet.Unicode)]
        private static extern int WinVerifyTrust(IntPtr hwnd, IntPtr pgActionID, IntPtr pWinTrustData);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WINTRUST_DATA
        {
            public uint cbStruct;
            public IntPtr pPolicyCallbackData;
            public IntPtr pSIPClientData;
            public uint dwUIChoice;
            public uint fdwRevocationChecks;
            public uint dwUnionChoice;
            public IntPtr pUnion;       // Union of pFile/pCatalog/pBlob — one IntPtr
            public uint dwStateAction;
            public IntPtr hWVTStateData;
            public IntPtr pwszURLReference;
            public uint dwProvFlags;
            public uint dwUIContext;
            public IntPtr pSignatureSettings;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WINTRUST_FILE_INFO
        {
            public uint cbStruct;
            public IntPtr pcwszFilePath;
            public IntPtr hFile;
            public IntPtr pgKnownSubject;
        }

        private const int WTD_UI_NONE = 2;
        private const int WTD_REVOKE_NONE = 0;
        private const int WTD_CHOICE_FILE = 1;
        private const int WTD_STATEACTION_IGNORE = 0;

        public static bool GetPublisherInfo(string filePath, out string? publisherName)
            => IsSignatureTrusted(filePath, out publisherName);

        public static bool IsSignatureTrusted(string filePath, out string? publisherName)
        {
            publisherName = null;

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            // Gate on Authenticode digest verification. WinVerifyTrust validates that the
            // file's hash matches its embedded or catalog signature AND that the signer
            // chains to a trusted root. Without this gate, a tampered file would still
            // expose its original signer's CN via CreateFromSignedFile.
            if (!VerifyWithWinTrust(filePath))
            {
                return false;
            }

            // File integrity verified. Extract publisher name for display/whitelist matching.
            try
            {
#pragma warning disable SYSLIB0057 // CreateFromSignedFile has no direct replacement for Authenticode signer extraction
                using var basicCert = X509Certificate.CreateFromSignedFile(filePath);
                using var cert = new X509Certificate2(basicCert);
#pragma warning restore SYSLIB0057
                publisherName = NormalizePublisherName(cert.Subject);
            }
            catch (CryptographicException)
            {
                // Catalog-signed: no embedded cert. Fall back to PE metadata.
                publisherName = GetPublisherNameFromFileVersion(filePath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Debug.WriteLine($"[INFO] Signer extraction failed for {filePath}: {ex.Message}");
                return false;
            }

            return !string.IsNullOrEmpty(publisherName);
        }

        private static string? NormalizePublisherName(string? subject)
        {
            if (string.IsNullOrEmpty(subject)) return null;

            const string cnPrefix = "CN=";
            int cnStart = subject.IndexOf(cnPrefix, StringComparison.Ordinal);
            if (cnStart < 0) return subject;

            cnStart += cnPrefix.Length;
            int cnEnd = subject.IndexOf(',', cnStart);
            string cn = cnEnd < 0
                ? subject.Substring(cnStart).Trim()
                : subject.Substring(cnStart, cnEnd - cnStart).Trim();

            return string.IsNullOrEmpty(cn) ? subject : cn;
        }

        private static bool VerifyWithWinTrust(string filePath)
        {
            IntPtr pFilePath = IntPtr.Zero;
            IntPtr pFileInfo = IntPtr.Zero;
            IntPtr pTrustData = IntPtr.Zero;
            IntPtr pActionId = IntPtr.Zero;

            try
            {
                pFilePath = Marshal.StringToHGlobalUni(filePath);

                var fileInfo = new WINTRUST_FILE_INFO
                {
                    cbStruct = (uint)Marshal.SizeOf<WINTRUST_FILE_INFO>(),
                    pcwszFilePath = pFilePath,
                    hFile = IntPtr.Zero,
                    pgKnownSubject = IntPtr.Zero
                };

                pFileInfo = Marshal.AllocHGlobal(Marshal.SizeOf<WINTRUST_FILE_INFO>());
                Marshal.StructureToPtr(fileInfo, pFileInfo, false);

                var trustData = new WINTRUST_DATA
                {
                    cbStruct = (uint)Marshal.SizeOf<WINTRUST_DATA>(),
                    pPolicyCallbackData = IntPtr.Zero,
                    pSIPClientData = IntPtr.Zero,
                    dwUIChoice = WTD_UI_NONE,
                    fdwRevocationChecks = WTD_REVOKE_NONE,
                    dwUnionChoice = WTD_CHOICE_FILE,
                    pUnion = pFileInfo,
                    dwStateAction = WTD_STATEACTION_IGNORE,
                    hWVTStateData = IntPtr.Zero,
                    pwszURLReference = IntPtr.Zero,
                    dwProvFlags = 0,
                    dwUIContext = 0,
                    pSignatureSettings = IntPtr.Zero
                };

                pTrustData = Marshal.AllocHGlobal(Marshal.SizeOf<WINTRUST_DATA>());
                Marshal.StructureToPtr(trustData, pTrustData, false);

                pActionId = Marshal.AllocHGlobal(Marshal.SizeOf<Guid>());
                Marshal.StructureToPtr(WINTRUST_ACTION_GENERIC_VERIFY_V2, pActionId, false);

                int result = WinVerifyTrust(IntPtr.Zero, pActionId, pTrustData);

                // No state retained with WTD_STATEACTION_IGNORE, so no close pass needed.
                return result == 0; // ERROR_SUCCESS
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] WinVerifyTrust failed for {filePath}: {ex.Message}");
                return false;
            }
            finally
            {
                if (pFilePath != IntPtr.Zero) Marshal.FreeHGlobal(pFilePath);
                if (pFileInfo != IntPtr.Zero) Marshal.FreeHGlobal(pFileInfo);
                if (pTrustData != IntPtr.Zero) Marshal.FreeHGlobal(pTrustData);
                if (pActionId != IntPtr.Zero) Marshal.FreeHGlobal(pActionId);
            }
        }

        private static string? GetPublisherNameFromFileVersion(string filePath)
        {
            try
            {
                var info = FileVersionInfo.GetVersionInfo(filePath);
                return string.IsNullOrEmpty(info.CompanyName) ? info.OriginalFilename : info.CompanyName;
            }
            catch
            {
                return null;
            }
        }
    }
}