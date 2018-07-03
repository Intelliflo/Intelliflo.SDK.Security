using System;
using System.Collections.Generic;
using System.Web;
using Intelliflo.SDK.Security.Algorithms;
using Intelliflo.SDK.Security.Utils;

namespace Intelliflo.SDK.Security
{
    public sealed class SignatureRequest
    {
        internal const string AlgorithmKey = "x-iflo-Algorithm";
        internal const string CredentialKey = "x-iflo-Credential";
        internal const string DateKey = "x-iflo-Date";
        internal const string ExpiresKey = "x-iflo-Expires";
        internal const string SignedHeadersKey = "x-iflo-SignedHeaders";
        internal const string SignatureKey = "x-iflo-Signature";

        public string Secret { get; set; }
        public string Method { get; set; }
        public Uri Url { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        public ICollection<string> SignedHeaders { get; set; }
        public int ExpirySeconds { get; set; }
        public string Body { get; set; }
        public string Credential { get; set; }
        public string Algorithm { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime CurrentTime { get; set; }
        public string Signature { get; set; }

        public static SignatureRequest CreateSignRequest(
            Uri url,
            DateTime timeStamp,
            string credential,
            string secret,
            string method = "GET",
            string body = null,
            int expirySeconds = 60,
            string algorithm = Io2HmacSha256SigningAlgorithm.AlgorithmName)
        {
            if (string.IsNullOrEmpty(secret))
                throw new ArgumentException("May not be null or empty", nameof(secret));
            if (string.IsNullOrEmpty(method))
                throw new ArgumentException("May not be null or empty", nameof(method));
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            if (!url.IsAbsoluteUri)
                throw new ArgumentException("Must be absolute Uri", nameof(url));

            var headers = new Dictionary<string, string>
            {
                ["Host"] = url.Host
            };

            return new SignatureRequest
            {
                Url = url,
                Secret = secret,
                Headers = headers,
                Method = method,
                Body = body,
                Credential = credential,
                Timestamp = timeStamp,
                ExpirySeconds = expirySeconds,
                Algorithm = algorithm,
                SignedHeaders = new List<string> { "Host" }
            };
        }

        public static SignatureRequest CreateVerificationRequest(
            Uri url,
            DateTime currentTime,
            string secret,
            string method,
            int expirySeconds = 60,
            string body = null,
            IDictionary<string, string> headers = null)
        {
            if (string.IsNullOrEmpty(secret))
                throw new ArgumentException("May not be null or empty", nameof(secret));
            if (string.IsNullOrEmpty(method))
                throw new ArgumentException("May not be null or empty", nameof(method));
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            if (!url.IsAbsoluteUri)
                throw new ArgumentException("Must be absolute Uri", nameof(url));
            if (expirySeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(expirySeconds));

            var query = url.GetQuery();
            var parts = HttpUtility.ParseQueryString(query);

            var request = CreateSignRequest(
                url,
                parts[DateKey]?.FromIso8601Format() ?? DateTime.MinValue,
                parts[CredentialKey],
                secret,
                method,
                body,
                expirySeconds
            );

            if (headers != null)
                request.Headers = headers;

            request.CurrentTime = currentTime;
            request.Signature = parts[SignatureKey];
            request.Algorithm = parts[AlgorithmKey];

            return request;
        }
    }
}