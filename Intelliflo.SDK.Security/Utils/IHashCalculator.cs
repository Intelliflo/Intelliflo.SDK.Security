namespace Intelliflo.SDK.Security.Utils
{
    public interface IHashCalculator
    {
        string GetStringToSignHash(string value, string secret);
        string GetCanonicalRequestHash(string canonicalRequest);
    }
}