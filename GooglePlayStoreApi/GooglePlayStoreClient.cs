using Google.Protobuf;
using GooglePlayStore;
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
        
        private async Task<T> Get<T>(string url, MessageParser<T> parser) where T : IMessage<T>
        {
            var bytes = await client.GetByteArrayAsync(url);
            return parser.ParseFrom(bytes);
        }

        public async Task<string> GetGoogleToken()
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "androidId", AndroidId },
                { "lang", "ja_JP" },
                { "google_play_services_version", GOOGLE_PLAY_SERVICE_VERSION },
                { "sdk_version", "19" },
                { "device_country", "jp" },
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
                { "lang", "ja_JP" },
                { "google_play_services_version", GOOGLE_PLAY_SERVICE_VERSION },
                { "sdk_version", "19" },
                { "device_country", "jp" },
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

        public async Task<ResponseWrapper> Search(string str)
        {
            HeaderSet("X-DFE-Device-Id", AndroidId);
            HeaderSet("Accept-Language", "ja-JP");
            HeaderSet("Authorization", $"GoogleLogin auth={Auth}");
            return await Get($"{API_ENDPOINT}/fdfe/search?c=3&q={Uri.EscapeUriString(str)}", ResponseWrapper.Parser);
        }
    }
}
