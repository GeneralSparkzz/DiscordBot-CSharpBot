using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SnuggleBot.CommandModules
{
    [Group("reaction")]
    public class ReactionPosts : ModuleBase<SocketCommandContext>
    {
        [Command("createpoll")]
        [Summary("e")]
        public async Task createPoll()
        {
            ITextChannel textChannel = null ?? (Context.Channel as ITextChannel);
            await textChannel.SendMessageAsync("Fetched: " + Context.Message.Content);
            string CurText = Context.Message.Content;
        }
    }
}
