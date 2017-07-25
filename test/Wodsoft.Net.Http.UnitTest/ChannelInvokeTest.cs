using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Wodsoft.Net.Http.UnitTest.Core;

namespace Wodsoft.Net.Http.UnitTest
{
    [TestClass]
    public class ChannelInvokeTest
    {
        [TestMethod]
        public async Task MathTest()
        {
            ApiClient client = new ApiClient(new Uri("http://localhost:16413/api/"), new ApiJsonFormatter());
            var mathChannel = client.GetChannel<IMathApi>();
            double result = await mathChannel.Add(1, 2);
            Assert.AreEqual(3, result);
            result = await mathChannel.Divide(10, 2);
            Assert.AreEqual(5, result);
            result = await mathChannel.Subtract(3, 4);
            Assert.AreEqual(-1, result);
            result = await mathChannel.Multiply(2.5, 3);
            Assert.AreEqual(7.5, result);
        }
    }
}
