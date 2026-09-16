using HutkoSDK;
using HutkoSDK.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HutkoSDKTest
{
    /// <summary>
    /// Offline signature tests (no network). Validates the request-signature vector
    /// published in docs/hutko-obsidian/02_API reference/08_Підпис запиту (signature).md,
    /// which guards the SHA-1 hashing and the ordinal key ordering.
    /// </summary>
    [TestClass]
    public class SignatureTests
    {
        [TestMethod]
        public void GetRequestSignature_MatchesDocumentedVector()
        {
            Config.SecretKey = "test";

            // Documented example: "test|125|UAH|1700002|test order|test123456"
            // (the secret key is prepended by GetRequestSignature).
            var values = new[] { "125", "UAH", "1700002", "test order", "test123456" };

            var signature = Signature.GetRequestSignature(values);

            Assert.AreEqual("99edfb423145556bf9e4cb002df09eb717140126", signature);
        }
    }
}
