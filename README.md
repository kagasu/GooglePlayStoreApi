# GooglePlayStoreApi

# Usage
### Search
```cs
var email = "abc@gmail.com";
var password = "mypassword";
var androidId = Guid.NewGuid().ToString("N").Substring(0, 16);

var client = new GooglePlayStoreClient(email, password, androidId);
var token = await client.GetGoogleToken();
var auth = await client.GetGoogleAuth(token);

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
var password = "mypassword";
var androidId = Guid.NewGuid().ToString("N").Substring(0, 16);

var client = new GooglePlayStoreClient(email, password, androidId);
var token = await client.GetGoogleToken();
var auth = await client.GetGoogleAuth(token);

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
var password = "mypassword";
var androidId = Guid.NewGuid().ToString("N").Substring(0, 16);

var client = new GooglePlayStoreClient(email, password, androidId);
var token = await client.GetGoogleToken();
var auth = await client.GetGoogleAuth(token);

var bytes = await client.DownloadApk("com.google.android.gm");
File.WriteAllBytes("Gmail.apk", bytes);
```
