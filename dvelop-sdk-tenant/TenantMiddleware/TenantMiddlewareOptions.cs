using System;

namespace Dvelop.Sdk.TenantMiddleware
{

    public enum TenantMiddlewareLogLevel
    {
        Debug=1,
        Info=2,
        Error=3
    }

    public class TenantMiddlewareOptions
    {
        /// <summary>
        /// Wird aufgerufen wenn eine valide TenantId und oder SystemBaseUri aus den HttpHeadern und Defaultwerten ermittelt werden konnte.
        /// Der 1. Parameter ist die tenantId, der 2. Parameter die SystemBaseUri
        /// </summary>
        public Action<string,string> OnTenantIdentified { get; set; }

        /// <summary>
        /// Diese SystemBaseUri wird verwendet, wenn im request keine Information über die zu verwendende SystemBaseUri enthalten sind (Header nicht vorhanden)
        /// </summary>
        public string DefaultSystemBaseUri { get; set; }

        /// <summary>
        /// Diese TenantId wird verwendet, wenn im request keine Information über die zu verwendende TenantId enthalten sind (Header nicht vorhanden)
        /// </summary>
        public string DefaultTenantId { get; set; }

        public byte[] SignatureSecretKey { get; set; }
        
        /// <summary>
        /// Ist diese Option gesetzt, so wird die TenantId aus dem HttpHeader verwendet, auch wenn die Signatur falsch ist (nur zu Testzwecken)
        /// </summary>
        public bool IgnoreSignature { get; set; }

        /// <summary>
        /// Wird dieser Callback angegeben, so werden Logereignisse über diesen Callback ausgegeben
        /// </summary>
        public Action<TenantMiddlewareLogLevel, string> LogCallback { get; set; }

    }
}