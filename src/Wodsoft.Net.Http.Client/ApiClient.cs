using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Wodsoft.Net.Http
{
    public class ApiClient : IDisposable
    {
        private ApiChannelBuilder _Builder;
        private HttpClient _Client;

        public ApiClient(Uri serviceUri, IApiFormatter formatter) : this(serviceUri, formatter, false)
        { }

        public ApiClient(Uri serviceUri, IApiFormatter formatter, bool useCookie)
        {
            if (serviceUri == null)
                throw new ArgumentNullException(nameof(serviceUri));
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));
            HttpClientHandler handler = new HttpClientHandler();
            if (useCookie)
            {
                handler.CookieContainer = new CookieContainer();
                handler.UseCookies = true;
            }
            _Client = new HttpClient(handler);
            _Client.BaseAddress = serviceUri;
            _Builder = new ApiChannelBuilder(_Client, formatter);
        }

        public T GetChannel<T>()
        {
            return _Builder.GetChannel<T>();
        }

        protected void Dispose(bool isDisposed)
        {
            if (isDisposed)
                return;
        }

        public void Dispose()
        {
            Dispose(false);
        }
    }
}
