using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SnuggleBot
{
    class EventHandler
    {
        private static DiscordSocketClient _Client;
        private static SQLQueryIssuer _SQLIssuer;

        public EventHandler(DiscordSocketClient Client, SQLQueryIssuer SQLIssuer)
        {
            _Client = Client;
            _SQLIssuer = SQLIssuer;

            var UsersOnlineTimer = new System.Threading.Timer((e) =>
            {
                Console.WriteLine("Running event");
                IReadOnlyCollection<SocketGuild> Guilds = _Client.Guilds;
                int online = 0;
                int Total = 0;

                foreach (SocketGuild guild in Guilds)
                {
                    online = 0;
                    Total = 0;
                    if(guild.GetChannel(717233309691674665) != null)
                    {
                        IReadOnlyCollection<SocketUser> Users = guild.Users;
                        Total = Users.Count;

                        foreach (SocketUser user in Users)
                        {
                            switch (user.Status)
                            {
                                case UserStatus.Offline:
                                    online += 1;
                                    break;
                                case UserStatus.Invisible:
                                    online += 1;
                                    break;
                                default:
                                    break;
                            }
                        }
                        Console.WriteLine("Members: " + online + "/" + Total);
                        guild.GetChannel(717233309691674665).ModifyAsync(prop => prop.Name = "Members: " + online + "/" + Total);
                    }
                }

            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public async Task UserJoined(SocketGuildUser user)
        {
            Console.WriteLine("Event - Adding user data to guild: " + user.Guild.ToString() + "(" + user.Guild.Id + "), User: " + user.ToString() + "(" + user.Id.ToString() + ")");

            string SafeUserName = user.ToString().Replace("'", "''");
            _SQLIssuer.SendNonQuery("exec SP_AddUser '" + user.Id + "','" + SafeUserName + "'");

            _SQLIssuer.SendNonQuery("exec SP_AddUserToGuild '" + user.Id + "', '" + user.Guild.Id + "'");
            user.Guild.GetTextChannel(719293211801288745).SendMessageAsync(user.Username + " has joined");

        }

        public async Task GuildMembersDownloaded(SocketGuild guild)
        {

            string SafeGuildName = guild.Name.Replace("'", "''");
            Console.WriteLine("Updating guild info: " + guild.ToString() + "(" + guild.Id.ToString() + ")");

            _SQLIssuer.SendNonQuery("exec SP_AddGuild '" + guild.Id.ToString() + "', '" + SafeGuildName + "'");

            IReadOnlyCollection<SocketGuildUser> Users = guild.Users;

            List<string> UsersInDatabase = new List<string>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_SQLIssuer.GetConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand("select * from GuildUserConnector where GuildID = '" + guild.Id + "';", connection))
                    {
                        connection.Open();
                        using (SqlDataReader Reader = cmd.ExecuteReader())
                        {
                            while (Reader.Read())
                            {
                                UsersInDatabase.Add(Reader.GetString("UserID"));
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            string SafeUserName = "";
            foreach (SocketGuildUser user in Users)
            {
                if (user.IsBot == false && user.IsWebhook == false)
                {
                    if (UsersInDatabase.Contains(user.Id.ToString()))
                    {
                        UsersInDatabase.Remove(user.Id.ToString());
                    }
                    Console.WriteLine("Event - Adding user data to guild: " + guild.ToString() + "(" + guild.Id + "), User: " + user.ToString() + "(" + user.Id.ToString() + ")");

                    SafeUserName = user.ToString().Replace("'", "''");
                    _SQLIssuer.SendNonQuery("exec SP_AddUser '" + user.Id + "','" + SafeUserName + "'");

                    _SQLIssuer.SendNonQuery("exec SP_AddUserToGuild '" + user.Id + "', '" + guild.Id + "'");
                }

            }

            if (UsersInDatabase.Count > 0)
            {
                foreach (string id in UsersInDatabase)
                {
                    SocketUser curUser = _Client.GetUser(Convert.ToUInt64(id)); //guild.GetUser(Convert.ToUInt64(id));
                    if (curUser != null)
                    {
                        Console.WriteLine("Event - Removing user from guild: " + guild.ToString() + "(" + guild.Id + "), User: " + curUser.ToString() + "(" + curUser.Id.ToString() + ")");
                        _SQLIssuer.SendNonQuery("exec SP_RemoveUserFromGuild '" + guild.Id + "', '" + id + "'");
                    }
                    else
                    {
                        Console.WriteLine("ERROR - User ID of " + id + " was found in the database but data failed to fetch! Attempting rough remove with stored ID");
                        _SQLIssuer.SendNonQuery("exec SP_RemoveUserFromGuild '" + guild.Id + "', '" + id + "'");
                    }
                }
            }

            IReadOnlyCollection<RestBan> Bans = await guild.GetBansAsync();
            if (Bans.Count > 0)
            {
                foreach (RestBan ban in Bans)
                {
                    Console.WriteLine("Event - Adding ban data to guild: " + guild.ToString() + "(" + guild.Id + "), Ban on: " + ban.User + "(" + ban.User.Id + ")");
                    _SQLIssuer.SendNonQuery("exec SP_Log_Ban '" + guild.Id + "', '" + ban.User.Id.ToString() + "', '" + ban.User + "', '" + ban.Reason + "'");
                }
            }

            Console.WriteLine("Updating info finished\n\n");
        }

        public async Task GuildAvailable(SocketGuild guild)
        {
        }

        public async Task UserLeft(SocketGuildUser user)
        {
            _SQLIssuer.SendNonQuery("exec SP_RemoveUserFromGuild '" + user.Guild.Id.ToString() + "', '" + user.Id.ToString() + "'");

            IReadOnlyCollection<RestBan> Bans = await user.Guild.GetBansAsync();

            bool IsBanned = false;
            foreach (RestBan ban in Bans)
            {
                if (ban.User.Id == user.Id)
                {
                    IsBanned = true;
                    break;
                }
            }
            if(IsBanned == false)
            {
                Console.WriteLine("Event - User left guild: " + user.Guild.ToString() + "(" + user.Guild.Id + "), User: " + user.ToString() + "(" + user.Id.ToString() + ")");
                user.Guild.GetTextChannel(719293211801288745).SendMessageAsync(user.Username + " has left");
            }
        }

        public async Task UserBanned(SocketUser user, SocketGuild guild)
        {
            IReadOnlyCollection<RestBan> Bans = await guild.GetBansAsync();
            string reason = "N/A";

            foreach(RestBan ban in Bans)
            {
                if(ban.User.Id == user.Id)
                {
                    reason = ban.Reason;
                    break;
                }
            }

            _SQLIssuer.SendNonQuery("exec SP_RemoveUserFromGuild '" + guild.Id.ToString() + "', '" + user.Id.ToString() + "'");
            _SQLIssuer.SendNonQuery("exec SP_Log_Ban '" + guild.Id + "', '" + user.Id.ToString() + "', '" + user + "', '" + reason + "'");
            Console.WriteLine("Event - User banned: " + guild.ToString() + "(" + guild.Id + "), User: " + user.ToString() + "(" + user.Id.ToString() + "), Reason: " + reason);
            guild.GetTextChannel(719293211801288745).SendMessageAsync(user.Username + " was banned for " + reason);
        }
        public async Task UserUnbanned(SocketUser user, SocketGuild guild)
        {

            _SQLIssuer.SendNonQuery("delete from Bans where GuildID = '" + guild.Id.ToString() + "' and BannedID = '" + user.Id.ToString() + "';");
            Console.WriteLine("Event - User ban revoked: " + guild.ToString() + "(" + guild.Id + "), User: " + user.ToString() + "(" + user.Id.ToString() + ")");
            guild.GetTextChannel(719293211801288745).SendMessageAsync(user.Username + " was unbanned");
        }


        // --------------------------------------
        // Reactions
        // --------------------------------------
        public async Task ReactionRemoved(Cacheable<IUserMessage, ulong> Message, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            //throw new NotImplementedException();
        }

        public async Task ReactionsCleared(Cacheable<IUserMessage, ulong> Message, ISocketMessageChannel Channel)
        {
            //throw new NotImplementedException();
        }

        public async Task ReactionAdded(Cacheable<IUserMessage, ulong> Message, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            //Console.WriteLine(Reaction.User.Value.Username + " added reaction (" + Reaction.Emote.Name + ") to message: \"" + Channel.GetMessageAsync(Message.Id).Result.Content + "\"");
        }

        public async Task JoinedGuild(SocketGuild arg)
        {
            //throw new NotImplementedException();
        }
    }
}
