using System;
using Microsoft.Extensions.Options;

namespace Dvelop.Sdk.TenantMiddleware
{
    /// <summary>
    /// <see cref="IOptionsMonitor{TOptions}"/> for a fixed options instance, used by the legacy constructors.
    /// </summary>
    internal sealed class StaticOptionsMonitor<TOptions> : IOptionsMonitor<TOptions>
    {
        public StaticOptionsMonitor(TOptions value)
        {
            CurrentValue = value;
        }

        public TOptions CurrentValue { get; }

        public TOptions Get(string name) => CurrentValue;

        // The value never changes, so listeners are never called.
        public IDisposable OnChange(Action<TOptions, string> listener) => null;
    }
}
