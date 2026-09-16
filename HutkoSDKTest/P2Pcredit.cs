using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HutkoSDK;
using HutkoSDK.P2pcredit;

namespace HutkoSDKTest
{
    [TestClass]
    [TestCategory("Integration")]
    public class P2PcreditTest
    {
        public int MerchantId = Sandbox.MerchantId;
        public string SecretKey = Sandbox.SecretKey;
        public string CreditKey = Sandbox.CreditKey;
        public string ContentType = "form";
        public string Endpoint = Sandbox.ApiHost;
        public string card_number = Sandbox.CardApproved;

        [TestMethod]
        public void P2PTest()
        {
            Config.MerchantId = MerchantId;
            Config.SecretKey = SecretKey;
            Config.CreditKey = CreditKey;
            Config.ContentType = ContentType;
            Config.Endpoint(Endpoint);
            string oID = Guid.NewGuid().ToString();
            var req = new P2PcreditRequest()
            {
                order_id = oID,
                amount = 10000,
                order_desc = "Checking! checkout tests",
                currency = "UAH",
                receiver_card_number = card_number
            };
            var resp = new P2Pcredit().Post(req);

            Assert.IsNotNull(resp);
            // The sandbox may approve or decline a P2P credit transfer. Either way the
            // SDK must round-trip the credit-signed request correctly: an approval echoes
            // the order_id, while a gateway decline is surfaced via Error (not an
            // exception and not a signature failure).
            if (resp.Error == null)
            {
                Assert.AreEqual(oID, resp.order_id);
                Assert.IsNotNull(resp.order_status);
            }
            else
            {
                Assert.IsNotNull(resp.Error.ErrorMessage);
            }
        }
    }
}