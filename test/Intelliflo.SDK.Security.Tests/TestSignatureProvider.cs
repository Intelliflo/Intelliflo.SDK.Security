using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace Intelliflo.SDK.Security.Tests
{
    [TestFixture]
    public class TestSignatureProvider
    {
        private SignatureProvider underTest;
        private DateTime time;
        private string secret;
        private string appId;

        [SetUp]
        public void SetUp()
        {
            underTest = new SignatureProvider();
            time = DateTime.UtcNow;
            secret = "secret";
            appId = "xf6tge1";
        }

        [TestCase("https://intelliflo.com", "GET", "some text", 0)]
        [TestCase("https://intelliflo.com", "POST", "some text", 0)]
        [TestCase("https://intelliflo.com", "GET", null, 0)]
        [TestCase("https://intelliflo.com", "GET", null, 59)]
        [TestCase("https://intelliflo.com/", "GET", "some text", 0)]
        [TestCase("https://intelliflo.com/go?", "GET", "some text", 0)]
        [TestCase("https://intelliflo.com/go?x=y", "GET", "some text", 0)]
        [TestCase("https://intelliflo.com", "gEt", "some text", 0)]
        [TestCase("https://intelliflo.com/a/intelliflo?s=22466452&product=IP_IFLO", "GET", "some text", 0)]
        [TestCase("https://intelliflo.com/#/intelliflo?s=22466452&product=IP_IFLO", "GET", "some text", 0)]
        public void Verify_When_v1_Algorithm_User_Should_Return_True(string url, string method, string body, int futureSeconds)
        {
            var uri = new Uri(url);

            var unsignedRequest = SignatureRequest.CreateSignRequest(uri, time, appId, secret, method, body, algorithm: "IO2-HMAC-SHA256");

            var signedUrl = underTest.Sign(unsignedRequest);

            var signedRequest = SignatureRequest.CreateVerificationRequest(signedUrl, time.AddSeconds(futureSeconds), secret, method, 60, body);

            underTest.Verify(signedRequest).Should().BeTrue();
        }


        [TestCase("https://intelliflo.com", "GET", "some text", 0)]
        [TestCase("https://intelliflo.com", "POST", "some text", 0)]
        [TestCase("https://intelliflo.com", "GET", null, 0)]
        [TestCase("https://intelliflo.com", "GET", null, 59)]
        [TestCase("https://intelliflo.com/", "GET", "some text", 0)]
        [TestCase("https://intelliflo.com/go?", "GET", "some text", 0)]
        [TestCase("https://intelliflo.com/go?x=y", "GET", "some text", 0)]
        [TestCase("https://intelliflo.com", "gEt", "some text", 0)]
        [TestCase("https://intelliflo.com/a/intelliflo?s=22466452&product=IP_IFLO", "GET", "some text", 0)]
        [TestCase("https://intelliflo.com/#/intelliflo?s=22466452&product=IP_IFLO", "GET", "some text", 0)]
        public void Verify_When_v2_Algorithm_User_Should_Return_True(string url, string method, string body, int futureSeconds)
        {
            var uri = new Uri(url);

            var unsignedRequest = SignatureRequest.CreateSignRequest(uri, time, appId, secret, method, body, algorithm: "IO2-HMAC-SHA256");

            var signedUrl = underTest.Sign(unsignedRequest);

            var signedRequest = SignatureRequest.CreateVerificationRequest(signedUrl, time.AddSeconds(futureSeconds), secret, method, 60, body);

            underTest.Verify(signedRequest).Should().BeTrue();
        }

        [Test]
        public void Can_verify_signature_with_multiple_unsigned_headers()
        {
            var uri = new Uri("https://intelliflo.com");
            var method = "POST";
            var body = "hey";

            var unsignedRequest = SignatureRequest.CreateSignRequest(uri, time, appId, secret, method, body);
            unsignedRequest.Headers.Add("Content-Type", "text/plain");
            unsignedRequest.Headers.Add("Accept", "text/plain");

            var signedUrl = underTest.Sign(unsignedRequest);

            var signedRequest = SignatureRequest.CreateVerificationRequest(signedUrl, time.AddSeconds(1), secret, method, 60, body, unsignedRequest.Headers);

            underTest.Verify(signedRequest).Should().BeTrue();
        }

        [Test]
        public void Can_verify_signature_with_multiple_signed_headers()
        {
            var uri = new Uri("https://intelliflo.com");
            var method = "POST";
            var body = "hey";

            var unsignedRequest = SignatureRequest.CreateSignRequest(uri, time, appId, secret, method, body);
            unsignedRequest.Headers.Add("Content-Type", "text/plain");
            unsignedRequest.Headers.Add("Accept", "text/plain");
            unsignedRequest.SignedHeaders.Clear();
            foreach (var key in unsignedRequest.Headers.Keys)
                unsignedRequest.SignedHeaders.Add(key);

            var signedUrl = underTest.Sign(unsignedRequest);

            var signedRequest = SignatureRequest.CreateVerificationRequest(signedUrl, time.AddSeconds(1), secret, method, 60, body, unsignedRequest.Headers);
            signedRequest.SignedHeaders.Clear();

            foreach (var header in unsignedRequest.SignedHeaders)
            {
                signedRequest.SignedHeaders.Add(header);
            }

            underTest.Verify(signedRequest).Should().BeTrue();
        }

        [TestCase("https://intelliflo.com,http://intelliflo.com", "GET,GET", "some text,some text", "secret,secret", 0)]
        [TestCase("https://intelliflo.com,https://intelliflo.com", "POST,GET", "some text,some text", "secret,secret", 0)]
        [TestCase("https://intelliflo.com,https://intelliflo.com", "GET,GET", "some text,some other text", "secret,secret", 0)]
        [TestCase("https://intelliflo.com,https://intelliflo.com", "GET,GET", "some text,some text", "secret,secret", 61)]
        [TestCase("https://intelliflo.com,http://intelliflo.com", "GET,GET", "some text,some text", "secret,fake", 0)]
        public void Cannot_verify_signature(string url, string method, string body, string testSecret, int futureSeconds)
        {
            var uri = new Uri(First(url));

            var unsignedRequest = SignatureRequest.CreateSignRequest(uri, time, appId, First(testSecret), First(method), First(body));

            var signedUrl = underTest.Sign(unsignedRequest);

            signedUrl = new Uri(signedUrl.AbsoluteUri.Replace(First(url), Second(url)));

            var signedRequest = SignatureRequest.CreateVerificationRequest(signedUrl, time.AddSeconds(futureSeconds), testSecret, Second(method), 60, Second(body));

            underTest.Verify(signedRequest).Should().BeFalse();
        }

        [Test]
        [TestCaseSource(nameof(CreateTestCases))]
        public void Sign_Should_Produse_Expected_Urls(SignatureRequest request, string expectedResult)
        {
            var signature = new SignatureProvider().Sign(request);

            signature.ToString().Should().Be(expectedResult);
        }

        public static IEnumerable<object[]> CreateTestCases()
        {
            yield return new object[]
            {
                SignatureRequest.CreateSignRequest(
                    new Uri(
                        "http://development.matrix.local.co.uk/Pages/Account/IOAppInstall.aspx?event=before_appinstall&ioUserID=81960&ioAppID=fbd9844&ioReturnUrl=https://uat-apps.intelligent-office.net/preview-apps/fbd9844/install/preview?token=fbd9844-1518435999701"),
                        new DateTime(2018, 2, 22, 11, 46, 39, DateTimeKind.Utc),
                        "xxx",
                        "fbd9844",
                        "GET",
                        null,
                        900),
                "http://development.matrix.local.co.uk/Pages/Account/IOAppInstall.aspx?event=before_appinstall&ioUserID=81960&ioAppID=fbd9844&ioReturnUrl=https://uat-apps.intelligent-office.net/preview-apps/fbd9844/install/preview?token=fbd9844-1518435999701&x-iflo-Algorithm=IO2-HMAC-SHA256&x-iflo-Credential=xxx&x-iflo-Date=20180222T114639Z&x-iflo-SignedHeaders=host&x-iflo-Signature=52ca5b4b18373eb2d255eb9ee68bc8968a0ace9d69b8870c4aed37dd1bc2e7c3"
            };

            yield return new object[]
            {
                            SignatureRequest.CreateSignRequest(
                                new Uri(
                                    "http://development.matrix.local.co.uk/Pages/Account/IOAppInstall.aspx?event=before_appinstall&ioUserID=81960&ioAppID=fbd9844&ioReturnUrl=https%3A%2F%2Fuat-apps.intelligent-office.net%2Fpreview-apps%2Ffbd9844%2Finstall%2Fpreview%3Ftoken%3Dfbd9844-1518435999701"),
                                new DateTime(2019, 2, 22, 11, 46, 39, DateTimeKind.Utc),
                                "aaa",
                                "fbd9844"),
                            "http://development.matrix.local.co.uk/Pages/Account/IOAppInstall.aspx?event=before_appinstall&ioUserID=81960&ioAppID=fbd9844&ioReturnUrl=https:%2F%2Fuat-apps.intelligent-office.net%2Fpreview-apps%2Ffbd9844%2Finstall%2Fpreview%3Ftoken%3Dfbd9844-1518435999701&x-iflo-Algorithm=IO2-HMAC-SHA256&x-iflo-Credential=aaa&x-iflo-Date=20190222T114639Z&x-iflo-SignedHeaders=host&x-iflo-Signature=1e8504d60c37391426e233818b9f2cd7dde4f2bd6e3719609103d09d28f30db6"
            };

            yield return new object[]
            {
                SignatureRequest.CreateSignRequest(
                    new Uri(
                        "http://dragon.local.co.uk/Pages/Account/IOAppInstall.aspx?event=before_appinstall&ioUserID=81960&ioAppID=fbd9844&ioReturnUrl=https://uat-apps.intelligent-office.net/preview-apps/fbd9844/install/preview?token=fbd9844-1518435999701"),
                    new DateTime(2018, 2, 22, 11, 46, 39, DateTimeKind.Utc),
                    "xxx",
                    "fbd9844",
                    "GET",
                    null,
                    900),
                "http://dragon.local.co.uk/Pages/Account/IOAppInstall.aspx?event=before_appinstall&ioUserID=81960&ioAppID=fbd9844&ioReturnUrl=https://uat-apps.intelligent-office.net/preview-apps/fbd9844/install/preview?token=fbd9844-1518435999701&x-iflo-Algorithm=IO2-HMAC-SHA256&x-iflo-Credential=xxx&x-iflo-Date=20180222T114639Z&x-iflo-SignedHeaders=host&x-iflo-Signature=8cc267d46588af28dab6404eaa0e92986895ea21d3a369f3496634b92ad1921a"
            };

            yield return new object[]
            {
                SignatureRequest.CreateSignRequest(
                    new Uri(
                        "https://developer.intelliflo.com/docs/Pre-SignedURLs?x=y&foo=bar"),
                    new DateTime(2018, 2, 22, 11, 46, 39, DateTimeKind.Utc),
                    "myCredential",
                    "mySecret",
                    "GET",
                    null,
                    900),
                "https://developer.intelliflo.com/docs/Pre-SignedURLs?x=y&foo=bar&x-iflo-Algorithm=IO2-HMAC-SHA256&x-iflo-Credential=myCredential&x-iflo-Date=20180222T114639Z&x-iflo-SignedHeaders=host&x-iflo-Signature=7664790e62b988b01ebff76716cd5c6c651cc6b0ba350c7b6be29278f347a77e"
            };
        }

        private static string First(string str)
        {
            return str.Split(',')[0];
        }
        private static string Second(string str)
        {
            return str.Split(',')[1];
        }
    }
}
