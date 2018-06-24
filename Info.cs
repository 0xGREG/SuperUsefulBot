using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SuperUsefulBot
{
    public class Info : ModuleBase
    {
        Random rand = new Random();

        [Command("gif"), Summary("Displays a meme.")]
        public async Task Gif([Summary("what gif")] string gif)
        {
            string url1 = "https://api.giphy.com/v1/gifs/search?api_key=" + Config.GiphyAPIToken + "&q=";
            string url2 = "&limit=50&offset=0&rating=G&lang=en";

            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url1 + gif + url2);

            var json = await response.Content.ReadAsStringAsync();
            JObject search = JObject.Parse(json);

            List<JToken> results = search["data"].Children().ToList();
            List<data.GiphyObjects> searchResults = new List<data.GiphyObjects>();

            foreach (JToken result in results)
            {
                // JToken.ToObject is a helper method that uses JsonSerializer internally
                data.GiphyObjects searchResult = result.ToObject<data.GiphyObjects>();
                searchResults.Add(searchResult);
            }

            if (searchResults.Count == 0)
            {
                await ReplyAsync("Not able to find any gifs with " + gif);
                return;
            }

            int imageIndex = rand.Next(searchResults.Count);

            string imageUrl = searchResults[imageIndex].images.original.url;
            
            EmbedBuilder eBuilder = new EmbedBuilder().WithImageUrl(imageUrl);
            
            await ReplyAsync("", false, eBuilder.Build());
        }
    }
}
