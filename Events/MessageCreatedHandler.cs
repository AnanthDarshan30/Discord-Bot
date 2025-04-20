using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;

namespace SoulBot.Events
{
    public class MessageCreatedHandler
    {
        public static async Task OnMessageCreated(DiscordClient client, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot) return;

            const string trigger = "sool ";
            if (!e.Message.Content.StartsWith(trigger)) return;

            var originalMessage = e.Message;
            var channel = originalMessage.Channel;
            var messageContent = originalMessage.Content.Substring(trigger.Length);

            // Block @everyone and @here
            if (messageContent.Contains("@everyone") || messageContent.Contains("@here"))
            {
                await originalMessage.RespondAsync(" You cannot mention \\@everyone or \\@here.");
                return;
            }

            // Replace :emoji or ?emoji with global custom emojis
            var allGuilds = client.Guilds.Values;
            var uniqueEmojis = allGuilds
                .SelectMany(g => g.Emojis.Values)
                .Where(emoji => !string.IsNullOrEmpty(emoji.Name))
                .GroupBy(emoji => emoji.Name.ToLower())
                .Select(group => group.OrderByDescending(emoji => emoji.IsAnimated).First())
                .ToDictionary(emoji => emoji.Name.ToLower(), emoji => emoji);

            messageContent = Regex.Replace(
                messageContent,
                @"[?:](\w+)\b",
                match =>
                {
                    var name = match.Groups[1].Value.ToLower();
                    return uniqueEmojis.TryGetValue(name, out var emoji)
                        ? (emoji.IsAnimated ? $"<a:{emoji.Name}:{emoji.Id}>" : $"<:{emoji.Name}:{emoji.Id}>")
                        : match.Value;
                },
                RegexOptions.IgnoreCase
            );

            await originalMessage.DeleteAsync();

            // Get display name and avatar
            var member = await e.Guild.GetMemberAsync(originalMessage.Author.Id);
            var displayName = member?.DisplayName ?? originalMessage.Author.Username;
            var avatarUrl = member?.AvatarUrl ?? originalMessage.Author.AvatarUrl;

            // Handle threads
            var webhookChannel = channel.IsThread ? channel.Parent : channel;
            var webhooks = await webhookChannel.GetWebhooksAsync();
            var botWebhook = webhooks.FirstOrDefault(w => w.Name == "SoulBot-Impersonator")
                             ?? await webhookChannel.CreateWebhookAsync("SoulBot-Impersonator");

            // Build the webhook content using DiscordWebhookBuilder
            var builder = new DiscordWebhookBuilder()
                .WithUsername(displayName)
                .WithAvatarUrl(avatarUrl)
                .WithContent(messageContent);

            if (channel.IsThread)
                builder.WithThreadId(channel.Id);

            // Embed replied message if any
            var refMsg = originalMessage.ReferencedMessage;
            if (refMsg != null)
            {
                try
                {
                    string refDisplayName;
                    string refAvatar;

                    DiscordMember refMember = null;

                    try
                    {
                        refMember = await e.Guild.GetMemberAsync(refMsg.Author.Id);
                    }
                    catch
                    {
                        // Member not found or left server
                    }

                    if (refMember != null)
                    {
                        refDisplayName = refMember.DisplayName;
                        refAvatar = refMember.AvatarUrl;
                    }
                    else if (refMsg.Embeds.Count > 0 && refMsg.Embeds.Last().Author != null)
                    {
                        var embedAuthor = refMsg.Embeds.Last().Author;
                        refDisplayName = embedAuthor.Name;
                        refAvatar = embedAuthor.IconUrl?.ToString() ?? refMsg.Author.AvatarUrl;
                    }
                    else
                    {
                        refDisplayName = refMsg.Author.Username;
                        refAvatar = refMsg.Author.AvatarUrl;
                    }

                    var embed = new DiscordEmbedBuilder()
                        .WithColor(DiscordColor.Purple)
                        .WithAuthor(refDisplayName, null, refAvatar)
                        .WithDescription(refMsg.Content)
                        .AddField("\u200B", $"<@{refMsg.Author.Id}>", false); // stealth ping

                    builder.AddEmbed(embed);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" Failed to embed quoted message: {ex.Message}");
                }
            }

            // Send the message via the webhook
            await botWebhook.ExecuteAsync(builder);
        }
    }
}
