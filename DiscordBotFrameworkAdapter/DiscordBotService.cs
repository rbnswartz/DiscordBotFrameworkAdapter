using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Hosting;

namespace DiscordBotFrameworkAdapter
{
    public class DiscordBotService : IHostedService
    {
        DiscordAdapter adapter;
        CancellationTokenSource cancellationTokenSource;
        public DiscordBotService(DiscordAdapter adapter)
        {
            this.adapter = adapter;
            this.cancellationTokenSource = new CancellationTokenSource();
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await adapter.Run(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationTokenSource.Cancel();
        }
    }
}
