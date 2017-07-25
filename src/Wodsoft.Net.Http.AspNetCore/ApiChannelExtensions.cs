using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Wodsoft.ComBoost;
using Wodsoft.Net.Http.AspNetCore;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApiChannelExtensions
    {
        public static void UseApi(this IApplicationBuilder app, PathString path, Action<ApiChannelProvider> channelRegisterFunc)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            var domainProvider = app.ApplicationServices.GetRequiredService<IDomainServiceProvider>();
            ApiChannelProvider provider = new ApiChannelProvider(domainProvider);
            channelRegisterFunc(provider);
            app.UseMiddleware<ApiChannelMiddleware>(path, provider);
        }
    }
}
