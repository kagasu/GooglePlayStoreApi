using Google.Protobuf.Collections;
using GooglePlayStore;
using GooglePlayStoreApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace GooglePlayStoreApi
{
    public class GooglePlayStoreClient
    {
        private const string GOOGLE_PLAY_SERVICE_VERSION = "11951038";
        private const string API_ENDPOINT = "https://android.clients.google.com";
        private static HttpClient client;
        
        public CountryCode Country { get; set; } = CountryCode.Japan;

        public string AndroidId { get; set; }
        public string GoogleEmailAddress { get; set; }
        public string GooglePassword { get; set; }
        public string Token { get; set; }
        public string Auth { get; set; }

        public GooglePlayStoreClient(string googleEmailAddress, string googlePassword, string androidId, IWebProxy proxy = null)
        {
            GoogleEmailAddress = googleEmailAddress;
            GooglePassword = googlePassword;
            AndroidId = androidId;

            var handler = new HttpClientHandler();

            if (proxy != null)
            {
                handler.UseProxy = true;
                handler.Proxy = proxy;
            }

            client = new HttpClient(handler);
            HeaderSet("User-Agent", "Android-Finsky/8.5.39.W-all%20%5B0%5D%20%5BPR%5D%20178322352 (api=3,versionCode=80853900,sdk=19,device=bacon,hardware=bacon,product=aokp_bacon,platformVersionRelease=4.4.4,model=One,buildId=KTU84Q,isWideScreen=0,supportedAbis=armeabi-v7a;armeabi)");
            client.DefaultRequestHeaders.TryAddWithoutValidation("device", androidId);
        }

        private void HeaderSet(string name, string value)
        {
            if (client.DefaultRequestHeaders.Contains(name))
            {
                client.DefaultRequestHeaders.Remove(name);
            }

            client.DefaultRequestHeaders.TryAddWithoutValidation(name, value);
        }
        
        private async Task<ResponseWrapper> Get(string url)
        {
            var bytes = await client.GetByteArrayAsync(url);
            return ResponseWrapper.Parser.ParseFrom(bytes);
        }

        private async Task<ResponseWrapper> Post(string url, HttpContent content)
        {
            var response = await client.PostAsync(url, content);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            return ResponseWrapper.Parser.ParseFrom(bytes);
        }

        public async Task<string> GetGoogleToken()
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "androidId", AndroidId },
                { "lang", Country.GetCountryCode() },
                { "google_play_services_version", GOOGLE_PLAY_SERVICE_VERSION },
                { "sdk_version", "19" },
                { "device_country", Country.GetCountry() },
                { "callerSig", "38918a453d07199354f8b19af05ec6562ced5788" },
                { "Email", GoogleEmailAddress },
                { "get_accountid", "1" },
                { "add_account", "1" },
                { "service", "ac2dm" },
                { "callerPkg", "com.android.settings" },
                { "EncryptedPasswd", PasswordCryptor.EncryptPassword(GoogleEmailAddress, GooglePassword) },
            });

            var response = await client.PostAsync($"{API_ENDPOINT}/auth", content);
            var str = await response.Content.ReadAsStringAsync();
            var parameters = str.Split('\n').ToDictionary(x => x.Split('=')[0], x => x.Split('=')[1]);
            Token = parameters["Token"];
            return Token;
        }

        public async Task<string> GetGoogleAuth(string token)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "androidId", AndroidId },
                { "lang", Country.GetCountryCode() },
                { "google_play_services_version", GOOGLE_PLAY_SERVICE_VERSION },
                { "sdk_version", "19" },
                { "device_country", Country.GetCountry() },
                { "app", "com.android.vending" },
                { "callerSig", "38918a453d07199354f8b19af05ec6562ced5788" },
                { "client_sig", "38918a453d07199354f8b19af05ec6562ced5788" },
                { "token_request_options", "CAA4AQ==" },
                { "Email", GoogleEmailAddress },
                { "droidguardPeriodicUpdate", "1" },
                { "service", "androidmarket" },
                { "system_partition", "1" },
                { "check_email", "1" },
                { "callerPkg", "com.google.android.gsf.login" },
                { "Token", token }
            });

            var response = await client.PostAsync($"{API_ENDPOINT}/auth", content);
            var str = await response.Content.ReadAsStringAsync();
            var parameters = str.Split('\n').ToDictionary(x => x.Split('=')[0], x => x.Split('=')[1]);
            Auth = parameters["Auth"];
            return Auth;
        }

        public async Task<SearchSuggestResponse> SearchSuggest(string str, bool suggestString = true, bool suggestApp = true)
        {
            HeaderSet("X-DFE-Device-Id", AndroidId);
            HeaderSet("Accept-Language", Country.GetCountryCode());
            HeaderSet("Authorization", $"GoogleLogin auth={Auth}");

            var parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("q", Uri.EscapeUriString(str)));
            parameters.Add(new KeyValuePair<string, string>("c", "3"));
            parameters.Add(new KeyValuePair<string, string>("ssis", "120"));

            if (suggestString)
            {
                parameters.Add(new KeyValuePair<string, string>("sst", "2"));
            }
            if (suggestApp)
            {
                parameters.Add(new KeyValuePair<string, string>("sst", "3"));
            }

            var response = await Get($"{API_ENDPOINT}/fdfe/searchSuggest?{string.Join("&", parameters.Select(x => $"{x.Key}={x.Value}"))}");

            return response.Payload.SearchSuggestResponse;
        }

        public async Task<ResponseWrapper> Search(string str)
        {
            HeaderSet("X-DFE-Device-Id", AndroidId);
            HeaderSet("Accept-Language", Country.GetCountryCode());
            HeaderSet("Authorization", $"GoogleLogin auth={Auth}");
            return await Get($"{API_ENDPOINT}/fdfe/search?c=3&q={Uri.EscapeUriString(str)}");
        }
        
        public async Task<DetailsResponse> AppDetail(string packageName)
        {
            HeaderSet("X-DFE-Device-Id", AndroidId);
            HeaderSet("Accept-Language", Country.GetCountryCode());
            HeaderSet("Authorization", $"GoogleLogin auth={Auth}");
            var response =  await Get($"{API_ENDPOINT}/fdfe/details?doc={Uri.EscapeUriString(packageName)}");
            
            return response.Payload.DetailsResponse;
        }

        public async Task<DeliveryResponse> AppDelivery(string packageName, int offerType, int versionCode)
        {
            HeaderSet("X-DFE-Device-Id", AndroidId);
            HeaderSet("Accept-Language", Country.GetCountryCode());
            HeaderSet("Authorization", $"GoogleLogin auth={Auth}");
            var response = await Get($"{API_ENDPOINT}/fdfe/delivery?doc={Uri.EscapeUriString(packageName)}&ot={offerType}&vc={versionCode}");

            return response.Payload.DeliveryResponse;
        }

        public async Task<byte[]> DownloadApk(string packageName)
        {
            HeaderSet("X-DFE-Device-Id", AndroidId);
            HeaderSet("Accept-Language", Country.GetCountryCode());
            HeaderSet("Authorization", $"GoogleLogin auth={Auth}");

            var appDetail = await AppDetail(packageName);
            var offerType = appDetail.DocV2.Offer[0].OfferType;
            var versionCode = appDetail.DocV2.Details.AppDetails.VersionCode;

            var appDelivery = await AppDelivery(packageName, offerType, versionCode);
            var apkDownloadUrl = appDelivery.AppDeliveryData.DownloadUrl;

            return await client.GetByteArrayAsync(apkDownloadUrl);
        }

        public async Task<ReviewResponse> AddReview(string packageName, int rating, string comment)
        {
            HeaderSet("X-DFE-Device-Id", AndroidId);
            HeaderSet("Accept-Language", Country.GetCountryCode());
            HeaderSet("Authorization", $"GoogleLogin auth={Auth}");

            var content = new ByteArrayContent(new byte[] { });
            var response = await Post($"{API_ENDPOINT}/fdfe/addReview?doc={packageName}&title=&content={comment}&rating={rating}&ipr=true&itpr=false", content);
            
            return response.Payload.ReviewResponse;
        }

        public async Task DeleteReview(string packageName)
        {
            HeaderSet("X-DFE-Device-Id", AndroidId);
            HeaderSet("Accept-Language", Country.GetCountryCode());
            HeaderSet("Authorization", $"GoogleLogin auth={Auth}");

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "itpr", "false" },
                { "doc", packageName }
            });

            await Post($"{API_ENDPOINT}/fdfe/deleteReview", content);
        }

        public async Task<RepeatedField<PreFetch>> TopCharts()
        {
            HeaderSet("X-DFE-Device-Id", AndroidId);
            HeaderSet("Accept-Language", Country.GetCountryCode());
            HeaderSet("Authorization", $"GoogleLogin auth={Auth}");
            
            var response = await Get($"{API_ENDPOINT}/fdfe/topCharts?c=3");
            return response.PreFetch;
        }
        
        public async Task<ReviewResponse> Reviews(string packageName, int numberOfResults, ReviewSortType sortType, int offset = 0)
        {
            HeaderSet("X-DFE-Device-Id", AndroidId);
            HeaderSet("Accept-Language", Country.GetCountryCode());
            HeaderSet("Authorization", $"GoogleLogin auth={Auth}");

            var response = await Get($"{API_ENDPOINT}/fdfe/rev?doc={packageName}&n={numberOfResults}&o={offset}&sort={(int)sortType}");
            return response.Payload.ReviewResponse;
        }

        public async Task<BuyResponse> Purchase(string packageName, int offerType, int versionCode)
        {
            HeaderSet("X-DFE-Device-Id", AndroidId);
            HeaderSet("Accept-Language", Country.GetCountryCode());
            HeaderSet("Authorization", $"GoogleLogin auth={Auth}");

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "doc", packageName },
                { "ot", offerType.ToString() },
                { "vc", versionCode.ToString() }
            });

            var response = await Post($"{API_ENDPOINT}/fdfe/purchase", content);
            return response.Payload.BuyResponse;
        }
    }
}
