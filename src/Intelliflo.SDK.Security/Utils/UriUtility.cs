using System;
using System.Collections.Specialized;
using System.Text;

namespace Intelliflo.SDK.Security.Utils
{
    public static class UriUtility
    {
        internal static string GetQuery(this Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            if (string.IsNullOrWhiteSpace(uri.AbsoluteUri))
                return string.Empty;

            var url = uri.AbsoluteUri;

            var index = url.IndexOf("?", StringComparison.Ordinal);

            if (index < 0)
                return string.Empty;

            var query = url.Substring(index);

            return query;
        }

        internal static NameValueCollection Filter(this NameValueCollection collection, Predicate<string> filter)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            var result = new NameValueCollection();

            foreach (var item in collection)
            {
                if(item == null)
                    continue;

                if (filter(item.ToString()))
                    result.Add(item.ToString(), collection[item.ToString()]);
            }

            return result;
        }

        public static string UriEncode(this string value, bool encodeSlash)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var result = new StringBuilder();

            foreach (var ch in value)
            {
                if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '_' || ch == '-' || ch == '~' || ch == '.')
                {
                    result.Append(ch);
                }
                else switch (ch)
                {
                    case '/':
                        var slash = encodeSlash ? "%2F" : ch.ToString();
                        result.Append(slash);
                        break;
                    case ' ':
                        result.Append("%20");
                        break;
                    default:
                        result.Append($"%{Convert.ToByte(ch).ToString("x2").ToUpperInvariant()}");
                        break;
                }
            }

            return result.ToString();
        }
    }
}