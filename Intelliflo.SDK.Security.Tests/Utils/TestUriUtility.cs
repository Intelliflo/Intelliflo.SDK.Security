using System;
using FluentAssertions;
using Intelliflo.SDK.Security.Utils;
using NUnit.Framework;

namespace Intelliflo.SDK.Security.Tests.Utils
{
    public class TestUriUtility
    {
        [TestCase("http://google.com?q=123")]
        [TestCase("http://development.matrix.local.co.uk/Pages/Account/IOAppInstall.aspx?event=before_appinstall&ioUserID=81960&ioAppID=fbd9844&ioReturnUrl=https://uat-apps.intelligent-office.net/preview-apps/fbd9844/install/preview?token=fbd9844-1518435999701")]
        [TestCase("http://development.matrix.local.co.uk/Pages/Account/IOAppInstall.aspx?event=before_appinstall&ioUserID=81960&ioAppID=fbd9844&ioReturnUrl=https%3A%2F%2Fuat-apps.intelligent-office.net%2Fpreview-apps%2Ffbd9844%2Finstall%2Fpreview%3Ftoken%3Dfbd9844-1519305161545")]
        public void Test(string value)
        {
            var uri = new Uri(value);

            var urlQuery = uri.Query;
            var extensionQuery = uri.GetQuery();

            urlQuery.Should().Be(extensionQuery);
        }
    }
}