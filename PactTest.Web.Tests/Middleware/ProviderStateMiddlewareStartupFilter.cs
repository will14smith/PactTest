using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace PactTest.Web.Tests.Middleware
{
    public class ProviderStateMiddlewareStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<ProviderStateMiddleware>();
                next(builder);
            };
        }
    }
}