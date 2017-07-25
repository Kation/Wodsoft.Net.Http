using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Wodsoft.ComBoost;

namespace Wodsoft.Net.Http.AspNetCore
{
    public class ApiChannelProvider
    {
        internal Dictionary<string, IDomainService> Channels;
        private IDomainServiceProvider _DomainProvider;

        public ApiChannelProvider(IDomainServiceProvider domainProvider)
        {
            Channels = new Dictionary<string, IDomainService>();
            _DomainProvider = domainProvider;
        }

        public void RegisterChannel<T>() where T : IDomainService
        {
            Type type = typeof(T);
            foreach (var item in type.GetInterfaces())
            {
                var attribute = item.GetTypeInfo().GetCustomAttribute<ApiChannelAttribute>();
                if (attribute == null)
                    continue;
                if (Channels.ContainsKey(attribute.ChannelName))
                    throw new InvalidOperationException("该通道已注册为：" + Channels[attribute.ChannelName].GetType().FullName);
                Channels.Add(attribute.ChannelName.ToLower(), _DomainProvider.GetService<T>());
            }
        }

        public IDomainService GetChannel(string channelName)
        {
            IDomainService service;
            Channels.TryGetValue(channelName.ToLower(), out service);
            return service;
        }
    }
}
