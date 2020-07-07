using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SnuggleBot
{
    class SnuggleBot
    {
        private static DiscordSocketClient _Client;

        private static CommandService _Commands;
        private static CommandHandler _CommandHandler;
        private static EventHandler _EventHandler;

        private static Logger _Logger;
        private static SQLQueryIssuer _SQLIssuer;

        public SnuggleBot(Logger logger, SQLQueryIssuer issuer)
        {
            _Logger = logger;
            _SQLIssuer = issuer;
            this.BotMainAsync().GetAwaiter().GetResult();
        }

        public async Task BotMainAsync()
        {
            _Client = new DiscordSocketClient(new DiscordSocketConfig{AlwaysDownloadUsers = true,MessageCacheSize = 100});
            _Commands = new CommandService();
            _EventHandler = new EventHandler(_Client, _SQLIssuer);
            _CommandHandler = new CommandHandler(_Client, _Commands, _Logger);

            await _CommandHandler.InstallCommandAsync();

            _Client.Log += _Logger.Log;

            // -------------
            // Event setup
            // -------------
            // Fired when a user joins a guild
            _Client.UserJoined += _EventHandler.UserJoined;

            // fired when a user is banned from a guild
            _Client.UserBanned += _EventHandler.UserBanned;

            // Fired when a user is unbanned from a guild
            _Client.UserUnbanned += _EventHandler.UserUnbanned;
            
            // Fired when a user leaves a server
            _Client.UserLeft += _EventHandler.UserLeft;

            // Fired when the bot is added to a new server
            _Client.JoinedGuild += _EventHandler.JoinedGuild;
            
            // Reactions
            _Client.ReactionAdded += _EventHandler.ReactionAdded;
            _Client.ReactionRemoved += _EventHandler.ReactionRemoved;
            _Client.ReactionsCleared += _EventHandler.ReactionsCleared;
            _Client.GuildAvailable += _EventHandler.GuildAvailable;
            _Client.GuildMembersDownloaded += _EventHandler.GuildMembersDownloaded;

            /*
            _Client.ChannelCreated += ;
            _Client.ChannelDestroyed += ;
            _Client.ChannelUpdated += ;
            _Client.Connected += ;
            _Client.CurrentUserUpdated += ;
            _Client.Disconnected += ;
            _Client.GuildAvailable += ;
            _Client.GuildUnavailable += ;
            _Client.GuildUpdated += ;
            _Client.LatencyUpdated += ;
            _Client.LeftGuild += ;
            _Client.MessageDeleted += ;
            _Client.MessageReceived += ;
            _Client.MessagesBulkDeleted += ;
            _Client.MessageUpdated += ;
            _Client.RoleCreated += ;
            _Client.RoleDeleted += ;
            _Client.RoleUpdated += ;
            _Client.UserIsTyping += ;
            _Client.VoiceServerUpdated += ;
            */

            // Fired when the bot is ready in a server
            _Client.Ready += BotReady;

            await _Client.LoginAsync(TokenType.Bot, "Token Removed for Protection");
            await _Client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task BotReady()
        {
            await _Logger.Log("Snuggle bot is ready to play!~");
        }

        public static Logger FetchLogger()
        {
            return _Logger;
        }
        public static SQLQueryIssuer FetchSQLIssuer()
        {
            return _SQLIssuer;
        }
        public static DiscordSocketClient FetchClient()
        {
            return _Client;
        }
    }
}
