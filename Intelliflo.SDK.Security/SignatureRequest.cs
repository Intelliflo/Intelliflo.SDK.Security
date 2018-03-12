using System;
using System.Collections.Generic;
using System.Web;
using Intelliflo.SDK.Security.Utils;

namespace Intelliflo.SDK.Security
{
    public sealed class SignatureRequest
    {
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
            string algorith = "IO1-HMAC-SHA256")
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
                Algorithm = algorith,
                SignedHeaders = new List<string> { "Host" }
            };
        }

        public static SignatureRequest CreateVerificationRequest(
            Uri url,
            DateTime currentTime,
            string secret,
            string method,
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

            var query = url.GetQuery();
            var parts = HttpUtility.ParseQueryString(query);

            var request = CreateSignRequest(
                url,
                parts[SignatureProvider.DateKey]?.FromIso8601Format() ?? DateTime.MinValue,
                parts[SignatureProvider.CredentialKey],
                secret,
                method,
                body,
                int.Parse(parts[SignatureProvider.ExpiresKey] ?? "-1")
            );


            if (headers != null)
                request.Headers = headers;

            request.CurrentTime = currentTime;
            request.Signature = parts[SignatureProvider.SignatureKey];
            request.Algorithm = parts[SignatureProvider.AlgorithmKey];

            return request;
        }
    }
}