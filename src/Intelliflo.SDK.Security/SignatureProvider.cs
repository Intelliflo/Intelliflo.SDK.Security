using System;
using System.Text;
using Intelliflo.SDK.Security.Utils;

namespace Intelliflo.SDK.Security
{
    public sealed class SignatureProvider
    {
        public const string AlgorithmKey = "x-iflo-Algorithm";
        public const string CredentialKey = "x-iflo-Credential";
        public const string DateKey = "x-iflo-Date";
        public const string ExpiresKey = "x-iflo-Expires";
        public const string SignedHeadersKey = "x-iflo-SignedHeaders";
        public const string SignatureKey = "x-iflo-Signature";

        private readonly IHashCalculator hashCalculator;
        private readonly ICanonicalStringBuider canonicalStringBuider;

        public SignatureProvider()
            : this(new CanonicalStringBuider(), new HashCalculator())
        {
            
        }

        public SignatureProvider(ICanonicalStringBuider canonicalStringBuider, IHashCalculator hashCalculator)
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
    }
}
