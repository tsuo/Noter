using System;
using System.IO;

using Discord.WebSocket;

namespace DiscordNote
{
    class Program
    {
        static void Main(string[] args)
        {
            DiscordSocketClient bot = new DiscordSocketClient();

            if (Directory.Exists($"data"))
            {
                if (File.Exists($"data/user.txt"))
                    using (StreamReader sr = new StreamReader($"data/user.txt"))
                    {
                        new Client(bot, sr.ReadLine(), sr.ReadLine(), sr.ReadLine(), $"data/").Login().GetAwaiter().GetResult();
                    }
            }
            else
            {
                Directory.CreateDirectory($"data");
                File.Create($"data/user.txt");
            }
        }
    }
}
