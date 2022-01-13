using Discord;
using Discord.WebSocket;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBotFrameworkAdapter
{
    public class DiscordAdapter : BotAdapter
    {
        private string token;
        private DiscordSocketClient client;
        private IBot bot;
        private ILogger log;
        public DiscordAdapter(IBot bot, ILogger<DiscordAdapter> logger, IConfiguration config)
        {
            this.token = config.GetValue<string>("DiscordToken");
            this.bot = bot;
            this.log = logger;
        }


        public async Task Run(CancellationToken cancellationToken)
        {
            DiscordSocketConfig config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.DirectMessages | GatewayIntents.DirectMessageTyping | GatewayIntents.DirectMessageTyping,
            };
            client = new DiscordSocketClient(config);
            var channel = client.GetChannel(2);
            client.MessageReceived += HandleMessageReceived;
            client.Log += Log;
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
        }

        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            var responeses = new List<ResourceResponse>();
            foreach(var activity in activities)
            {
                var channel = await client.GetDMChannelAsync(ulong.Parse(activity.ChannelId));
                switch (activity.Type)
                {
                    case ActivityTypes.Message:
                        await channel.SendMessageAsync(activity.Text);
                        break;
                    case ActivityTypes.Typing:
                        await channel.TriggerTypingAsync();
                        break;
                    case ActivityTypes.MessageDelete:
                        await channel.DeleteMessageAsync(ulong.Parse(activity.AsMessageDeleteActivity().Id));
                        break;
                }
            }
            return responeses.ToArray();
        }

        private Task Log(LogMessage message)
        {
            log.Log(ConvertLogLevel(message.Severity), message.Message);
            return Task.CompletedTask;
        }

        private LogLevel ConvertLogLevel(LogSeverity input)
        {
            switch (input)
            {
                case LogSeverity.Critical:
                    return LogLevel.Critical;
                case LogSeverity.Debug:
                    return LogLevel.Debug;
                case LogSeverity.Error:
                    return LogLevel.Error;
                case LogSeverity.Info:
                    return LogLevel.Information;
                case LogSeverity.Verbose:
                    return LogLevel.Information;
                case LogSeverity.Warning:
                    return LogLevel.Warning;
                default:
                    return LogLevel.Warning;
            }
        }

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task HandleMessageReceived(SocketMessage message)
        {
            try
            {
                // Stop from listening to messages from ourselves
                if (message.Author.Id != client.CurrentUser.Id)
                {
                    using (var context = new TurnContext(this, ConvertDiscordMessageToActivity(message)))
                    {
                        await RunPipelineAsync(context, bot.OnTurnAsync, new CancellationToken());
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
        }

        private Activity ConvertDiscordMessageToActivity(SocketMessage message)
        {
            var attachments = new List<Microsoft.Bot.Schema.Attachment>();
            foreach(var attachement in message.Attachments)
            {
                attachments.Add(new Microsoft.Bot.Schema.Attachment(contentUrl: attachement.Url));
            }
            return new Activity()
            {
                Text = message.Content,
                Attachments = attachments,
                ChannelId = message.Channel.Id.ToString(),
                Conversation = new ConversationAccount(id: message.Channel.Id.ToString()),
                Type = ActivityTypes.Message,
                From = new ChannelAccount(message.Author.Id.ToString(), message.Author.Username)
            };
        }
    }
}
