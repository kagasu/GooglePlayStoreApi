<img src="https://i.imgur.com/I6Fcgir.png" width="96" height="96">

# GooglePlayStoreApi[![NuGet](https://img.shields.io/nuget/v/GooglePlayStoreApi.svg?style=flat-square)](https://www.nuget.org/packages/GooglePlayStoreApi) [![Codacy grade](https://img.shields.io/codacy/grade/40c88adfb64d499dbb4e2414582f1b81.svg?style=flat-square)](https://www.codacy.com/app/kagasu/GooglePlayStoreApi/dashboard)

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
# Related projects
- https://github.com/microg/android_packages_apps_GmsCore
- https://github.com/NoMore201/googleplay-api
- https://github.com/MCMrARM/Google-Play-API
- https://github.com/yeriomin/play-store-api
- https://github.com/dweinstein/node-google-play
