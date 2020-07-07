using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnuggleBot
{
    class YoutubeDownloader
    {
        Logger _Logger = SnuggleBot.FetchLogger();
        private static readonly string DownloadPath = Path.Combine(Directory.GetCurrentDirectory(), "Temp");

        public async Task<string> DownloadYoutubeLink(string url)
        {
            _Logger.Log("Checking provided url: " + url);
            if (url.ToLower().Contains("youtube.com"))
            {
                _Logger.Log("Attempting video download!");
                TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

                new Thread(()=>
                {
                    string file;
                    int count = 0;

                    do
                    {
                        file = Path.Combine(DownloadPath, "MusicTemp" + ++count + ".mp3");
                    } while (File.Exists(file));

                    Process youtubedl;

                    ProcessStartInfo youtubedlDownload = new ProcessStartInfo()
                    {
                        FileName = "youtube-dl",
                        Arguments = $"-x --audio-format mp3 -o \"{file.Replace(".mp3", ".%(ext)s")}\" {url}",
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                    };

                    youtubedl = Process.Start(youtubedlDownload);
                    youtubedl.WaitForExit();

                    //Thread.Sleep(1000);
                    if (File.Exists(file))
                    {
                        _Logger.Log("Download done!");
                        tcs.SetResult(file);
                    }
                    else
                    {
                        _Logger.Log("Download failed!");
                        tcs.SetResult(null);

                    }
                }).Start();
                string result = await tcs.Task;
                if (result == null)
                {
                    _Logger.Log("Download failed!");
                    throw new Exception("youtube-dl.exe failed to download!");
                }

                //Remove \n at end of Line
                result = result.Replace("\n", "").Replace(Environment.NewLine, "");


                _Logger.Log("end reached");
                return result;
            }
            else
            {
                _Logger.Log("Exception thrown!");
                throw new Exception("Video not supported!");
            }
        }
    }
}
