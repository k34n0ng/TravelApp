using System.Diagnostics;
using System.Security.Cryptography;
using Android.Content.PM;

namespace TravelApp.Services;

/// <summary>
/// Android part: reads the key from the existing com.google.android.geo.API_KEY
/// manifest meta-data (populated from GoogleMaps.Local.props) and supplies the
/// X-Android-Package / X-Android-Cert headers that Android-restricted keys
/// require for Google web-service calls.
/// </summary>
public sealed partial class GoogleApiCredentialProvider
{
    private const string ApiKeyMetaDataName = "com.google.android.geo.API_KEY";

    partial void ReadPlatformApiKey(ref string? apiKey)
    {
        try
        {
            var context = global::Android.App.Application.Context;
            var applicationInfo = context.PackageManager?.GetApplicationInfo(
                context.PackageName!,
                PackageInfoFlags.MetaData);

            apiKey = applicationInfo?.MetaData?.GetString(ApiKeyMetaDataName);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GoogleApiCredentialProvider: failed to read API key meta-data ({ex.GetType().Name}).");
        }
    }

    partial void AddPlatformRequestHeaders(IDictionary<string, string> headers)
    {
        try
        {
            var context = global::Android.App.Application.Context;
            var packageName = context.PackageName;
            if (string.IsNullOrEmpty(packageName))
            {
                return;
            }

            headers["X-Android-Package"] = packageName;

            var certificateSha1 = GetSigningCertificateSha1(context.PackageManager, packageName);
            if (certificateSha1 != null)
            {
                headers["X-Android-Cert"] = certificateSha1;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GoogleApiCredentialProvider: failed to read app identity ({ex.GetType().Name}).");
        }
    }

    private static string? GetSigningCertificateSha1(PackageManager? packageManager, string packageName)
    {
        if (packageManager == null)
        {
            return null;
        }

        Signature? signature;
        if (OperatingSystem.IsAndroidVersionAtLeast(28))
        {
            var signingInfo = packageManager
                .GetPackageInfo(packageName, PackageInfoFlags.SigningCertificates)?
                .SigningInfo;

            // History is ordered oldest to current; the current signer is last.
            signature = signingInfo == null
                ? null
                : signingInfo.HasMultipleSigners
                    ? signingInfo.GetApkContentsSigners()?.FirstOrDefault()
                    : signingInfo.GetSigningCertificateHistory()?.LastOrDefault();
        }
        else
        {
#pragma warning disable CS0618 // Signatures is the only option below API 28.
            signature = packageManager
                .GetPackageInfo(packageName, PackageInfoFlags.Signatures)?
                .Signatures?
                .FirstOrDefault();
#pragma warning restore CS0618
        }

        var certificate = signature?.ToByteArray();
        if (certificate == null || certificate.Length == 0)
        {
            return null;
        }

        return Convert.ToHexString(SHA1.HashData(certificate));
    }
}
