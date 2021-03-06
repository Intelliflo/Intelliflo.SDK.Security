﻿using FluentAssertions;
using Intelliflo.SDK.Security.Utils;
using NUnit.Framework;

namespace Intelliflo.SDK.Security.Tests.Utils
{
    public class TestUrlBuilder
    {
        private UrlBuilder underTest;

        [SetUp]
        public void SetUp()
        {
            underTest = new UrlBuilder();
        }

        [Test]
        public void AddAbsoluteUri_Without_Query_String_Should_Provide_The_Same_Uri()
        {
            underTest.AddAbsoluteUri("http://google.com?q=dragon");

            underTest.ToUri().ToString().Should().Be("http://google.com/?q=dragon");
        }

        [TestCase("http://google.com", "q", "value", "http://google.com/?q=value")]
        [TestCase("http://google.com", "q a a ", "b b b ", "http://google.com/?q a a =b b b")]
        [TestCase("http://google.com?p1=v1", "q a a ", "b b b ", "http://google.com/?p1=v1&q a a =b b b")]
        public void AddQueryParam_Should_Appedn_Parameters_Without_UrlEncoding(string url, string queryParam, string value, string expectedResult)
        {
            underTest.AddAbsoluteUri(url);
            underTest.AddQueryParam(queryParam, value);

            underTest.ToUri().ToString().Should().Be(expectedResult);
        }

        [Test]
        public void AddQueryParam_When_Several_Parameters_Added_Should_Produce_Expected_Url()
        {
            underTest.AddAbsoluteUri("http://google.com?p1=v1");
            underTest.AddQueryParam("q2", "v2");
            underTest.AddQueryParam("q3", "v3");

            underTest.ToUri().ToString().Should().Be("http://google.com/?p1=v1&q2=v2&q3=v3");
        }
    }
}
