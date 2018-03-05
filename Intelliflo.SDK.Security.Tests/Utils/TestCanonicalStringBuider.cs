using System;
using System.Collections.Generic;
using FluentAssertions;
using Intelliflo.SDK.Security.Utils;
using Xunit;

namespace Intelliflo.SDK.Security.Tests.Utils
{
    public class TestCanonicalStringBuider
    {
        private readonly CanonicalStringBuider underTest;
        private readonly SignatureRequest request;

        public TestCanonicalStringBuider()
        {
            request = new SignatureRequest();

            underTest = new CanonicalStringBuider();
        }

        [Theory]
        [InlineData("test", "test")]
        [InlineData("te   st", "te   st")]
        [InlineData("=!%\\//", "=!%\\//")]
        public void BuildCredentials_Should_Return_Unmodified_String(string input, string expectedResult)
        {
            request.Credential = input;

            underTest.BuildCredentials(request).Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(1, "1")]
        [InlineData(1000, "1000")]
        [InlineData(1000000, "1000000")]
        public void BuildExpirySeconds_Should_Return_String_Representation_Of_ExpirySeconds(int value, string expectedResult)
        {
            request.ExpirySeconds = value;

            underTest.BuildExpirySeconds(request).Should().Be(expectedResult);
        }


        [Theory]
        [MemberData(nameof(CreateSignedHeadersTests))]
        public void BuildSignedHeaders_Should_Convet_Headers_Into_Lower_Case_And_Join_Them_Sorting_By_Name(string[] headers, string expectedResult)
        {
            request.SignedHeaders = headers;

            underTest.BuildSignedHeaders(request).Should().Be(expectedResult);
        }

        public static IEnumerable<object[]> CreateSignedHeadersTests()
        {
            yield return new object[] { new[] { "A"}, "a" };
            yield return new object[] { new[] { "A", "B"}, "a;b" };
            yield return new object[] { new[] { "A", "B", "C"}, "a;b;c" };
            yield return new object[] { new[] { "C", "B", "A"}, "a;b;c" };
        }

        [Theory]
        [MemberData(nameof(CreateBuildTimestampTests))]
        public void BuildTimestamp_Should_Convert_Date_To_Utc_And_Format_Using_Iso8601(DateTime value, string expectedResult)
        {
            request.Timestamp = value;

            underTest.BuildTimestamp(request).Should().Be(expectedResult);
        }

        public static IEnumerable<object[]> CreateBuildTimestampTests()
        {
            yield return new object[] { new DateTime(2015, 11, 22, 11, 22, 33, DateTimeKind.Utc), "20151122T112233Z" };
            yield return new object[] { new DateTime(2013, 5, 23, 11, 22, 33, DateTimeKind.Utc), "20130523T112233Z" };
        }

        [Theory]
        [InlineData("test", "test")]
        [InlineData("te   st", "te   st")]
        [InlineData("=!%\\//", "=!%\\//")]
        public void BuildBody_Should_Return_Unmodified_String(string input, string expectedResult)
        {
            request.Body = input;

            underTest.BuildBody(request).Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("GET", "GET")]
        [InlineData("get", "GET")]
        [InlineData("pOsT", "POST")]
        public void BuildMethod_Should_Return_Method_In_Upper_Case(string input, string expectedResult)
        {
            request.Method = input;

            underTest.BuildMethod(request).Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("ALGO", "ALGO")]
        [InlineData("A-L/\\Go", "A-L/\\GO")]
        [InlineData("algo", "ALGO")]
        public void BuildAlgorithm_Should_Return_Method_In_Upper_Case(string input, string expectedResult)
        {
            request.Algorithm = input;

            underTest.BuildAlgorithm(request).Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("http://google.com", "http%3A//google.com/")]
        [InlineData("http://google.com/", "http%3A//google.com/")]
        [InlineData("http://google.com/a", "http%3A//google.com/a")]
        [InlineData("http://google.com/my lin", "http%3A//google.com/my%2520lin")]
        [InlineData("http://google.com/a/b", "http%3A//google.com/a/b")]
        [InlineData("http://google.com/a/b/", "http%3A//google.com/a/b/")]
        [InlineData("http://google.com/a/b?c=d", "http%3A//google.com/a/b")]
        [InlineData("http://google.com/a/b#e?c=d", "http%3A//google.com/a/b")]
        public void BuildUrl_Should_Return_Expected_Value(string input, string expectedResult)
        {
            request.Url = new Uri(input);

            underTest.BuildUrl(request).Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("http://google.com", "")]
        [InlineData("http://google.com/?q=123", "q=123")]
        [InlineData("http://google.com?q=123&a=234", "a=234&q=123")]
        [InlineData("http://google.com?q=ma%20ma", "q=ma%20ma")]
        [InlineData("http://google.com?q=ma ma", "q=ma%20ma")]
        [InlineData("http://google.com/?q=123&a=234&c=", "a=234&c=&q=123")]
        [InlineData("http://google.com/?q%20q=123&a=234&c=", "a=234&c=&q%20q=123")]
        [InlineData("http://google.com/?q q=123&a=234&c=", "a=234&c=&q%20q=123")]
        [InlineData("http://google.com/a/b#e?c=d", "c=d")]
        public void BuildQueryString_Should_Return_Sorted_And_Encoded_Result(string input, string expectedResult)
        {
            request.Url = new Uri(input);

            underTest.BuildQueryString(request).Should().Be(expectedResult);
        }

        [Theory]
        [MemberData(nameof(CreateBuildHeadersTests))]
        public void BuildHeaders_Should_Change_Headers_To_Lower_Case_And_Trim_Value_Spaces(Dictionary<string, string> value, string expectedResult)
        {
            request.Headers = value;

            underTest.BuildHeaders(request).Should().Be(expectedResult);
        }

        public static IEnumerable<object[]> CreateBuildHeadersTests()
        {
            yield return new object[]
            {
                new Dictionary<string, string>
                {
                    ["HOST"] = "http://google.com",
                    ["header1"] = " VALUE ",
                    ["HeAdEr2"] = "some value",
                },
                "header1:VALUE\nheader2:some value\nhost:http://google.com"
            };
            yield return new object[]
            {
                new Dictionary<string, string>
                {
                    ["HOST"] = "http://google.com",
                },
                "host:http://google.com"
            };
            yield return new object[]
            {
                new Dictionary<string, string>(),
                string.Empty
            };
        }
    }
}
