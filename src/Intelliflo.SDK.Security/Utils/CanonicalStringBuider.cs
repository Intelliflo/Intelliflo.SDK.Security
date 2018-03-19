using System;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Intelliflo.SDK.Security.Utils
{
    internal sealed class CanonicalStringBuider : ICanonicalStringBuider
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
}