using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wodsoft.ComBoost;
using Wodsoft.ComBoost.AspNetCore;

namespace Wodsoft.Net.Http.AspNetCore
{
    public class ApiChannelMiddleware
    {
        private RequestDelegate _Next;
        private ApiChannelProvider _ChannelProvider;
        private PathString _Path;
        public ApiChannelMiddleware(RequestDelegate next, PathString path, ApiChannelProvider channelProvider)
        {
            _Next = next ?? throw new ArgumentNullException(nameof(next));
            _Path = path;
            _ChannelProvider = channelProvider ?? throw new ArgumentNullException(nameof(channelProvider));

            _Methods = channelProvider.Channels.ToDictionary(t => t.Key.ToLower(), t =>
                t.Value.GetType().GetInterfaces().Single(s => s.GetTypeInfo().GetCustomAttribute<ApiChannelAttribute>()?.ChannelName.ToLower() == t.Key)
                .GetMethods().ToDictionary(k => k.Name.ToLower(), k => t.Value.GetType().GetTypeInfo().GetDeclaredMethod(k.Name))
            );
        }

        public async Task Invoke(HttpContext httpContext)
        {
            PathString path = httpContext.Request.PathBase.Add(_Path);
            if (httpContext.Request.Path.StartsWithSegments(path))
            {
                var formatter = httpContext.RequestServices.GetRequiredService<IApiFormatter>();
                if (formatter == null)
                    throw new InvalidOperationException("找不到“IApiFormatter”对象。");
                var values = httpContext.Request.Path.ToUriComponent().Substring(path.ToUriComponent().Length + 1).Split('/');
                if (values.Length != 2)
                    goto notfound;
                var domainService = _ChannelProvider.GetChannel(values[0]);
                if (domainService == null)
                    goto notfound;
                var methodName = values[1];
                MethodInfo method;
                if (!_Methods[values[0].ToLower()].TryGetValue(methodName.ToLower(), out method))
                    goto notfound;
                var context = new HttpDomainContext(httpContext);
                if (httpContext.Request.Headers["content-type"].ToString().Contains("application/json"))
                {
                    context.ValueProvider.ValueSelectors.Clear();
                    context.ValueProvider.ValueSelectors.Add(new HttpJsonValueSelector(httpContext));
                }
                var valueType = method.ReturnType.GetGenericArguments();
                var executeMethod = _ExecuteAsync.MakeGenericMethod(valueType);
                Task task = (Task)executeMethod.Invoke(domainService, new object[] { context, method });
                await task;
                var resultProperty = method.ReturnType.GetProperty("Result");
                var result = resultProperty.GetValue(task);
                await formatter.Serialize(httpContext.Response.Body, valueType[0], result);
                return;

                notfound:
                httpContext.Response.StatusCode = 404;
                return;
            }
            await _Next(httpContext);
        }

        private Dictionary<string, Dictionary<string, MethodInfo>> _Methods;
        private static readonly MethodInfo _ExecuteAsync = typeof(IDomainService).GetTypeInfo().GetDeclaredMethods("ExecuteAsync").First(t => t.IsGenericMethodDefinition);
    }
}
