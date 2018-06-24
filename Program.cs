using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace SuperUsefulBot
{
    class Program
    {
        static void Main(string[] args) => new Program().SuperUsefulBotAsync().GetAwaiter().GetResult();

        private CommandService commands;
        private DiscordSocketClient client;
        private IServiceProvider services;

        private async Task SuperUsefulBotAsync()
        {
            client = new DiscordSocketClient();
            commands = new CommandService();

            services = new ServiceCollection()
                .BuildServiceProvider();
            
            client.Log += Log;
            //client.MessageReceived += MessageReceived;

            await InstallCommands();

            await client.LoginAsync(TokenType.Bot, Config.Token);
            await client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        /*private async Task MessageReceived(SocketMessage message)
        {
            if (message.Content == "bot test")
            {
                await message.Channel.SendMessageAsync("I'm here");
            }
        }*/

        public async Task InstallCommands()
        {
            // Hook the MessageReceived Event into our Command Handler
            client.MessageReceived += HandleCommand;
            // Discover all of the commands in this assembly and load them.
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            Console.WriteLine("Entered command: " + message);
            if (message == null) return;
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))) return;
            // Create a Command Context
            var context = new CommandContext(client, message);
            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            var result = await commands.ExecuteAsync(context, argPos, services);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }

}
