using System;
using System.Threading;

namespace HutkoSDK
{
    public static class Config
    {
        /// <summary>
        /// Merchant identification
        /// </summary>
        public static int MerchantId { get; set; }

        /// <summary>
        /// Merchant Secret Key
        /// </summary>
        public static string SecretKey { get; set; }

        /// <summary>
        /// Merchant Credit Key
        /// </summary>
        public static string CreditKey { get; set; }

        // Per-call overrides. AsyncLocal keeps them scoped to the current logical
        // call context, so a value set for one request never leaks into another
        // request running concurrently on a different thread (fixes the previous
        // race where operations mutated the shared static state mid-flight).
        private static readonly AsyncLocal<string> ContentTypeOverride = new AsyncLocal<string>();
        private static readonly AsyncLocal<string> ProtocolOverride = new AsyncLocal<string>();

        private static string contentType = "json";

        /// <summary>
        /// Content type used for requests (json/xml/form). Defaults to json.
        /// </summary>
        public static string ContentType
        {
            get { return ContentTypeOverride.Value ?? contentType; }
            set { contentType = value; }
        }

        private static string protocol = "1.0.1";

        /// <summary>
        /// Protocol version. Defaults to "1.0.1" (adds external_ref to responses),
        /// which is the current default for new merchants. Use "2.0" only for
        /// calendar subscriptions.
        /// </summary>
        public static string Protocol
        {
            get { return ProtocolOverride.Value ?? protocol; }
            set { protocol = value; }
        }

        /// <summary>
        /// api host
        /// </summary>
        public static string ApiHost = "pay.hutko.org";

        /// <summary>
        /// Set api endpoint
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string Endpoint(string url)
        {
            string domain = @"https://{0}/api/";
            if (url == null)
            {
                url = ApiHost;
            }

            return string.Format(domain, url);
        }

        /// <summary>
        /// Applies protocol/content-type overrides for the duration of a single
        /// request without mutating the shared configuration. Dispose restores the
        /// previous values. Thread/async-safe.
        /// </summary>
        internal static IDisposable UseRequestScope(string protocolOverride = null, string contentTypeOverride = null)
        {
            return new RequestScope(protocolOverride, contentTypeOverride);
        }

        private sealed class RequestScope : IDisposable
        {
            private readonly string previousProtocol;
            private readonly string previousContentType;

            public RequestScope(string protocolOverride, string contentTypeOverride)
            {
                previousProtocol = ProtocolOverride.Value;
                previousContentType = ContentTypeOverride.Value;
                if (protocolOverride != null)
                {
                    ProtocolOverride.Value = protocolOverride;
                }

                if (contentTypeOverride != null)
                {
                    ContentTypeOverride.Value = contentTypeOverride;
                }
            }

            public void Dispose()
            {
                ProtocolOverride.Value = previousProtocol;
                ContentTypeOverride.Value = previousContentType;
            }
        }
    }
}
