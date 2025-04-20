using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands.Attributes;
using System.Linq;
using System.Threading.Tasks;
using System;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Globalization;
using System.IO;

namespace SoulBot.Commands
{
    public class TestSlashCommands : ApplicationCommandModule
    {
        [SlashCommand("emoji", "Sends a server emoji")]
        public async Task EmojiCommand(
            InteractionContext ctx,
            [Option("emoji", "The name of the emoji")]
        [Autocomplete(typeof(EmojiAutocompleteProvider))] string emojiName)
        {
            //Console.WriteLine("Entered test slashcommands");
            var emoji = ctx.Guild.Emojis.Values.FirstOrDefault(e => e.Name.Equals(emojiName, StringComparison.OrdinalIgnoreCase));

            if (emoji != null)
            {
                string emojiText = emoji.IsAnimated
                    ? $"<a:{emoji.Name}:{emoji.Id}>"
                    : $"<:{emoji.Name}:{emoji.Id}>";

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent(emojiText));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Emoji not found :("));
            }
        }

        [SlashCommand("userban", "Fake ban someone for trolling purposes.")]
        public async Task FakeBanCommand(InteractionContext ctx,
            [Option("user", "User to fake ban")] DiscordUser user,
            [Option("reason", "Reason for fake ban")] string reason = "Being sus")
        {
            ulong requiredRoleId = 1359228488015417445;
            ulong soulId = 474200668601843719;

            bool hasRequiredRole = ctx.Member.Roles.Any(role => role.Id == requiredRoleId);
            bool isAdmin = ctx.Member.Permissions.HasPermission(Permissions.Administrator);

            if (!hasRequiredRole && !isAdmin)
            {
                var responseBuilder = new DiscordInteractionResponseBuilder()
                    .WithContent($"❗You must be a SoulBot User. React <:Alex:885210623368130622> to request access or <:kannafacepalm:900473650455642113> to cancel.");

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder);

                var msg = await ctx.GetOriginalResponseAsync();

                var yesEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 885210623368130622);
                var noEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 900473650455642113);

                await msg.CreateReactionAsync(yesEmoji);
                await msg.CreateReactionAsync(noEmoji);

                var interactivity = ctx.Client.GetInteractivity();
                var reactionResult = await interactivity.WaitForReactionAsync(
                    msg,
                    ctx.User,
                    TimeSpan.FromSeconds(30)
                );

                if (reactionResult.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync("No response received. Request timed out.");
                    return;
                }

                if (reactionResult.Result.Emoji == yesEmoji)
                {
                    await ctx.Channel.SendMessageAsync($"<@{soulId}>, {ctx.User.Mention} is requesting the SoulBot User role.");
                }
                else
                {
                    await ctx.Channel.SendMessageAsync(" Request cancelled.");
                }

                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = $"<a:b0nkban:920768677761146981> Banned User",
                Description = $"**Reason:** {reason}\n\n**Action Taken:** <:angelpew:855708979308396554> User has been banned from the server.",
                Color = DiscordColor.Red,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = user.AvatarUrl },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "Please don't behave like this in other servers!",
                    IconUrl = "https://cdn.discordapp.com/emojis/900653261646868480.png"
                }
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .WithContent($"{user.Mention}")
                    .AddEmbed(embed));
        }
        [SlashCommand("roleinfo", "Dump all metadata of a role")]
        public async Task GetRoleInfo(InteractionContext ctx,
            [Option("role", "The role to inspect")] DiscordRole role)
        {
            if (ctx.User.Id != 474200668601843719)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent(" Sike You thought!")
                        .AsEphemeral(true));
                return;
            }

            var json = System.Text.Json.JsonSerializer.Serialize(role, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            });

            if (json.Length > 1900)
                json = json.Substring(0, 1900);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .WithContent($"```json\n{json}\n```")
                    .AsEphemeral(true));
        }

        [SlashCommand("testroleunicode", "Creates a test role with unicode characters to debug role issues.")]
        public async Task TestRoleUnicodeCommand(InteractionContext ctx)
        {
            // Only allow you (replace with your user ID)
            if (ctx.User.Id != 474200668601843719)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("❌ You aren't allowed to run this command."));
                return;
            }

            try
            {
                // Zero-width space, word joiner, zero-width joiner
                string weirdUnicode = "\u200B\u2060\u200D";
                string roleName = $"Invisible{weirdUnicode}Name🧠";

                var role = await ctx.Guild.CreateRoleAsync(
                    name: roleName,
                    color: DiscordColor.Grayple,
                    permissions: Permissions.None,
                    hoist: true,
                    mentionable: true,
                    reason: "Testing unicode role bug"
                );

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"✅ Created test role: `{role.Name}` (ID: {role.Id})"));
            }
            catch (Exception ex)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent($"❌ Error: {ex.Message}"));
            }
        }
        [SlashCommand("setrole", "Assigns a role to a user.")]
        public async Task SetRoleCommandAsync(
        InteractionContext ctx,
        [Option("role", "The role to assign")] DiscordRole role,
        [Option("user", "The user to assign the role to")] DiscordUser user)
        {
            if (ctx.User.Id != 474200668601843719)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("You aren't allowed to run this command. Oops"));
                return;
            }
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var member = await ctx.Guild.GetMemberAsync(user.Id);

            try
            {
                await member.GrantRoleAsync(role);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($" {role.Mention} has been assigned to {member.DisplayName}."));
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($" Failed to assign role: {ex.Message}"));
            }
        }

        [SlashCommand("createrolebroken", "Creates a role with a broken emoji (zero-byte png)")]
        public async Task CreateBrokenEmojiRole(InteractionContext ctx,
            [Option("name", "Name of the role")] string roleName)
        {
            if (ctx.User.Id != 474200668601843719)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("You aren't allowed to run this command. Oops"));
                return;
            }

            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            try
            {
                // Minimal 1x1 transparent PNG in Base64
                byte[] transparentPng = Convert.FromBase64String(
                    "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO50n+IAAAAASUVORK5CYII="
                );

                DiscordRole role;

                using (var stream = new MemoryStream(transparentPng))
                {
                    role = await ctx.Guild.CreateRoleAsync(
                        name: roleName,
                        permissions: Permissions.None,
                        color: DiscordColor.Gray,
                        icon: stream, 
                        hoist: false,
                        mentionable: false,
                        reason: "Testing role with broken emoji"
                    );
                }

                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"✅ Created role `{roleName}` with broken emoji."));
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"❌ Failed to create role: {ex.Message}"));
            }
        }



    }

}
