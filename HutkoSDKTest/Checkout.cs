using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HutkoSDK;
using HutkoSDK.Checkout;

namespace HutkoSDKTest
{
    [TestClass]
    [TestCategory("Integration")]
    public class Checkout
    {
        public int MerchantId = Sandbox.MerchantId;
        public string SecretKey = Sandbox.SecretKey;
        public string ContentType = "json";
        public string Endpoint = Sandbox.ApiHost;

        [TestMethod]
        public void TestCheckout()
        {
            Config.MerchantId = MerchantId;
            Config.SecretKey = SecretKey;
            Config.ContentType = ContentType;
            Config.Endpoint(Endpoint);
            string orderId = Guid.NewGuid().ToString();
            var req = new CheckoutRequest
            {
                order_id = orderId,
                amount = 10000,
                order_desc = "проверка! checkout tests demo",
                currency = "USD"
            };
            var resp = new Url().Post(req);

            Assert.IsNotNull(resp);
            Assert.AreEqual("success", resp.response_status);
            Assert.IsNotNull(resp.payment_id);
        }

        [TestMethod]
        public void TestCheckoutXml()
        {
            Config.MerchantId = MerchantId;
            Config.SecretKey = SecretKey;
            Config.ContentType = "xml";
            Config.Endpoint(Endpoint);
            string orderId = Guid.NewGuid().ToString();
            var req = new CheckoutRequest
            {
                order_id = orderId,
                amount = 10000,
                order_desc = "Cheking! checkout tests demo",
                currency = "UAH"
            };
            var resp = new Url().Post(req);

            Assert.IsNotNull(resp);
            Assert.AreEqual("success", resp.response_status);
            Assert.IsNotNull(resp.payment_id);
        }

        [TestMethod]
        public void TestToken()
        {
            Config.MerchantId = MerchantId;
            Config.SecretKey = SecretKey;
            Config.ContentType = ContentType;
            Config.Endpoint(Endpoint);
            var req = new TokenRequest
            {
                order_id = Guid.NewGuid().ToString(),
                amount = 10500,
                order_desc = "Cheking! checkout tests demo",
                currency = "UAH"
            };
            var resp = new Token().Post(req);

            Assert.IsNotNull(resp);
            Assert.AreEqual("success", resp.response_status);
            Assert.IsNotNull(resp.token);
        }

        [TestMethod]
        public void TestXmlToken()
        {
            Config.MerchantId = MerchantId;
            Config.SecretKey = SecretKey;
            Config.ContentType = "xml";
            Config.Endpoint(Endpoint);
            var req = new TokenRequest
            {
                order_id = Guid.NewGuid().ToString(),
                amount = 10500,
                order_desc = "Cheking! checkout tests demo",
                currency = "UAH"
            };
            var resp = new Token().Post(req);

            Assert.IsNotNull(resp);
            Assert.AreEqual("success", resp.response_status);
            Assert.IsNotNull(resp.token);
        }

        [TestMethod]
        public void TestFormToken()
        {
            Config.MerchantId = MerchantId;
            Config.SecretKey = SecretKey;
            Config.ContentType = "form";
            Config.Endpoint(Endpoint);
            var req = new TokenRequest
            {
                order_id = Guid.NewGuid().ToString(),
                amount = 10500,
                order_desc = "Cheking! checkout tests demo",
                currency = "UAH"
            };
            var resp = new Token().Post(req);

            Assert.IsNotNull(resp);
            Assert.AreEqual("success", resp.response_status);
            Assert.IsNotNull(resp.token);
        }
    }
}