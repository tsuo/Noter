using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Discord.WebSocket;
using Discord.Commands;
using Discord.Commands.Builders;

namespace DiscordNote
{
    class Client
    {
        DiscordSocketClient Bot = null;
        String Token = null;
        String MasterName = null;
        String MasterID = null;
        String DataPath = null;
        public Client(DiscordSocketClient bot, String token, String master, String masterID, String path)
        {
            Bot = bot;
            Token = token;
            MasterName = master;
            MasterID = masterID;
            DataPath = path;

            bot.Log += Log;
            bot.MessageReceived += Bot_MessageReceived;
            
        }

        private Task Log(Discord.LogMessage arg)
        {
            switch (arg.Severity)
            {
                case Discord.LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case Discord.LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case Discord.LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                case Discord.LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case Discord.LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case Discord.LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                default:
                    break;
            }
            Console.Write("[" + arg.Severity + "]");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(arg.Message);
            return Task.CompletedTask;
        }

        #region MESSAGE RECEIVED
        private Task Bot_MessageReceived(SocketMessage arg)
        {
            String text = arg.Content;
            if (text[0] == '>')
            {
                String[] args = text.Split(' ');
                String output = "";

                switch (args[0].ToLower())
                {
                    #region COMMAND NOTE
                    case ">note":

                        String path = $"{DataPath}note{arg.Author.Id}.txt";
                        
                        if (!File.Exists(path))
                        {
                            Stream str = File.Create(path);
                            str.Dispose();
                            output += $"{arg.Author.Username}, could not find note file so a new note file was created.\n";
                        }

                        if (args.Length > 1)
                        {
                            switch (args[1].ToLower())
                            {
                                case "-a":
                                    if (args.Length > 2)
                                    {
                                        using (StreamWriter sw = File.AppendText(path))
                                        {
                                            String notetxt = text.Substring(args[0].Length + args[1].Length + 2).Trim(' ');
                                            String noteToAdd = $"[{DateTime.Now.Date.Month}/{DateTime.Now.Date.Day}|{DateTime.Now.TimeOfDay.Hours}:{DateTime.Now.TimeOfDay.Minutes}] {notetxt}";
                                            sw.WriteLine(noteToAdd);
                                            output += $"Added note {noteToAdd}\n";
                                        }
                                    }
                                    break;
                                case "-r":
                                    if (args.Length > 2)
                                    {
                                        int noteId = 0;
                                        if (int.TryParse(args[2].Trim(' '), out noteId) && noteId > 0)
                                        {

                                            String tempPath = $"{DataPath}notetemp{arg.Author.Id}.txt";
                                            String[] lines = File.ReadAllLines(path);
                                            using (StreamWriter sw = new StreamWriter($"{DataPath}notetemp{arg.Author.Id}.txt"))
                                            {
                                                for (int i = 0; i < lines.Length; i++)
                                                {
                                                    if (i != noteId - 1)
                                                    {
                                                        sw.WriteLine(lines[i]);
                                                    }
                                                }
                                            }
                                            File.Delete(path);
                                            File.Move(tempPath, path);
                                            output += $"Removed note {noteId}. {lines[noteId - 1]}";

                                        }
                                    }
                                    break;
                                case "-ra":
                                    {
                                        File.Delete(path);
                                        File.Create(path).Dispose();
                                        output += $"{arg.Author.Username}, all notes cleared.\n";
                                    }
                                    break;
                                default:
                                    int offset = 1;
                                    if (int.TryParse(text.Substring(args[0].Trim(' ').Length + 1), out offset))
                                    {
                                        {
                                            String[] lines = File.ReadAllLines(path);
                                            if (lines.Length > 0 && offset > 0 && offset <= lines.Length)
                                            {
                                                for (int i = offset - 1; i <= offset + 2 && i < lines.Length; i++)
                                                {
                                                    output += $"{i + 1}. {lines[i]}\n";
                                                }
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                        else if (args.Length == 1)
                        {
                            output += $"You have {File.ReadAllLines(path).Length} line(s) of note.\nInput command \">note ##\" to return notes from that line. Replace ## with a note ID.\n";

                        }

                        break;
                    #endregion COMMAND NOTE
                    #region COMMAND OTHER
                    case ">data":
                        DirectoryInfo dir = new DirectoryInfo(DataPath);
                        foreach (FileInfo fi in dir.GetFiles())
                        {
                            output += $"{fi.Name},";
                        }
                        output = output.TrimEnd(',');
                        break;
                    #endregion COMMAND OTHER
                    default:

                        break;
                }

                if (!output.Trim(' ').Equals(""))
                {
                    arg.Channel.SendMessageAsync($"```{output}```");
                }
            }

            return Task.CompletedTask;
        }
        #endregion MESSAGE RECEIVED

        #region LOGIN
        public async Task Login()
        {
            await Bot.LoginAsync(Discord.TokenType.Bot, Token);
            await Bot.StartAsync();

            await Task.Delay(-1);
        }
        #endregion LOGIN

    }
}
