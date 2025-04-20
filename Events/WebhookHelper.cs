using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SoulBot.Events
{
    public class WebhookHelper
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task SendWebhookReplyAsync(
            DiscordWebhook webhook,
            string content,
            string username,
            string avatarUrl,
            IEnumerable<object> embeds = null,
            ulong? replyToMessageId = null)
        {
            var payload = new
            {
                content = content,
                username = username,
                avatar_url = avatarUrl,
                embeds = embeds,
                allowed_mentions = new { parse = new string[] { } },
                message_reference = replyToMessageId.HasValue
                    ? new { message_id = replyToMessageId.Value.ToString() }
                    : null
            };

            var json = JsonConvert.SerializeObject(payload);

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://discord.com/api/webhooks/{webhook.Id}/{webhook.Token}")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            try
            {
                var response = await httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Webhook failed: {response.StatusCode} - {error}");
                }
                else
                {
                    Console.WriteLine("Webhook sent successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while sending webhook: {ex.Message}");
            }
        }
    }
}
