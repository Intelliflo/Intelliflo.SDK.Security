using System;

namespace Intelliflo.SDK.Security.Algorithms
{
    internal interface ISigningAlgorithm
    {
        bool Verify(SignatureRequest arg);
        Uri Sign(SignatureRequest arg);
    }
}