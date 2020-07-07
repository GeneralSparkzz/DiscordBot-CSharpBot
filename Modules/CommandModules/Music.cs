using Discord;
using Discord.Audio;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SnuggleBot.CommandModules
{
    [Group("music")]
    public class Music : ModuleBase<SocketCommandContext>
    {
        Logger _Logger = SnuggleBot.FetchLogger();
        static YoutubeDownloader _YTDownloader = new YoutubeDownloader();
        IAudioClient audioClient;
        Process ffmpeg;
        AudioOutStream discordOutStream;

          [Command("play", RunMode = RunMode.Async)]
        [Summary("Plays the provided music link")]
        public async Task PlayMusic(string Link, IVoiceChannel channel = null)
        {
            _Logger.Log("User: \"" + Context.Message.Author + "\" ran command: \"~music join\"");
            if (Link == null) { return; };

            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null) { await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

            // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
            audioClient = await channel.ConnectAsync();
            await SendAsync(audioClient, await _YTDownloader.DownloadYoutubeLink(Link));
        }
        [Command("leave")]
        [Summary("Makes the bot leave the voice channel its in")]
        public async Task LeaveVoice(IVoiceChannel channel = null)
        {
            _Logger.Log("User: \"" + Context.Message.Author + "\" ran command: \"~music leave\"");

            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null) { await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

            await channel.DisconnectAsync();
        }

        private async Task SendAsync(IAudioClient client, string path)
        {
            // Create FFmpeg using the previous example
            using (ffmpeg = CreateStream(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (discordOutStream = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(discordOutStream); }
                finally { await discordOutStream.FlushAsync(); }
            }
        }
        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }
    }
}
