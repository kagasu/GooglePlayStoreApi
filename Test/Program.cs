using GooglePlayStoreApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var accountInfo = JObject.Parse(File.ReadAllText("accountInfo.json"));

            var client = new GooglePlayStoreClient(accountInfo["email"].Value<string>(), accountInfo["password"].Value<string>(), accountInfo["android_id"].Value<string>());
            var token = await client.GetGoogleToken();
            var auth = await client.GetGoogleAuth(token);

            var searchResult = await client.Search("gmail");
            foreach (var appDetail in searchResult.PreFetch[0].Response.Payload.ListResponse.Doc[0].Child.Select(x => x.Child[0]))
            {
                var appId = appDetail.Docid;
                var appName = appDetail.Title;

                Console.WriteLine($"{appId},{appName}");
            }
        }
    }
}
