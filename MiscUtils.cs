using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace EasyFramework;

public static class MiscUtils
{
    /**
     * Based on: https://stackoverflow.com/a/43232486/2076423
     */
    public static void OpenUrl(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }

    public static string HashPassword(string password)
    {
        var enc = Encoding.UTF8;
        var hash = SHA256.HashData(enc.GetBytes(password));
        return Convert.ToBase64String(hash);
    }
}