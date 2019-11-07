using GooglePlayStore;
using GooglePlayStoreApi;
using GooglePlayStoreApi.Model;
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
                var packageName = appDetail.Docid;
                var appName = appDetail.Title;

                Console.WriteLine($"{packageName},{appName}");
            }

            return searchResult;
        }

        static async Task<DetailsResponse> GetAppDetail(GooglePlayStoreClient client, string packageName)
        {
            var appDetail = await client.AppDetail(packageName);
            var appName = appDetail.DocV2.Title;
            var descriptionHtml = appDetail.DocV2.DescriptionHtml;
            var versionCode = appDetail.DocV2.Details.AppDetails.VersionCode;
            var versionString = appDetail.DocV2.Details.AppDetails.VersionString;
            var permissions = appDetail.DocV2.Details.AppDetails.Permission;
            var offerType = appDetail.DocV2.Offer[0].OfferType;

            return appDetail;
        }

        static async Task DownloadApk(GooglePlayStoreClient client, string packageName, int offerType, int versionCode)
        {
            await client.Purchase(packageName, offerType, versionCode);
            var bytes = await client.DownloadApk(packageName);
            File.WriteAllBytes("Gmail.apk", bytes);
        }

        static async Task Reviews(GooglePlayStoreClient client, string packageName)
        {
            var reviews = await client.Reviews(packageName, 20, ReviewSortType.HighRating);
            foreach (var review in reviews.GetResponse.Review)
            {
                Console.WriteLine($"{review.Author2.Name},{review.Comment}");
            }
        }

        static async Task AddReview(GooglePlayStoreClient client, string packageName)
        {
            var review = await client.AddReview(packageName, 5, "great app");
        }

        static async Task DeleteReview(GooglePlayStoreClient client, string packageName)
        {
            await client.DeleteReview(packageName);
        }

        static async Task SearchSuggest(GooglePlayStoreClient client, string str)
        {
            var suggests = await client.SearchSuggest(str);
            foreach (var x in suggests.Entry)
            {
                switch (x.Type)
                {
                    case 2:
                        Console.WriteLine(x.SuggestedQuery);
                        break;
                    case 3:
                        Console.WriteLine(x.PackageNameContainer.PackageName);
                        break;
                }
            }
        }

        static async Task TopCharts(GooglePlayStoreClient client)
        {
            var topCharts = await client.TopCharts();
            foreach (var topChart in topCharts.Select(x => x.Response.Payload.ListResponse.Doc[0].Child[0]))
            {
                var title = topChart.Title;
                Console.WriteLine(title);

                foreach (var appDetail in topChart.Child)
                {
                    var packageName = appDetail.Docid;
                    var appName = appDetail.Title;
                    
                    Console.WriteLine($"{packageName},{appName}");
                }
            }
        }

        static async Task Main(string[] args)
        {
            var accountInfo = JObject.Parse(File.ReadAllText("accountInfo.json"));

            var email = accountInfo["email"].Value<string>();
            var androidId = accountInfo["android_id"].Value<string>();
            var proxy = new WebProxy("127.0.0.1", 8008);

            var client = new GooglePlayStoreClient(email, androidId, proxy)
            {
                Country = CountryCode.Japan
            };
            // login
            var token = "oauth2_4/...";
            token = await client.GetGoogleToken(token);
            await client.GetGoogleAuth(token);
            Console.WriteLine($"auth token: {client.Auth}");

            // use auth token
            // client.Auth = "ya29....";

            var searchWord = "Gmail";
            var gmailPackageName = "com.google.android.gm";

            await SearchSuggest(client, searchWord);
            await GetSearchResult(client, searchWord);
            var appDetail = await GetAppDetail(client, gmailPackageName);
            var versionCode = appDetail.DocV2.Details.AppDetails.VersionCode;
            var offerType = appDetail.DocV2.Offer[0].OfferType;

            await DownloadApk(client, gmailPackageName, offerType, versionCode);
            await Reviews(client, gmailPackageName);
            await AddReview(client, gmailPackageName);
            await DeleteReview(client, gmailPackageName);
            await TopCharts(client);
        }
    }
}