using System;
using System.Collections.Generic;
using System.Text;

namespace Intelliflo.SDK.Security.Utils
{
    internal sealed class UrlBuilder
    {
        private string absoluteUri;
        private readonly List<KeyValuePair<string, string>> queryValues = new List<KeyValuePair<string, string>>();

        public void AddAbsoluteUri(string uri)
        {
            absoluteUri = uri ?? throw new ArgumentNullException(nameof(uri));
        }

        public void AddQueryParam(string paramName, string value)
        {
            if (paramName == null)
                throw new ArgumentNullException(nameof(paramName));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            queryValues.Add(new KeyValuePair<string, string>(paramName, value));
        }

        public Uri ToUri(bool encodeParameters = false)
        {
            if (queryValues.Count == 0)
                return new Uri(absoluteUri);

            var builder = new StringBuilder(absoluteUri);

            var query = new Uri(absoluteUri).GetQuery();

            if (string.IsNullOrEmpty(query))
            {
                builder.Append("?");
            }
            else
            {
                if (!query.EndsWith("&"))
                    builder.Append("&");
            }

            for (var i = 0; i < queryValues.Count; i++)
            {
                if (i > 0)
                    builder.Append("&");

                var key = queryValues[i].Key;
                var value = queryValues[i].Value;

                if (encodeParameters)
                {
                    key = key.UriEncode(true);
                    value = value.UriEncode(true);
                }

                builder.Append(key + "=" + value);
            }

            return new Uri(builder.ToString());
        }
    }
}