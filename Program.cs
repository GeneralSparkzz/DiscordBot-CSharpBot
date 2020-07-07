using System;
using System.Threading.Tasks;

namespace SnuggleBot
{
    class Program
    {
        static Logger logger = new Logger();
        static SQLQueryIssuer issuer = new SQLQueryIssuer();
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            await logger.Log("Snuggle bot is starting! Give me a moment to get warmed up~");
            SnuggleBot _snuggle_bot = new SnuggleBot(logger, issuer);
        }
    }
}
