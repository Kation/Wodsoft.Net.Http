using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wodsoft.Net.Http.UnitTest
{
    [TestClass]
    public class ChannelBuilderTest
    {
        [TestMethod]
        public void BuildTest()
        {
            ApiClient client = new ApiClient(new Uri("http://www.test.com/api/"), new ApiJsonFormatter());
            var testChannel = client.GetChannel<ITestChannel>();
            Assert.IsNotNull(testChannel);
            Assert.IsNotNull(testChannel.TestMethod1("test"));
        }
    }
}
