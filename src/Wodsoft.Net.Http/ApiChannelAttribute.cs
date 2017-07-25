using System;
using System.Collections.Generic;
using System.Text;

namespace Wodsoft.Net.Http
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class ApiChannelAttribute : Attribute
    {
        public ApiChannelAttribute(string name)
        {
            ChannelName = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string ChannelName { get; }
    }
}
