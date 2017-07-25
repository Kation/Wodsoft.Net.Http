using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wodsoft.ComBoost;
using Wodsoft.Net.Http.UnitTest.Core;

namespace Wodsoft.Net.Http.WebTest
{
    public class MathDomainService : DomainService, IMathApi
    {
        public Task<double> Add([FromValue]double left, [FromValue]double right)
        {
            return Task.FromResult(left + right);
        }

        public Task<double> Divide([FromValue]double left, [FromValue]double right)
        {
            return Task.FromResult(left / right);
        }

        public Task<double> Multiply([FromValue]double left, [FromValue]double right)
        {
            return Task.FromResult(left * right);
        }

        public Task<double> Subtract([FromValue]double left, [FromValue]double right)
        {
            return Task.FromResult(left - right);
        }
    }
}
