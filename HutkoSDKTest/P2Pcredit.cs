using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HutkoSDK;
using HutkoSDK.P2pcredit;

namespace HutkoSDKTest
{
    [TestClass]
    public class P2PcreditTest
    {
        public int MerchantId = 1700002;
        public string SecretKey = "test";
        public string CreditKey = "testcredit";
        public string ContentType = "form";
        public string Endpoint = "pay.hutko.org";
        public string card_number = "4444555511116666";

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
            Assert.AreEqual(oID, resp.order_id);
            Assert.IsNotNull(resp.order_status);
        }
    }
}