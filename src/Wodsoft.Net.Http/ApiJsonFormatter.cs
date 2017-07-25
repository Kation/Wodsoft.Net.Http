using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Wodsoft.Net.Http
{
    public class ApiJsonFormatter : IApiFormatter
    {
        public JsonSerializer _Serializer;

        public ApiJsonFormatter()
        {
            _Serializer = Newtonsoft.Json.JsonSerializer.CreateDefault();
        }

        public Task<object> Deserialize(Stream stream, Type valueType)
        {
            return Task.FromResult(_Serializer.Deserialize(new StreamReader(stream), valueType));
        }

        private static readonly Task _CompletedTask = Task.FromResult(0);
        public async Task Serialize(Stream stream, Type valueType, object value)
        {
            var writer = new StreamWriter(stream);
            _Serializer.Serialize(writer, value, valueType);
            await writer.FlushAsync();
        }
    }
}
