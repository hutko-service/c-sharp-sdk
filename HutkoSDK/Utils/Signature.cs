using System;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace HutkoSDK.Utils
{
    public static class Signature
    {
        /// <summary>
        /// Generate the signature version 2
        /// </summary>
        public static string GetRequestSignatureV2(string data, bool credit = false)
        {
            string secret = Config.SecretKey;
            if (credit)
            {
                secret = Config.CreditKey;
            }
            string signature = secret + "|" + data;
            return GetSha1(signature).ToLower();
        }
        /// <summary>
        /// Generate the signature version 1
        /// </summary>
        public static string GetRequestSignature(IEnumerable<string> hashKeys, bool credit = false)
        {
            string signature = string.Join("|", hashKeys);
            string secret = Config.SecretKey;
            if (credit)
            {
                secret = Config.CreditKey;
            }
            signature = secret + "|" + signature;
            return GetSha1(signature).ToLower();
        }

        /// <summary>
        /// Generate Sha1. SHA-1 is required by the Hutko signature specification
        /// (the gateway validates SHA-1); it must not be swapped for another algorithm.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string GetSha1(string value)
        {
            var data = Encoding.UTF8.GetBytes(value);
            using (var sha1 = SHA1.Create())
            {
                var hashData = sha1.ComputeHash(data);
#if NET5_0_OR_GREATER
                return Convert.ToHexString(hashData).ToLowerInvariant();
#else
                var hash = new StringBuilder(hashData.Length * 2);
                foreach (var b in hashData)
                {
                    hash.Append(b.ToString("x2"));
                }

                return hash.ToString();
#endif
            }
        }
        /// <summary>
        /// Encode base64 String
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Base64Encode(string data) {
            var plainTextBytes = Encoding.UTF8.GetBytes(data);
            return Convert.ToBase64String(plainTextBytes);
        }
        
        /// <summary>
        /// Decode base64 String
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Base64Decode(string data) {
            byte[] decoded = Convert.FromBase64String(data);
            return Encoding.UTF8.GetString(decoded);
        }
    }
}