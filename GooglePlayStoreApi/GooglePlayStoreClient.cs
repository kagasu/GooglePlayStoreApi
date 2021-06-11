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
        private const string DefaultUserAgent = "Android-Finsky/25.7.22-21%20%5B0%5D%20%5BPR%5D%20378204440 (api=3,versionCode=82572210,sdk=22,device=d2q,hardware=qcom,product=d2que,platformVersionRelease=5.1.1,model=SM-N976N,buildId=QP1A.190711.020,isWideScreen=1,supportedAbis=x86;armeabi-v7a;armeabi)";
        private const string ApiEndpoint = "https://android.clients.google.com";

        private static HttpClient client = default!;

        public CountryCode Country { get; set; } = CountryCode.Japan;
        public string AndroidId { get; private set; } = default!;
        public string GoogleEmailAddress { get; private set; } = default!;
        public string Auth { get; private set; } = default!;
        public string GooglePlayServiceVersion { get; set; } = "19530037";

        public GooglePlayStoreClient(string googleEmailAddress, string androidId, string? userAgent = null, IWebProxy? proxy = null)
        {
            GoogleEmailAddress = googleEmailAddress;
            AndroidId = androidId;

            var handler = new HttpClientHandler();

            if (proxy != null)
            {
                handler.UseProxy = true;
                handler.Proxy = proxy;
            }

            client = new HttpClient(handler);
            HeaderSet("User-Agent", userAgent ?? DefaultUserAgent);
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
            var bytes = await client.GetByteArrayAsync(url).ConfigureAwait(false);
            return ResponseWrapper.Parser.ParseFrom(bytes);
        }

        private async Task<ResponseWrapper> Post(string url, HttpContent content)
        {
            var response = await client.PostAsync(url, content).ConfigureAwait(false);
            var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            return ResponseWrapper.Parser.ParseFrom(bytes);
        }

        public async Task<string> GetGoogleToken(string token)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "androidId", AndroidId },
                { "lang", Country.GetCountryCode() },
                { "google_play_services_version", GooglePlayServiceVersion },
                { "sdk_version", "19" },
                { "device_country", Country.GetCountry() },
                { "callerSig", "38918a453d07199354f8b19af05ec6562ced5788" },
                { "client_sig", "38918a453d07199354f8b19af05ec6562ced5788" },
                { "token_request_options", "CAA4AQ==" },
                { "Email", GoogleEmailAddress },
                { "droidguardPeriodicUpdate", "1" },
                { "service", "ac2dm" },
                { "system_partition", "1" },
                { "check_email", "1" },
                { "callerPkg", "com.google.android.gms" },
                { "get_accountid", "1" },
                { "ACCESS_TOKEN", "1" },
                { "add_account", "1" },
                { "Token", token }
            });

            var response = await client.PostAsync($"{ApiEndpoint}/auth", content).ConfigureAwait(false);
            var str = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new ForbiddenException(str);
            }

            var parameters = str.Split('\n').ToDictionary(x => x.Split('=')[0], x => x.Split('=')[1]);
            return parameters["Token"];
        }

        public async Task GetGoogleAuth(string token)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "androidId", AndroidId },
                { "lang", Country.GetCountryCode() },
                { "google_play_services_version", GooglePlayServiceVersion },
                { "sdk_version", "19" },
                { "device_country", Country.GetCountry() },
                { "callerSig", "38918a453d07199354f8b19af05ec6562ced5788" },
                { "client_sig", "38918a453d07199354f8b19af05ec6562ced5788" },
                { "token_request_options", "CAA4AVAB" },
                { "Email", GoogleEmailAddress },
                { "service", "oauth2:https://www.googleapis.com/auth/googleplay" },
                { "system_partition", "1" },
                { "check_email", "1" },
                { "callerPkg", "com.google.android.gms" },
                { "Token", token },
                { "oauth2_foreground", "1" },
                { "app", "com.android.vending" },
                { "_opt_is_called_from_account_manager", "1" },
                { "is_called_from_account_manager", "1" }
            });

            var response = await client.PostAsync($"{ApiEndpoint}/auth", content).ConfigureAwait(false);
            var str = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var parameters = str.Split('\n').ToDictionary(x => x.Split('=')[0], x => x.Split('=')[1]);
            Auth = parameters["Auth"];
        }

        public async Task<SearchSuggestResponse> SearchSuggest(string str, bool suggestString = true, bool suggestApp = true)
        {
            HeaderSet("X-DFE-Device-Id", AndroidId);
            HeaderSet("Accept-Language", Country.GetCountryCode());
            HeaderSet("Authorization", $"OAuth {Auth}");

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

            var response = await Get($"{ApiEndpoint}/fdfe/searchSuggest?{string.Join("&", parameters.Select(x => $"{x.Key}={x.Value}"))}").ConfigureAwait(false);

            return response.Payload.SearchSuggestResponse;
        }

        public async Task<ResponseWrapper> Search(string str)
        {
            HeaderSet("X-DFE-Device-Id", AndroidId);
            HeaderSet("Accept-Language", Country.GetCountryCode());
            HeaderSet("Authorization", $"OAuth {Auth}");
            return await Get($"{ApiEndpoint}/fdfe/search?c=3&q={Uri.EscapeUriString(str)}").ConfigureAwait(false);
        }
        
        public async Task<DetailsResponse> AppDetail(string packageName)
        {
            HeaderSet("X-DFE-Device-Id", AndroidId);
            HeaderSet("Accept-Language", Country.GetCountryCode());
            HeaderSet("Authorization", $"OAuth {Auth}");
            var response =  await Get($"{ApiEndpoint}/fdfe/details?doc={Uri.EscapeUriString(packageName)}").ConfigureAwait(false);
            
            return response.Payload.DetailsResponse;
        }

        public async Task<DeliveryResponse> AppDelivery(string packageName, int offerType, int versionCode)
        {
            HeaderSet("X-DFE-Device-Id", AndroidId);
            HeaderSet("Accept-Language", Country.GetCountryCode());
            HeaderSet("Authorization", $"OAuth {Auth}");
            var response = await Get($"{ApiEndpoint}/fdfe/delivery?doc={Uri.EscapeUriString(packageName)}&ot={offerType}&vc={versionCode}").ConfigureAwait(false);

            return response.Payload.DeliveryResponse;
        }

        public async Task<byte[]> DownloadApk(string packageName)
        {
            HeaderSet("X-DFE-Device-Id", AndroidId);
            HeaderSet("Accept-Language", Country.GetCountryCode());
            HeaderSet("Authorization", $"OAuth {Auth}");

            var appDetail = await AppDetail(packageName).ConfigureAwait(false);
            var offerType = appDetail.Item.Offer[0].OfferType;
            var versionCode = appDetail.Item.Details.AppDetails.VersionCode;

            var appDelivery = await AppDelivery(packageName, offerType, versionCode).ConfigureAwait(false);
            var apkDownloadUrl = appDelivery.AppDeliveryData.DownloadUrl;
            return await client.GetByteArrayAsync(apkDownloadUrl).ConfigureAwait(false);
        }

        public async Task<ReviewResponse> AddReview(string packageName, int rating, string comment)
        {
            HeaderSet("X-DFE-Device-Id", AndroidId);
            HeaderSet("Accept-Language", Country.GetCountryCode());
            HeaderSet("Authorization", $"OAuth {Auth}");

            var content = new ByteArrayContent(new byte[] { });
            var response = await Post($"{ApiEndpoint}/fdfe/addReview?doc={packageName}&title=&content={comment}&rating={rating}&ipr=true&itpr=false", content).ConfigureAwait(false);
            
            return response.Payload.ReviewResponse;
        }

        public async Task DeleteReview(string packageName)
        {
            HeaderSet("X-DFE-Device-Id", AndroidId);
            HeaderSet("Accept-Language", Country.GetCountryCode());
            HeaderSet("Authorization", $"OAuth {Auth}");

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "itpr", "false" },
                { "doc", packageName }
            });

            await Post($"{ApiEndpoint}/fdfe/deleteReview", content).ConfigureAwait(false);
        }

        public async Task<RepeatedField<PreFetch>> TopCharts()
        {
            HeaderSet("X-DFE-Device-Id", AndroidId);
            HeaderSet("Accept-Language", Country.GetCountryCode());
            HeaderSet("Authorization", $"OAuth {Auth}");
            
            var response = await Get($"{ApiEndpoint}/fdfe/topCharts?c=3").ConfigureAwait(false);
            return response.PreFetch;
        }
        
        public async Task<ReviewResponse> Reviews(string packageName, int numberOfResults, ReviewSortType sortType, int offset = 0)
        {
            HeaderSet("X-DFE-Device-Id", AndroidId);
            HeaderSet("Accept-Language", Country.GetCountryCode());
            HeaderSet("Authorization", $"OAuth {Auth}");

            var response = await Get($"{ApiEndpoint}/fdfe/rev?doc={packageName}&n={numberOfResults}&o={offset}&sort={(int)sortType}").ConfigureAwait(false);
            return response.Payload.ReviewResponse;
        }

        public async Task<BuyResponse> Purchase(string packageName, int offerType, int versionCode)
        {
            HeaderSet("X-DFE-Device-Id", AndroidId);
            HeaderSet("Accept-Language", Country.GetCountryCode());
            HeaderSet("Authorization", $"OAuth {Auth}");

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "doc", packageName },
                { "ot", offerType.ToString() },
                { "vc", versionCode.ToString() }
            });

            var response = await Post($"{ApiEndpoint}/fdfe/purchase", content).ConfigureAwait(false);
            return response.Payload.BuyResponse;
        }
    }
}
