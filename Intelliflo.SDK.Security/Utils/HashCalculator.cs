using System;
using System.Security.Cryptography;
using System.Text;

namespace Intelliflo.SDK.Security.Utils
{
    internal sealed class HashCalculator : IHashCalculator
    {
        private static readonly Encoding DefaultEncoding = Encoding.Unicode;


        public string GetStringToSignHash(string value, string secret)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (secret == null)
                throw new ArgumentNullException(nameof(secret));

            return ToLowerBase64(ToHmacsha256Hash(value, secret));
        }

        public string GetCanonicalRequestHash(string canonicalRequest)
        {
            if (canonicalRequest == null)
                throw new ArgumentNullException(nameof(canonicalRequest));

            return ToLowerBase64(ToSha256Hash(canonicalRequest));
        }

        private static string ToSha256Hash(string str)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException(nameof(str));

            using (var sha256 = new SHA256Managed())
            {
                var hash = new StringBuilder();

                foreach (var b in sha256.ComputeHash(DefaultEncoding.GetBytes(str), 0, DefaultEncoding.GetByteCount(str)))
                {
                    hash.Append(b.ToString("x2"));
                }

                return hash.ToString();
            }
        }

        private static string ToHmacsha256Hash(string str, string secret)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException(nameof(str));
            if (string.IsNullOrEmpty(secret))
                throw new ArgumentNullException(nameof(secret));

            using (var hmac = new HMACSHA256(Encoding.Unicode.GetBytes(secret)))
            {
                return Convert.ToBase64String(
                    hmac.ComputeHash(
                        DefaultEncoding.GetBytes(str)));
            }
        }


        private static string ToLowerBase64(string str)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException(nameof(str));

            return Convert.ToBase64String(DefaultEncoding.GetBytes(str)).ToLower();
        }

    }
}