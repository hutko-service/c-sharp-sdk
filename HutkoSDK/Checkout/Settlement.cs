using System;
using HutkoSDK.Utils;
using Newtonsoft.Json;

namespace HutkoSDK.Checkout
{
    /// <summary>
    /// Settlement url Api
    /// </summary>
    public class Settlement
    {
        public SettlementResponse Post(SettlementRequest req)
        {
            if (req == null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            // Settlement is a protocol 2.0 / json-only operation. Scope the overrides
            // to this call so concurrent requests are unaffected.
            using (Config.UseRequestScope(protocolOverride: "2.0", contentTypeOverride: "json"))
            {
                SettlementResponse response;
                req.merchant_id = Config.MerchantId;
                req.order_type = "settlement";
                try
                {
                    response = Client.Invoke<SettlementRequest, SettlementResponse>(req, req.ActionUrl);
                }
                catch (ClientException c)
                {
                    return new SettlementResponse {Error = c};
                }

                if (response.data != null)
                {
                    return JsonFormatter.ConvertFromJson<SettlementResponse>(response.data, true, "order");
                }

                return response;
            }
        }
    }

    [JsonObject(Title = "request")]
    public class SettlementRequest : Models.CheckoutRequestModel
    {
        [JsonIgnore] public readonly string ActionUrl = @"settlement/";
    }

    [JsonObject(Title = "response")]
    public class SettlementResponse : Models.ResponseModel
    {
    }
}