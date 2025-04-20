using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using SoulBot.Commands;
using SoulBot.config;
using SoulBot.Events;
using System;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System.Threading.Tasks;

namespace SoulBot
{
    internal class Program
    {
        private static DiscordClient Client { get; set; }
        private static CommandsNextExtension Commands { get; set; }
        private static SlashCommandsExtension SlashCommands { get; set; }

        static async Task Main(string[] args)
        {
            // Load config
            var jsonReader = new JsonReader();
            await jsonReader.ReadJson();

            // Setup Discord client
            var discordConfig = new DiscordConfiguration
            {
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents | DiscordIntents.GuildMessages,
                Token = jsonReader.token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            Client = new DiscordClient(discordConfig);
            Client.Ready += OnClientReady;
            Client.MessageCreated += MessageCreatedHandler.OnMessageCreated;



            // Setup traditional commands
            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new[] { jsonReader.prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                CaseSensitive = false,
                EnableDefaultHelp = false
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<TestCommands>();

            // Setup slash commands
            SlashCommands = Client.UseSlashCommands();
            // Register slash commands for multiple guilds
            SlashCommands.RegisterCommands<TestSlashCommands>(811338657431814174); // VB
            //SlashCommands.RegisterCommands<TestSlashCommands>(859793537620770826); // Chill

            // Connect the bot
            await Client.ConnectAsync();
            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(2)
            });
            await Task.Delay(-1);
        }

        private static Task OnClientReady(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            Console.WriteLine("Bot is ready.");
            return Task.CompletedTask;
        }
    }
}
