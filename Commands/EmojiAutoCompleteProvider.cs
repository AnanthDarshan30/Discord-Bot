using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

public class EmojiAutocompleteProvider : IAutocompleteProvider
{
    public EmojiAutocompleteProvider()
    {
    }

    public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
    {
        try
        {
            if (ctx == null || ctx.Guild == null || ctx.Guild.Emojis == null)
            {
                //Console.WriteLine("Autocomplete context is not ready.");
                return Task.FromResult(Enumerable.Empty<DiscordAutoCompleteChoice>());
            }

            string userInput = ctx.OptionValue?.ToString()?.ToLower() ?? "";

            var matches = ctx.Guild.Emojis.Values
                .Where(e => !string.IsNullOrEmpty(e.Name) && e.Name.ToLower().Contains(userInput))
                .Take(25)
                .Select(e => new DiscordAutoCompleteChoice(e.Name, e.Name));

            //Console.WriteLine($"Autocomplete matches: {string.Join(", ", matches.Select(m => m.Name))}");

            return Task.FromResult(matches);
        }
        catch (Exception ex)
        {
            Console.WriteLine(" Autocomplete error: " + ex);
            return Task.FromResult(Enumerable.Empty<DiscordAutoCompleteChoice>());
        }
    }
}
