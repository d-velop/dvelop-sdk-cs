using System;
using System.Security.Cryptography;
using System.Text;

namespace Dvelop.Sdk.SigningAlgorithms
{
    public static class HmacSha256Algorithm
    {
        public static string Sha256(string input)
        {
            return BitConverter.ToString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", "").ToLower();
        }

        public static string HmacSha256(byte[] key, string input)
        {
            var sha = new HMACSHA256(key);
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", "").ToLower();
        }
    }
}