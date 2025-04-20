using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulBot.Commands
{
    public class TestCommands : BaseCommandModule
    {
        [Command("show")]
        public async Task show(CommandContext context)
        {
            await context.Channel.SendMessageAsync($"Hey {context.User.Mention}, Yeah it's true I am a bot made by SOUL! Crazy, Right??");
        }

        [Command("Party")]
        public async Task party(CommandContext context)
        {
            await context.Channel.SendMessageAsync($"<a:Party:816675948250398762>");
        }
        [Command("emoji")]
        public async Task Emoji(CommandContext ctx, string formatted)
        {
            // Match standard or animated emoji
            var match = System.Text.RegularExpressions.Regex.Match(formatted, @"<(a?):(\w+):(\d+)>");

            if (!match.Success)
            {
                await ctx.RespondAsync("❌ Invalid emoji format. Use something like `<:name:id>` or `<a:name:id>`.");
                return;
            }

            string animatedFlag = match.Groups[1].Value;
            string emojiName = match.Groups[2].Value;
            string emojiId = match.Groups[3].Value;

            bool isAnimated = animatedFlag == "a";
            string emojiUrl = isAnimated
                ? $"https://cdn.discordapp.com/emojis/{emojiId}.gif"
                : $"https://cdn.discordapp.com/emojis/{emojiId}.png";

            var webhooks = await ctx.Channel.GetWebhooksAsync();
            var webhook = webhooks.FirstOrDefault(w => w.Name == "SoulBot");

            if (webhook == null)
                webhook = await ctx.Channel.CreateWebhookAsync("SoulBot");

            var embed = new DiscordEmbedBuilder()
                .WithImageUrl(emojiUrl)
                .WithColor(DiscordColor.Blurple)
                .WithFooter($"Requested by {ctx.Member.DisplayName}");

            await webhook.ExecuteAsync(new DiscordWebhookBuilder()
                .AddEmbed(embed)
                .WithUsername(ctx.Member.DisplayName)
                .WithAvatarUrl(ctx.Member.AvatarUrl));

            await ctx.Message.DeleteAsync();
        }

    }
}
