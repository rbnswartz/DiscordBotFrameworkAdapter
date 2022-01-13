using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBotFrameworkAdapter.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void AddDiscordAdapter(this IServiceCollection services)
        {
            services.AddHostedService<DiscordBotService>();
            services.AddSingleton<DiscordAdapter>();
        }
    }
}
