using System;
using System.Collections.Generic;
using Intelliflo.SDK.Security.Algorithms;

namespace Intelliflo.SDK.Security
{
    public sealed partial class SignatureProvider
    {
        private static readonly Dictionary<string, ISigningAlgorithm> SupportedAlgorithms = new Dictionary<string, ISigningAlgorithm>(StringComparer.OrdinalIgnoreCase)
        {
            [Io2HmacSha256SigningAlgorithm.AlgorithmName] = new Io2HmacSha256SigningAlgorithm()
        };

        public bool Verify(SignatureRequest arg)
        {
            if (arg == null)
                throw new ArgumentNullException(nameof(arg));

            if (!SupportedAlgorithms.TryGetValue(arg.Algorithm, out var algorithm))
                throw new ArgumentException($"Algorithm \"{arg.Algorithm}\" is not supported", nameof(arg));

            return algorithm.Verify(arg);
        }

        public Uri Sign(SignatureRequest arg)
        {
            if (arg == null)
                throw new ArgumentNullException(nameof(arg));

            if (!SupportedAlgorithms.TryGetValue(arg.Algorithm, out var algorithm))
                throw new ArgumentException($"Algorithm \"{arg.Algorithm}\" is not supported", nameof(arg));

            return algorithm.Sign(arg);
        }
    }
}
