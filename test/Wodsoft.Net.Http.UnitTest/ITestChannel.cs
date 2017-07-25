using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Wodsoft.Net.Http.UnitTest
{
    [ApiChannel("Test")]
    public interface ITestChannel
    {
        Task<string> TestMethod1(string arg1);
    }
}
