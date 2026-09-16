using System;

namespace HutkoSDKTest
{
    /// <summary>
    /// Public Hutko sandbox credentials and test data, documented at
    /// docs/hutko-obsidian/01_Getting started/04_Тестування.md.
    /// These are NOT secrets — they are the shared public test merchant. Override
    /// any value with the matching environment variable to run the integration
    /// tests against a private merchant; never hard-code a real secret key here.
    /// </summary>
    internal static class Sandbox
    {
        public static int MerchantId => int.Parse(Env("HUTKO_MERCHANT_ID", "1700002"));

        public static string SecretKey => Env("HUTKO_SECRET_KEY", "test");

        public static string CreditKey => Env("HUTKO_CREDIT_KEY", "testcredit");

        public static string ApiHost => Env("HUTKO_API_HOST", "pay.hutko.org");

        // Documented public test cards.
        public const string CardApproved = "4444555511116666"; // no 3DS, approved
        public const string Card3ds = "4444555566661111";      // 3DS, approved
        public const string Cvv = "111";
        public const string ExpiryDate = "0130";               // MMYY

        private static string Env(string name, string fallback)
        {
            var value = Environment.GetEnvironmentVariable(name);
            return string.IsNullOrEmpty(value) ? fallback : value;
        }
    }
}
