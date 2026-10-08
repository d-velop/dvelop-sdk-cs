using System.Reflection;
using System.Text.RegularExpressions;

namespace Dvelop.Sdk.ApplicationInformation
{
    public class VersionProvider : IVersionProvider
    {
        private readonly Assembly _assembly;

        /// <summary>
        /// Reads the version from the given assembly (usually the assembly of the application, e.g. <c>typeof(Program).Assembly</c>).
        /// </summary>
        public VersionProvider(Assembly assembly)
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }

        public SemVer Version
        {
            get
            {
                var assemblyInfoVersion = _assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                var version = Regex.Match(assemblyInfoVersion?.InformationalVersion ?? "", @"(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)[\s-]*(?<qualifier>.*)", RegexOptions.None, TimeSpan.FromMinutes(1));
                var qualifier = version.Groups["qualifier"].Value;
                return new SemVer
                {
                    Major = int.Parse(version.Groups["major"].Value),
                    Minor = int.Parse(version.Groups["minor"].Value),
                    Patch = int.Parse(version.Groups["patch"].Value),
                    Qualifier = qualifier
                };

            }
        }
    }
}
