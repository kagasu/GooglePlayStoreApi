<img src="https://i.imgur.com/I6Fcgir.png" width="96" height="96">

# GooglePlayStoreApi[![NuGet](https://img.shields.io/nuget/v/GooglePlayStoreApi.svg?style=flat-square)](https://www.nuget.org/packages/GooglePlayStoreApi)

# Install
Install as [NuGet package](https://www.nuget.org/packages/GooglePlayStoreApi/)
```powershell
Install-Package GooglePlayStoreApi
```

.NET CLI
```shell
dotnet add package GooglePlayStoreApi
```


# Usage
Get `auth_token` from this [URL](https://accounts.google.com/EmbeddedSetup)
![](https://i.imgur.com/80MLpoR.png)

### Search
```cs
var email = "abc@gmail.com";
var androidId = ""; // use your real GSF ID(Google Service Framework ID)
var token = "oauth2_4/...";

var client = new GooglePlayStoreClient(email, androidId);
token = await client.GetGoogleToken(token);
await client.GetGoogleAuth(token);

var searchResult = await client.Search("gmail");
foreach (var appDetail in searchResult.PreFetch[0].Response.Payload.ListResponse.Doc[0].Child.Select(x => x.Child[0]))
{
  var appId = appDetail.Docid;
  var appName = appDetail.Title;

  Console.WriteLine($"{appId},{appName}");
}
```

### AppDetail
```cs
var email = "abc@gmail.com";
var androidId = ""; // use your real GSF ID(Google Service Framework ID)
var token = "oauth2_4/...";

var client = new GooglePlayStoreClient(email, androidId);
token = await client.GetGoogleToken(token);
await client.GetGoogleAuth(token);

var appDetail = await client.AppDetail("com.google.android.gm");
var appName = appDetail.DocV2.Title;
var descriptionHtml = appDetail.DocV2.DescriptionHtml;
var versionCode = appDetail.DocV2.Details.AppDetails.VersionCode;
var versionString = appDetail.DocV2.Details.AppDetails.VersionString;
var permissions = appDetail.DocV2.Details.AppDetails.Permission;
var offerType = appDetail.DocV2.Offer[0].OfferType;
```

### Download APK
```cs
var email = "abc@gmail.com";
var androidId = ""; // use your real GSF ID(Google Service Framework ID)
var token = "oauth2_4/...";
var gmailPackageName = "com.google.android.gm";

var client = new GooglePlayStoreClient(email, androidId);
token = await client.GetGoogleToken(token);
await client.GetGoogleAuth(token);

var appDetail = await client.AppDetail(gmailPackageName);
var offerType = appDetail.DocV2.Offer[0].OfferType;
var versionCode = appDetail.DocV2.Details.AppDetails.VersionCode;

await client.Purchase(gmailPackageName, offerType, versionCode);
var bytes = await client.DownloadApk(gmailPackageName);
File.WriteAllBytes("Gmail.apk", bytes);
```

### First time login
```cs
var token = "oauth2_4/...";
token = await client.GetGoogleToken(token);
// save token to "token.txt"
File.WriteAllText("token.txt", token);
await client.GetGoogleAuth(token);
```

### After the second time login
```cs
// load token from "token.txt"
var token = File.ReadAllText("token.txt");
await client.GetGoogleAuth(token);
```

# Related projects
- https://github.com/microg/android_packages_apps_GmsCore
- https://github.com/NoMore201/googleplay-api
- https://github.com/MCMrARM/Google-Play-API
- https://github.com/yeriomin/play-store-api
- https://github.com/dweinstein/node-google-play
- https://github.com/Ksauder/googleplay-api
- https://github.com/yeriomin/token-dispenser
- https://github.com/onyxbits/raccoon4
- https://github.com/kiwiz/gkeepapi
- https://github.com/onyxbits/dummydroid
- https://github.com/vemacs/GPSOAuthSharp
- https://github.com/ClaudiuGeorgiu/PlaystoreDownloader
