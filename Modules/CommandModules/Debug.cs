using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace SnuggleBot.CommandModules
{
    [Group("debug")]
    public class Debug : ModuleBase<SocketCommandContext>
    {
        Logger _Logger = SnuggleBot.FetchLogger();
        SQLQueryIssuer _SqlIssuer = SnuggleBot.FetchSQLIssuer();

        [Command("fetchguildinfo")]
        [Summary("Fetching info about the discord server/guild")]
        public async Task FetchGuildInfo()
        {

            ITextChannel textChannel = null ?? (Context.Channel as ITextChannel);
            // Ulong is 0 to 18,446,744,073,709,551,615 in size.
            await textChannel.SendMessageAsync("***Fetching data:***" +
                "\n***Guild ID:*** "                + Context.Guild.Id +
                "\n***Owner ID:*** "                + Context.Guild.OwnerId + " Name(" + Context.Guild.Owner.Mention + ")" +
                "\n***Member count:*** "            + Context.Guild.MemberCount +
                "\n***Role count:*** "              + Context.Guild.Roles.Count +
                "\n***Region:*** "                  + Context.Guild.VoiceRegionId +
                "\n***Authentication level:*** "    + Context.Guild.MfaLevel);
        }

        [Command ("fetchMyUserInfo")]
        [Summary("Fetches the info for the user issuing the command")]
        public async Task FetchMyUserInfo()
        {

            ITextChannel textChannel = null ?? (Context.Channel as ITextChannel);
            await textChannel.SendMessageAsync("***Fetched user info:*** " +
                "\n***UserID:*** "      + Context.User.Id + "Name(" + Context.User.Mention + ")" +
                "\n***IsBot:*** "       + Context.User.IsBot +
                "\n***IsWebhook:*** "   + Context.User.IsWebhook +
                "\n***Status:*** "      + Context.User.Status.ToString() +
                //"\n***Activity:*** "    + Context.User.Activity.Name +
                "\n***ProfileImg:*** "  + Context.User.GetAvatarUrl(ImageFormat.Auto,128));
        }

        [Command("setupMyInfo")]
        [Summary("Populates command issuers info")]
        public async Task setupMyInfo()
        {
            ITextChannel textChannel = null ?? (Context.Channel as ITextChannel);

            string OutString = Context.Message.Author.Username + "'s info is being added to the database!";

            if(Context.Guild != null)
            {
                //_SqlIssuer.SendNonQuery("exec SP_DoesGuildExist '" + Context.Guild.Id + "';");
                _SqlIssuer.SendNonQuery("exec SP_AddUser '" + Context.Message.Author.Id + "';");
                _SqlIssuer.SendNonQuery("exec SP_AddUserToGuild '" + Context.Message.Author.Id + "','" + Context.Guild.Id + "';");

                await textChannel.SendMessageAsync(OutString);
            }
            else
            {
                OutString = "You must be inside a server!";
            }

        }


        [Command("setupUserCountDisplay")]
        [Summary("Setup channel showing user online count")]
        public async Task setupUserCountDisplay(string ChannelID)
        {
            //ITextChannel displayChannel =
        }

        [Command("UpdateServerInfo")]
        [Summary("Setup or update server info in the database")]
        public async Task UpdateServerInfo()
        {
            if(Context.Guild != null)
            {
                string GuildID = Context.Guild.Id.ToString();
                IReadOnlyCollection<RestBan> Bans = await Context.Guild.GetBansAsync();
                if(Bans.Count == 0)
                {
                    Console.WriteLine("No bans found!");
                }
                else
                {
                    foreach (RestBan ban in Bans)
                    {
                        Console.WriteLine(ban.User + " - " + ban.Reason);
                        _SqlIssuer.SendNonQuery("exec SP_Log_Ban '" + GuildID +"', '" + ban.User.Id.ToString() +"', '" + ban.User + "', '" + ban.Reason +"'");
                    }
                }
            }
            else
            {
                Console.WriteLine("No guild found!");
            }
        }
    }
}
