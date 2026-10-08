using System.Text.RegularExpressions;

namespace Dvelop.Sdk.ApplicationInformation
{
    public partial class SemVer
{
    public int Major { get; set; }
    public int Minor { get; set; }
    public int Patch { get; set; }
    public string Qualifier { get; set; } = string.Empty;


    public static SemVer FromString(string? versionString)
    {
        if (string.IsNullOrEmpty(versionString))
        {
            return new SemVer { Major = 0, Minor = 0, Patch = 0, Qualifier = string.Empty };
        }

        var version = MyRegex().Match(versionString);
        var semVer = new SemVer();

        if (int.TryParse(version.Groups["major"].Value, out var major))
        {
            semVer.Major = major;
        }

        if (int.TryParse(version.Groups["minor"].Value, out var minor))
        {
            semVer.Minor = minor;
        }

        if (int.TryParse(version.Groups["patch"].Value, out var patch))
        {
            semVer.Patch = patch;
        }

        semVer.Qualifier = version.Groups["qualifier"].Value;
        return semVer;
    }


    public string ToString(string? format = "v")
    {
        return format switch
        {
            "s" => $"{Major}.{Minor}.{Patch}",
            _ => $"{Major}.{Minor}.{Patch}-[{Qualifier}]"
        };
    }

    public static bool operator >(SemVer left, SemVer right)
    {
        return left.Major > right.Major ||
               left.Major == right.Major && left.Minor > right.Minor ||
               left.Major == right.Major && left.Minor == right.Minor && left.Patch > right.Patch;
    }

    public static bool operator <(SemVer left, SemVer right)
    {
        return right.Major > left.Major ||
               right.Major == left.Major && right.Minor > left.Minor ||
               right.Major == left.Major && right.Minor == left.Minor && right.Patch > left.Patch;
    }

    public static bool operator >=(SemVer left, SemVer right)
    {
        return left.Major > right.Major ||
               left.Major == right.Major && left.Minor > right.Minor ||
               left.Major == right.Major && left.Minor == right.Minor && left.Patch >= right.Patch;
    }

    public static bool operator <=(SemVer left, SemVer right)
    {
        return right.Major > left.Major ||
               right.Major == left.Major && right.Minor > left.Minor ||
               right.Major == left.Major && right.Minor == left.Minor && right.Patch >= left.Patch;
    }

        [GeneratedRegex(@"(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)[\s-\+]*(?<qualifier>[^+]*)")]
        private static partial Regex MyRegex();
    }
}