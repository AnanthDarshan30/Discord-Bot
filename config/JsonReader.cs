using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SoulBot.config 
{
    public class JsonReader
    {
        public string token { get; set; }  
        public string prefix { get; set; }
        public static string BotToken { get; private set; }
        public async Task ReadJson()
        {
            using (StreamReader sr = new StreamReader("config.json"))
            {
                string json = await sr.ReadToEndAsync();
                JsonStructure data = JsonConvert.DeserializeObject<JsonStructure>(json);

                this.token = data.token;
                this.prefix = data.prefix;

                BotToken = data.token;
            }
        }
    }

    internal sealed class JsonStructure
    {
        public string token { get; set; }
        public string prefix { get; set; }
    }
}
