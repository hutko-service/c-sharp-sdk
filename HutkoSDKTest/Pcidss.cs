using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HutkoSDK;
using HutkoSDK.Payment;

namespace HutkoSDKTest
{
    [TestClass]
    [TestCategory("Integration")]
    public class Psidss
    {
        public int MerchantId = Sandbox.MerchantId;
        public string SecretKey = Sandbox.SecretKey;
        public string ContentType = "json";
        public string Endpoint = Sandbox.ApiHost;
        public string card_number = Sandbox.CardApproved;
        public string card_number_3ds = Sandbox.Card3ds;
        public string cvv2 = Sandbox.Cvv;
        public string expiry_date = Sandbox.ExpiryDate;

        [TestMethod]
        public void PcidssStepOne()
        {
            Config.MerchantId = MerchantId;
            Config.SecretKey = SecretKey;
            Config.ContentType = ContentType;
            Config.Endpoint(Endpoint);
            string orderId = Guid.NewGuid().ToString();
            var req = new StepOneRequest
            {
                order_id = orderId,
                amount = 10000,
                order_desc = "Checking! checkout tests",
                currency = "UAH",
                card_number = card_number,
                cvv2 = cvv2,
                expiry_date = expiry_date
            };
            var resp = new Pcidss().StepOne(req);

            Assert.IsNotNull(resp);
            Assert.AreEqual("approved", resp.order_status);
            Assert.AreEqual(orderId, resp.order_id);
            Assert.IsNotNull(resp.order_id);
        }

        [TestMethod]
        public void PcidssStepTwo()
        {
            Config.MerchantId = MerchantId;
            Config.SecretKey = SecretKey;
            Config.ContentType = "form";
            Config.Endpoint(Endpoint);
            var req = new StepOneRequest()
            {
                order_id = Guid.NewGuid().ToString(),
                amount = 10000,
                order_desc = "Проверка! checkout tests",
                currency = "UAH",
                card_number = card_number_3ds,
                cvv2 = cvv2,
                expiry_date = expiry_date
            };
            var resp = new Pcidss().StepOne(req);

            Assert.IsNotNull(resp);
            Assert.IsNotNull(resp.md);
            Assert.IsNotNull(resp.pareq);
            Assert.IsNull(resp.order_id);
        }
    }
}