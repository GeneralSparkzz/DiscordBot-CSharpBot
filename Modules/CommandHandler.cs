using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SnuggleBot
{
    class CommandHandler
    {
        Dictionary<string, string> CatchWords = new Dictionary<string, string>();

        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly Logger _Logger;

        public CommandHandler(DiscordSocketClient client, CommandService commands, Logger logger)
        {
            _commands = commands;
            _client = client;
            _Logger = logger;

            CatchWords.Add("owo", "What's this?~");
            CatchWords.Add("toight", "Toight bois!~");
        }

        public async Task InstallCommandAsync()
        {
            await _Logger.Log("Snuggle bot is getting ready to be commanded~");
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
            _commands.CommandExecuted += OnCommandExecutedAsync;
        }

        private async Task HandleCommandAsync(SocketMessage msgParam)
        {
            var msg = msgParam as SocketUserMessage;
            if (msg == null) { return; };

            int argPos = 0;
            if(msg.HasMentionPrefix(_client.CurrentUser, ref argPos) || msg.Author.IsBot) { return; };
            if (!(msg.HasCharPrefix('~', ref argPos)))
            {
                if (CatchWords.ContainsKey(msg.Content.ToLower().Replace("*", "")))
                {
                    _Logger.Log("Snuggle found a catch word!: " + msg.Content.ToLower().Replace("*", ""));

                    string MsgOut = "Uh oh, something went wrong ;~;";
                    CatchWords.TryGetValue(msg.Content.ToLower().Replace("*", ""), out MsgOut);

                    await msg.Channel.SendMessageAsync(MsgOut);
                }
                return;
            }
            else
            {

                var context = new SocketCommandContext(_client, msg);

                var result = await _commands.ExecuteAsync(context: context, argPos: argPos, services: null);
            };
        }
        public async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // We have access to the information of the command executed,
            // the context of the command, and the result returned from the
            // execution in this event.

            // We can tell the user what went wrong
            if (!string.IsNullOrEmpty(result?.ErrorReason))
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }

            // ...or even log the result (the method used should fit into
            // your existing log handler)
            var commandName = command.IsSpecified ? command.Value.Name : "A command";

            _Logger.Log("Running command: " + commandName);
            /*await _log.LogAsync(new LogMessage(LogSeverity.Info,
                "CommandExecution",
                $"{commandName} was executed at {DateTime.UtcNow}."));*/
        }
    }
}
