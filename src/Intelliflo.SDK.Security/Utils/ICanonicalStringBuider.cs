namespace Intelliflo.SDK.Security.Utils
{
    public interface ICanonicalStringBuider
    {
        string BuildCredentials(SignatureRequest arg);
        string BuildExpirySeconds(SignatureRequest arg);
        string BuildSignedHeaders(SignatureRequest arg);
        string BuildTimestamp(SignatureRequest arg);
        string BuildBody(SignatureRequest arg);
        string BuildMethod(SignatureRequest arg);
        string BuildAlgorithm(SignatureRequest arg);
        string BuildUrl(SignatureRequest arg);
        string BuildQueryString(SignatureRequest arg);
        string BuildHeaders(SignatureRequest arg);
    }
}