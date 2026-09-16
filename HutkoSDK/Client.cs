using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HutkoSDK.Models;
using HutkoSDK.Utils;

namespace HutkoSDK
{
    public static class Client
    {
        private const string Agent = "Hutko-c-SDK";

        // A single shared HttpClient is reused for the lifetime of the process
        // (the recommended pattern; a new client per request exhausts sockets).
        private static readonly Lazy<HttpClient> HttpClientInstance =
            new Lazy<HttpClient>(CreateHttpClient, LazyThreadSafetyMode.ExecutionAndPublication);

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();
#if NET5_0_OR_GREATER
            // Enforce a modern TLS floor. Tls13 is only defined from .NET 5 onward.
            handler.SslProtocols =
                System.Security.Authentication.SslProtocols.Tls12 |
                System.Security.Authentication.SslProtocols.Tls13;
#elif NETSTANDARD2_1
            // netstandard2.1 exposes the setting but its enum has no Tls13 member.
            handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
#elif NETSTANDARD2_0
            // On the .NET Framework hosts that consume the netstandard2.0 build,
            // make sure TLS 1.2 is at least available (older defaults may omit it).
            try
            {
                System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
            }
            catch
            {
                // Platform already negotiates a secure protocol; nothing to do.
            }
#endif
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Agent);
            return client;
        }

        /// <summary>
        /// Basic Client (synchronous).
        /// </summary>
        /// <exception cref="ClientException"></exception>
        public static TChutkoResponse Invoke<TChutkoRequest, TChutkoResponse>(
            TChutkoRequest req,
            string actionUrl,
            bool isRoot = true,
            bool isCredit = false
        )
        {
            // Safe to block: InvokeAsync uses ConfigureAwait(false) throughout, so
            // there is no synchronization-context deadlock.
            return InvokeAsync<TChutkoRequest, TChutkoResponse>(req, actionUrl, isRoot, isCredit)
                .GetAwaiter().GetResult();
        }

        /// <summary>
        /// Basic Client (asynchronous).
        /// </summary>
        /// <exception cref="ClientException"></exception>
        public static async Task<TChutkoResponse> InvokeAsync<TChutkoRequest, TChutkoResponse>(
            TChutkoRequest req,
            string actionUrl,
            bool isRoot = true,
            bool isCredit = false,
            CancellationToken cancellationToken = default
        )
        {
            string data = BuildRequestBody(req, isCredit);
            var uri = new Uri(Config.Endpoint(null) + actionUrl);
            string contentType = GetContentTypeHeader(Config.ContentType);

            int statusCode;
            string responseBody;
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
                {
                    request.Content = new StringContent(data, Encoding.UTF8, contentType);
                    using (var httpResponse = await HttpClientInstance.Value
                               .SendAsync(request, cancellationToken).ConfigureAwait(false))
                    {
                        statusCode = (int) httpResponse.StatusCode;
#if NET5_0_OR_GREATER
                        responseBody = await httpResponse.Content
                            .ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
                        responseBody = await httpResponse.Content
                            .ReadAsStringAsync().ConfigureAwait(false);
#endif
                    }
                }
            }
            catch (HttpRequestException e)
            {
                throw new ClientException
                {
                    ErrorCode = "500",
                    ErrorMessage = e.Message,
                    RequestId = "Server is gone",
                };
            }
            catch (TaskCanceledException e)
            {
                throw new ClientException
                {
                    ErrorCode = "500",
                    ErrorMessage = e.Message,
                    RequestId = "Request timed out",
                };
            }

            return HandleResponse<TChutkoResponse>(statusCode, responseBody, isRoot);
        }

        /// <summary>
        /// Serializes the request according to the active protocol/content type.
        /// </summary>
        private static string BuildRequestBody<TChutkoRequest>(TChutkoRequest req, bool isCredit)
        {
            if (Config.Protocol == "2.0")
            {
                // In protocol v2 only json is allowed.
                if (Config.ContentType != "json")
                {
                    throw new ClientException
                    {
                        ErrorMessage = "In protocol v2 only json content allowed",
                        ErrorCode = "0"
                    };
                }

                return RequiredParams.GetParamsV2(req, isCredit);
            }

            return RequiredParams.ConvertRequestByContentType(req);
        }

        /// <summary>
        /// Applies the original status/error semantics to the raw response.
        /// </summary>
        private static TChutkoResponse HandleResponse<TChutkoResponse>(int statusCode, string response, bool isRoot)
        {
            if (statusCode != 200)
            {
                throw new ClientException
                {
                    ErrorCode = "500",
                    ErrorMessage = response,
                    RequestId = "Server is gone",
                };
            }

            ErrorResponseModel errorResponse =
                RequiredParams.ConvertResponseByContentType<ErrorResponseModel>(response, isRoot);
            if (errorResponse.response_status == "failure" || errorResponse.error_message != null)
            {
                throw new ClientException
                {
                    ErrorCode = errorResponse.error_code,
                    ErrorMessage = errorResponse.error_message,
                    RequestId = errorResponse.request_id,
                };
            }

            return RequiredParams.ConvertResponseByContentType<TChutkoResponse>(response, isRoot);
        }

        /// <summary>
        /// Content header by type
        /// </summary>
        private static string GetContentTypeHeader(string type = null)
        {
            switch (type)
            {
                case "xml":
                    return "application/xml";
                case "form":
                    return "application/x-www-form-urlencoded";
                default:
                    return "application/json";
            }
        }
    }
}
