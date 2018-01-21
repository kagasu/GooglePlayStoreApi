using GooglePlayStore;
using GooglePlayStoreApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static async Task<ResponseWrapper> GetSearchResult(GooglePlayStoreClient client, string str)
        {
            var searchResult = await client.Search(str);
            foreach (var appDetail in searchResult.PreFetch[0].Response.Payload.ListResponse.Doc[0].Child.Select(x => x.Child[0]))
            {
                var appId = appDetail.Docid;
                var appName = appDetail.Title;

                Console.WriteLine($"{appId},{appName}");
            }

            return searchResult;
        }

        static async Task<DetailsResponse> GetAppDetail(GooglePlayStoreClient client, string appId)
        {
            var appDetail = await client.AppDetail(appId);
            var appName = appDetail.DocV2.Title;
            var descriptionHtml = appDetail.DocV2.DescriptionHtml;
            var versionCode = appDetail.DocV2.Details.AppDetails.VersionCode;
            var versionString = appDetail.DocV2.Details.AppDetails.VersionString;
            var permissions = appDetail.DocV2.Details.AppDetails.Permission;
            var offerType = appDetail.DocV2.Offer[0].OfferType;

            return appDetail;
        }

        static async Task DownloadApk(GooglePlayStoreClient client, string appId, int offerType, int versionCode)
        {
            var appDelivery = await client.AppDelivery(appId, offerType, versionCode);
            var apkDownloadUrl = appDelivery.AppDeliveryData.DownloadUrl;
            File.WriteAllBytes("Gmail.apk", await new HttpClient().GetByteArrayAsync(apkDownloadUrl));
        }

        static async Task Main(string[] args)
        {
            var accountInfo = JObject.Parse(File.ReadAllText("accountInfo.json"));

            var client = new GooglePlayStoreClient(accountInfo["email"].Value<string>(), accountInfo["password"].Value<string>(), accountInfo["android_id"].Value<string>(), new WebProxy("127.0.0.1", 8008));
            var token = await client.GetGoogleToken();
            var auth = await client.GetGoogleAuth(token);

            var searchWord = "Gmail";
            var gmailAppId = "com.google.android.gm";

            await GetSearchResult(client, searchWord);
            var appDetail = await GetAppDetail(client, gmailAppId);
            var versionCode = appDetail.DocV2.Details.AppDetails.VersionCode;
            var offerType = appDetail.DocV2.Offer[0].OfferType;

            await DownloadApk(client, gmailAppId, offerType, versionCode);
        }
    }
}
