using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Intelliflo.SDK.Security.Utils;

namespace Intelliflo.SDK.Security
{
    public sealed partial class SignatureProvider
    {
        public const string AlgorithmKey = "x-iflo-Algorithm";
        public const string CredentialKey = "x-iflo-Credential";
        public const string DateKey = "x-iflo-Date";
        public const string ExpiresKey = "x-iflo-Expires";
        public const string SignedHeadersKey = "x-iflo-SignedHeaders";
        public const string SignatureKey = "x-iflo-Signature";

        private readonly HashCalculator hashCalculator;
        private readonly CanonicalStringBuider canonicalStringBuider;

        public SignatureProvider()
            : this(new CanonicalStringBuider(), new HashCalculator())
        {
            
        }

        internal SignatureProvider(CanonicalStringBuider canonicalStringBuider, HashCalculator hashCalculator)
        {
            this.canonicalStringBuider = canonicalStringBuider ?? throw new ArgumentNullException(nameof(canonicalStringBuider));
            this.hashCalculator = hashCalculator ?? throw new ArgumentNullException(nameof(hashCalculator));
        }

        public bool Verify(SignatureRequest arg)
        {
            if (arg == null)
                throw new ArgumentNullException(nameof(arg));

            if (arg.CurrentTime > arg.Timestamp.AddSeconds(arg.ExpirySeconds))
                return false;

            var stringToSign = BuildStringToSign(arg);
            var signature = hashCalculator.GetStringToSignHash(stringToSign, arg.Secret);

            return arg.Signature.Equals(signature, StringComparison.InvariantCulture);
        }

        public Uri Sign(SignatureRequest arg)
        {
            if (arg == null)
                throw new ArgumentNullException(nameof(arg));

            var urlBuilder = new UrlBuilder();

            urlBuilder.AddAbsoluteUri(arg.Url.AbsoluteUri);
            urlBuilder.AddQueryParam(AlgorithmKey, canonicalStringBuider.BuildAlgorithm(arg));
            urlBuilder.AddQueryParam(CredentialKey, canonicalStringBuider.BuildCredentials(arg));
            urlBuilder.AddQueryParam(DateKey, canonicalStringBuider.BuildTimestamp(arg));
            urlBuilder.AddQueryParam(ExpiresKey, canonicalStringBuider.BuildExpirySeconds(arg));
            urlBuilder.AddQueryParam(SignedHeadersKey, canonicalStringBuider.BuildSignedHeaders(arg));

            var stringToSign = BuildStringToSign(arg);
            var signature = hashCalculator.GetStringToSignHash(
                stringToSign,
                arg.Secret);

            urlBuilder.AddQueryParam(SignatureKey, signature);

            return urlBuilder.ToUri();
        }

        private string BuildStringToSign(SignatureRequest arg)
        {
            var builder = new StringBuilder();

            builder.Append(canonicalStringBuider.BuildAlgorithm(arg) + "\n"); 
            builder.Append(canonicalStringBuider.BuildTimestamp(arg) + "\n"); 
            builder.Append(hashCalculator.GetCanonicalRequestHash(BuildCanonicalRequest(arg)));

            return builder.ToString();
        }

        private string BuildCanonicalRequest(SignatureRequest arg)
        {
            var builder = new StringBuilder();

            builder.Append(canonicalStringBuider.BuildMethod(arg) + "\n");
            builder.Append(canonicalStringBuider.BuildUrl(arg) + "\n");
            builder.Append(canonicalStringBuider.BuildQueryString(arg) + "\n");
            builder.Append(canonicalStringBuider.BuildHeaders(arg) + "\n");
            builder.Append(canonicalStringBuider.BuildSignedHeaders(arg) + "\n");
            builder.Append(canonicalStringBuider.BuildBody(arg));

            return builder.ToString();
        }

        #region Nested classes

        internal sealed class HashCalculator
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

        internal sealed class CanonicalStringBuider
        {
            public string BuildCredentials(SignatureRequest arg)
            {
                if (arg == null)
                    throw new ArgumentNullException(nameof(arg));

                return arg.Credential;
            }

            public string BuildExpirySeconds(SignatureRequest arg)
            {
                if (arg == null)
                    throw new ArgumentNullException(nameof(arg));

                return arg.ExpirySeconds.ToString(CultureInfo.InvariantCulture);
            }

            public string BuildSignedHeaders(SignatureRequest arg)
            {
                if (arg == null)
                    throw new ArgumentNullException(nameof(arg));

                var signedHeaders = arg
                    .SignedHeaders
                    .Select(x => x.ToLowerInvariant())
                    .OrderBy(x => x);

                return string.Join(";", signedHeaders);
            }

            public string BuildTimestamp(SignatureRequest arg)
            {
                if (arg == null)
                    throw new ArgumentNullException(nameof(arg));

                return arg.Timestamp.ToUniversalTime().ToIso8601Format();
            }

            public string BuildBody(SignatureRequest arg)
            {
                if (arg == null)
                    throw new ArgumentNullException(nameof(arg));

                return arg.Body;
            }

            public string BuildMethod(SignatureRequest arg)
            {
                if (arg == null)
                    throw new ArgumentNullException(nameof(arg));

                return arg.Method.ToUpperInvariant();
            }

            public string BuildAlgorithm(SignatureRequest arg)
            {
                if (arg == null)
                    throw new ArgumentNullException(nameof(arg));

                return arg.Algorithm.ToUpperInvariant();
            }

            public string BuildUrl(SignatureRequest arg)
            {
                if (arg == null)
                    throw new ArgumentNullException(nameof(arg));

                return arg.Url.GetLeftPart(UriPartial.Path).UriEncode(false);
            }

            public string BuildQueryString(SignatureRequest arg)
            {
                if (arg == null)
                    throw new ArgumentNullException(nameof(arg));

                var query = arg.Url.GetQuery();
                var queryString = HttpUtility.ParseQueryString(query).Filter(k => k.StartsWith("x-iflo-") == false);

                var sortedQueryPairs =
                    queryString
                        .Keys
                        .OfType<string>()
                        .OrderBy(x => x)
                        .Select(x => $"{x.UriEncode(true)}={(queryString[x] ?? string.Empty).UriEncode(true)}");

                return string.Join("&", sortedQueryPairs);
            }

            public string BuildHeaders(SignatureRequest arg)
            {
                if (arg == null)
                    throw new ArgumentNullException(nameof(arg));

                var sortedHeades =
                    arg
                        .Headers
                        .Keys
                        .OrderBy(x => x)
                        .Select(x => $"{x.ToLowerInvariant()}:{arg.Headers[x].Trim()}");

                return string.Join("\n", sortedHeades);
            }
        }

        #endregion
    }
}
