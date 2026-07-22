using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Css.Scanner.Software;

public static class SignatureInspector
{
    public static string? GetSignatureSubject(string executablePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
                return null;

            using var cert = X509Certificate.CreateFromSignedFile(executablePath);
            return cert.Subject;
        }
        catch
        {
            return null;
        }
    }
}
