using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Wodsoft.Net.Http
{
    public class ApiChannel
    {
        private HttpClient _Client;
        private string _Channel;
        private IApiFormatter _Formatter;

        protected ApiChannel(HttpClient client, string channel, IApiFormatter formatter)
        {
            _Client = client ?? throw new ArgumentNullException(nameof(client));
            _Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            _Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        protected async Task<T> ExecuteAsync<T>(string method, IDictionary<string, object> values)
        {
            string data = JsonConvert.SerializeObject(values);
            StringContent content = new StringContent(data);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await _Client.PostAsync(_Channel + "/" + method, content);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new NotSupportedException("该Api不存在。");
            else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException("服务器出现未知错误。");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                try
                {
                    return (T)await _Formatter.Deserialize(stream, typeof(T));
                }
                catch (Exception ex)
                {
                    throw new InvalidCastException("解析结果发生错误。", ex);
                }
            }
            else
                throw new NotSupportedException("该Api不存在。");
        }
    }
}
