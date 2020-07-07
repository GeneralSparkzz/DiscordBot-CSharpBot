using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SnuggleBot
{
    class Logger
    {
        public Task Log(string Msg)
        {
            Console.WriteLine(DateTime.Now + " - " + Msg);
            return Task.CompletedTask;
        }
        public Task Log(LogMessage Msg)
        {
            Log(Msg.ToString());
            return Task.CompletedTask;
        }


        public void FileWrite(string msg)
        {

        }
    }
}
