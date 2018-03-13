using System;
using System.Collections.Generic;
using FluentAssertions;
using Intelliflo.SDK.Security.Utils;
using NUnit.Framework;

namespace Intelliflo.SDK.Security.Tests.Utils
{
    public class TestDateConverter
    {
        [Test]
        [TestCaseSource(nameof(CreateToIso8601FormatTests))]
        public void ToIso8601Format_Should_Return_Expected_String(DateTime value, string expectedResult)
        {
            value.ToIso8601Format().Should().Be(expectedResult);
        }

        [Test]
        [TestCaseSource(nameof(CreateToIso8601FormatTests))]
        public void FromIso8601Format_Should_Return_Expected_DateTime(DateTime expectedResult, string value)
        {
            value.FromIso8601Format().Should().Be(expectedResult);
        }

        public static IEnumerable<object[]> CreateToIso8601FormatTests()
        {
            yield return new object[] { new DateTime(2017, 1, 2, 3, 4, 5), "20170102T030405Z" };
            yield return new object[] { new DateTime(2018, 12, 22, 1, 2, 3), "20181222T010203Z" };
        }
    }
}
