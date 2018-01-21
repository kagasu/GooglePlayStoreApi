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
