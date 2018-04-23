using System;

namespace Intelliflo.SDK.Security.Algorithms
{
    internal interface ISigningAlgorithm
    {
        bool Verify(SignatureRequest arg, int expirySeconds = 60);
        Uri Sign(SignatureRequest arg);
    }
}