using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Wodsoft.Net.Http.UnitTest.Core
{
    [ApiChannel("Math")]
    public interface IMathApi
    {
        Task<double> Add(double left, double right);

        Task<double> Divide(double left, double right);

        Task<double> Subtract(double left, double right);

        Task<double> Multiply(double left, double right);
    }
}
