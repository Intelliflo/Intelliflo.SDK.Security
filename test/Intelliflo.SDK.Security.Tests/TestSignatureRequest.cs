using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;


namespace Intelliflo.SDK.Security.Tests
{
    [TestFixture]
    public class TestSignatureRequest
    {
        [Test]
        public void CreateSignRequest_With_Default_Parameters_Should_Create_Expected_Request()
        {
            var actual = SignatureRequest.CreateSignRequest(
                new Uri("http://google.com?q=dragon"), 
                new DateTime(2013, 5, 23, 10, 11, 13, DateTimeKind.Utc),
                "credential1",
                "secret1");
            
            
            actual.Should().BeEquivalentTo(
                new SignatureRequest
                {
                    Algorithm = "IO1-HMAC-SHA256",
                    Body = null,
                    Headers = new Dictionary<string, string>
                    {
                        ["Host"] = "google.com"
                    },
                    Credential = "credential1",
                    CurrentTime = DateTime.MinValue,
                    Url = new Uri("http://google.com?q=dragon"),
                    Method = "GET",
                    ExpirySeconds = 60,
                    SignedHeaders = new List<string>
                    {
                        "Host"
                    },
                    Timestamp = new DateTime(2013, 5, 23, 10, 11, 13, DateTimeKind.Utc),
                    Secret = "secret1",
                    Signature = null
                });
        }


        [Test]
        public void CreateSignRequest_With_Non_Default_Parameters_Should_Create_Expected_Request()
        {
            var actual = SignatureRequest.CreateSignRequest(
                new Uri("http://google.com?q=dragon"),
                new DateTime(2013, 5, 23, 10, 11, 13, DateTimeKind.Utc),
                "credential1",
                "secret1",
                "POST",
                "body1",
                240,
                "newAlgo");


            actual.Should().BeEquivalentTo(
                new SignatureRequest
                {
                    Algorithm = "newAlgo",
                    Body = "body1",
                    Headers = new Dictionary<string, string>
                    {
                        ["Host"] = "google.com"
                    },
                    Credential = "credential1",
                    CurrentTime = DateTime.MinValue,
                    Url = new Uri("http://google.com?q=dragon"),
                    Method = "POST",
                    ExpirySeconds = 240,
                    SignedHeaders = new List<string>
                    {
                        "Host"
                    },
                    Timestamp = new DateTime(2013, 5, 23, 10, 11, 13, DateTimeKind.Utc),
                    Secret = "secret1",
                    Signature = null
                });
        }


        [Test]
        public void CreateVerificationRequest_With_Default_Parameters_Should_Initialize_Request_From_Url_And_Parameters_ExpirySeconds_Value_Is_Not_Initialized()
        {
            var actual = SignatureRequest.CreateVerificationRequest(
                new Uri("http://dragon.local.co.uk/Pages/Account/IOAppInstall.aspx?event=before_appinstall&ioUserID=81960&ioAppID=fbd9844&ioReturnUrl=https://uat-apps.intelligent-office.net/preview-apps/fbd9844/install/preview?token=fbd9844-1518435999701&x-iflo-Algorithm=IO1-HMAC-SHA256&x-iflo-Credential=xxx&x-iflo-Date=20180222T114639Z&x-iflo-Expires=900&x-iflo-SignedHeaders=host&x-iflo-Signature=taa0agcaugbwaeyamqb0afoavga1ahuavabnahaavwbvaesasgbrafkaawbtaegangbyafeamqbhaegazaa4aeuadab6aheaqgbladeaaqbqadaacwa9aa=="),
                new DateTime(2013, 5, 23, 10, 11, 13, DateTimeKind.Utc),
                "secret1",
                "POST");

            actual.Should().BeEquivalentTo(
                new SignatureRequest
                {
                    Algorithm = "IO1-HMAC-SHA256",
                    Body = null,
                    Headers = new Dictionary<string, string>
                    {
                        ["Host"] = "dragon.local.co.uk"
                    },
                    Credential = "xxx",
                    CurrentTime = new DateTime(2013, 5, 23, 10, 11, 13),
                    Url = new Uri("http://dragon.local.co.uk/Pages/Account/IOAppInstall.aspx?event=before_appinstall&ioUserID=81960&ioAppID=fbd9844&ioReturnUrl=https://uat-apps.intelligent-office.net/preview-apps/fbd9844/install/preview?token=fbd9844-1518435999701&x-iflo-Algorithm=IO1-HMAC-SHA256&x-iflo-Credential=xxx&x-iflo-Date=20180222T114639Z&x-iflo-Expires=900&x-iflo-SignedHeaders=host&x-iflo-Signature=taa0agcaugbwaeyamqb0afoavga1ahuavabnahaavwbvaesasgbrafkaawbtaegangbyafeamqbhaegazaa4aeuadab6aheaqgbladeaaqbqadaacwa9aa=="),
                    Method = "POST",
                    ExpirySeconds = 60, // This value is no longer taken from query string
                    SignedHeaders = new List<string>
                    {
                        "Host"
                    },
                    Timestamp = new DateTime(2018, 2, 22, 11, 46, 39, DateTimeKind.Utc),
                    Secret = "secret1",
                    Signature = "taa0agcaugbwaeyamqb0afoavga1ahuavabnahaavwbvaesasgbrafkaawbtaegangbyafeamqbhaegazaa4aeuadab6aheaqgbladeaaqbqadaacwa9aa=="
                });
        }

        [Test]
        public void CreateVerificationRequest_With_Non_Default_Parameters_Should_Initialize_Request_From_Url_And_Parameters_Except_ExpirySeconds()
        {
            var actual = SignatureRequest.CreateVerificationRequest(
                new Uri("http://dragon.local.co.uk/Pages/Account/IOAppInstall.aspx?event=before_appinstall&ioUserID=81960&ioAppID=fbd9844&ioReturnUrl=https://uat-apps.intelligent-office.net/preview-apps/fbd9844/install/preview?token=fbd9844-1518435999701&x-iflo-Algorithm=IO1-HMAC-SHA256&x-iflo-Credential=xxx&x-iflo-Date=20180222T114639Z&x-iflo-Expires=900&x-iflo-SignedHeaders=host&x-iflo-Signature=taa0agcaugbwaeyamqb0afoavga1ahuavabnahaavwbvaesasgbrafkaawbtaegangbyafeamqbhaegazaa4aeuadab6aheaqgbladeaaqbqadaacwa9aa=="),
                new DateTime(2013, 5, 23, 10, 11, 13, DateTimeKind.Utc),
                "secret1",
                "POST",
                "body1",
                new Dictionary<string, string>
                {
                    ["h1"] = "v1",
                    ["h2"] = "v2"
                });

            actual.Should().BeEquivalentTo(
                new SignatureRequest
                {
                    Algorithm = "IO1-HMAC-SHA256",
                    Body = "body1",
                    Headers = new Dictionary<string, string>
                    {
                        ["h1"] = "v1",
                        ["h2"] = "v2"
                    },
                    Credential = "xxx",
                    CurrentTime = new DateTime(2013, 5, 23, 10, 11, 13),
                    Url = new Uri("http://dragon.local.co.uk/Pages/Account/IOAppInstall.aspx?event=before_appinstall&ioUserID=81960&ioAppID=fbd9844&ioReturnUrl=https://uat-apps.intelligent-office.net/preview-apps/fbd9844/install/preview?token=fbd9844-1518435999701&x-iflo-Algorithm=IO1-HMAC-SHA256&x-iflo-Credential=xxx&x-iflo-Date=20180222T114639Z&x-iflo-Expires=900&x-iflo-SignedHeaders=host&x-iflo-Signature=taa0agcaugbwaeyamqb0afoavga1ahuavabnahaavwbvaesasgbrafkaawbtaegangbyafeamqbhaegazaa4aeuadab6aheaqgbladeaaqbqadaacwa9aa=="),
                    Method = "POST",
                    ExpirySeconds = 60, // Value is not taken from query string
                    SignedHeaders = new List<string>
                    {
                        "Host"
                    },
                    Timestamp = new DateTime(2018, 2, 22, 11, 46, 39, DateTimeKind.Utc),
                    Secret = "secret1",
                    Signature = "taa0agcaugbwaeyamqb0afoavga1ahuavabnahaavwbvaesasgbrafkaawbtaegangbyafeamqbhaegazaa4aeuadab6aheaqgbladeaaqbqadaacwa9aa=="
                });
        }
    }
}
