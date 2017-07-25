using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Wodsoft.Net.Http
{
    public interface IApiFormatter
    {
        Task Serialize(Stream stream, Type valueType, object value);

        Task<object> Deserialize(Stream stream, Type valueType);
    }
}
