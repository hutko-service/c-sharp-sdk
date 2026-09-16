using System;
using System.Xml.Serialization;
using HutkoSDK.Models;
using HutkoSDK.Utils;
using Newtonsoft.Json;

namespace HutkoSDK.Checkout
{
    /// <summary>
    /// Subscription Api
    /// </summary>
    public class Subscription
    {
        public SubscriptionResponse Post(SubscriptionRequest req)
        {
            if (req == null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            // Subscription is a protocol 2.0 / json-only operation. Scope the overrides
            // to this call so concurrent requests are unaffected.
            using (Config.UseRequestScope(protocolOverride: "2.0", contentTypeOverride: "json"))
            {
                SubscriptionResponse response;
                req.merchant_id = Config.MerchantId;
                req.version = Config.Protocol;
                req.subscription = "Y";
                req.signature = Signature.GetRequestSignature(RequiredParams.GetHashProperties(req));
                try
                {
                    response = Client.Invoke<SubscriptionRequest, SubscriptionResponse>(req, req.ActionUrl);
                }
                catch (ClientException c)
                {
                    return new SubscriptionResponse {Error = c};
                }

                if (response.data != null)
                {
                    return JsonFormatter.ConvertFromJson<SubscriptionResponse>(response.data, true, "order");
                }

                return response;
            }
        }
    }

    [XmlRoot("request")]
    [JsonObject(Title = "request")]
    public class SubscriptionRequest : CheckoutRequestModel
    {
        [JsonProperty(PropertyName = "recurring_data")]
        public ReccuringData recurring_data { get; set; }

        [JsonIgnore] [XmlIgnore] public readonly string ActionUrl = @"checkout/url/";
    }

    [XmlRoot("response")]
    [JsonObject(Title = "response")]
    public class SubscriptionResponse : CheckoutResponseModel
    {
        [JsonProperty(PropertyName = "payment_id")]
        public int payment_id { get; set; }

        [JsonProperty(PropertyName = "checkout_url")]
        public string checkout_url { get; set; }
    }
}