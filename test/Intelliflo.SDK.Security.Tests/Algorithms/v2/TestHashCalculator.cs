using FluentAssertions;
using Intelliflo.SDK.Security.Algorithms;
using NUnit.Framework;

namespace Intelliflo.SDK.Security.Tests.Algorithms.v2
{
    /// <summary>
    /// Results can be validated using the following online tool https://www.freeformatter.com/sha256-generator.html
    /// </summary>
    public class TestHashCalculator
    {
        private Io2HmacSha256SigningAlgorithm.HashCalculator underTest;

        [SetUp]
        public void SetUp()
        {
            underTest = new Io2HmacSha256SigningAlgorithm.HashCalculator();
        }

        [TestCase("xx", "aa", "935efc218144b5c0ee9fbf91a2db878dea85c3d4b985b6e0a2c503935c85d3a9")]
        [TestCase("value1", "Password1", "a00bfd705f15510c62bea945698a292058496d238c9f1b28f2c1fb2584033d63")]
        [TestCase("DiamondDragon", "Password1", "af50db51919ebb7a74fc931ebcd5423e36d88632180968d4b75c6fca4e1c3a12")]
        [TestCase("Bomb", "Password1", "9dd71ecc728cd95ec6f5da8c8dec1dd132763efd1d32361343b9c591a0141edd")]
        public void GetStringToSignHash_Should_Return_Expected_Hash(string value, string secret, string expectedHash)
        {
            underTest.GetStringToSignHash(value, secret).Should().Be(expectedHash);
        }

        [TestCase("xx", "5dde896887f6754c9b15bfe3a441ae4806df2fde94001311e08bf110622e0bbe")]
        [TestCase("value1", "3c9683017f9e4bf33d0fbedd26bf143fd72de9b9dd145441b75f0604047ea28e")]
        [TestCase("VALUE2", "fe3d147c5902a7c7e0956b7074527202a4965789669a78478a636108b32c63f4")]
        [TestCase("DiamondDragon", "8d243c21df7e57aa48082cd25ca6cbc2e63c50f96a39d67e40811fe1becc7248")]
        public void GetCanonicalRequestHash_Should_Return_Expected_Hash(string value, string expectedHash)
        {
            underTest.GetCanonicalRequestHash(value).Should().Be(expectedHash);
        }
    }
}
