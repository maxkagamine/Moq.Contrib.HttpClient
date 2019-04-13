# Moq.Contrib.HttpClient

[![NuGet](https://img.shields.io/nuget/v/Moq.Contrib.HttpClient.svg)](https://www.nuget.org/packages/Moq.Contrib.HttpClient/) [![Travis](https://img.shields.io/travis/com/maxkagamine/Moq.Contrib.HttpClient.svg)](https://travis-ci.com/maxkagamine/Moq.Contrib.HttpClient)

[Blog post](https://maxkagamine.com/blog/mocking-httpclient-ihttpclientfactory-with-moq-the-easy-way) &nbsp;&middot;&nbsp; [日本語](README.ja.md)

A set of extension methods for mocking HttpClient and IHttpClientFactory with Moq.

Mocking HttpClient directly is [notoriously difficult](https://github.com/dotnet/corefx/issues/1624); the general approach has been to either create a wrapper of some form to mock instead or use a specific testing library. However, the former is typically undesirable, and the latter requires switching to a separate mocking system for HTTP calls which may be less flexible or awkward in conjunction with other mocks. These extensions instead allow HttpClient to be mocked the same way as everything else using Moq without excessive boilerplate.

- [Install](#install)
- [API](#api)
  - [Request](#request)
  - [Response](#response)
- [Examples](#examples)
  - [General usage](#general-usage)
  - [Matching requests by headers, JSON, query params, etc.](#matching-requests-by-headers-json-query-params-etc)
  - [Setting up a sequence of requests](#setting-up-a-sequence-of-requests)
  - [Composing responses based on the request body](#composing-responses-based-on-the-request-body)
  - [Using IHttpClientFactory](#using-ihttpclientfactory)
  - [Complete unit test examples](#complete-unit-test-examples)
- [License / Donate](#license--donate)

## Install

`Install-Package Moq.Contrib.HttpClient`

or `dotnet add package Moq.Contrib.HttpClient`

## API

The library adds request/response versions of the standard Moq methods:

- **Setup** → SetupRequest, SetupAnyRequest
- **SetupSequence** → SetupRequestSequence, SetupAnyRequestSequence
- **Verify** → VerifyRequest, VerifyAnyRequest
- **Returns(Async)** → ReturnsResponse

`InSequence().Setup()` is supported as well.

### Request

All Setup and Verify helpers have the same overloads, abbreviated here:

```csharp
SetupAnyRequest()
SetupRequest([HttpMethod method, ]Predicate<HttpRequestMessage> match)
SetupRequest(string|Uri requestUrl[, Predicate<HttpRequestMessage> match])
SetupRequest(HttpMethod method, string|Uri requestUrl[, Predicate<HttpRequestMessage> match])
```

The `match` predicate allows for more intricate matching, including headers, and may be async to inspect the request body.

### Response

The response helpers simplify sending a StringContent, ByteArrayContent, StreamContent, or just a status code. All overloads take an action to further configure the response (i.e. setting headers).

```csharp
ReturnsResponse(HttpStatusCode statusCode[, HttpContent content], Action<HttpResponseMessage> configure = null)
ReturnsResponse([HttpStatusCode statusCode, ]string content, string mediaType = null, Encoding encoding = null, Action<HttpResponseMessage> configure = null))
ReturnsResponse([HttpStatusCode statusCode, ]byte[]|Stream content, string mediaType = null, Action<HttpResponseMessage> configure = null)
```

## Examples

### General usage

```csharp
// All requests made with HttpClient go through its handler's SendAsync() which we mock
var handler = new Mock<HttpMessageHandler>();
var client = handler.CreateClient();

// A simple example that returns 404 for any request
handler.SetupAnyRequest()
    .ReturnsResponse(HttpStatusCode.NotFound);

// Match GET requests to an endpoint that returns json (defaults to 200 OK)
handler.SetupRequest(HttpMethod.Get, "https://example.com/api/stuff")
    .ReturnsResponse(JsonConvert.SerializeObject(model), "application/json");

// Setting additional headers on the response using the optional configure action
handler.SetupRequest("https://example.com/api/stuff")
    .ReturnsResponse(bytes, configure: response =>
    {
        response.Content.Headers.LastModified = new DateTime(2018, 3, 9);
    })
    .Verifiable(); // Naturally we can use Moq methods as well

// Verify methods are provided matching the setup helpers
handler.VerifyAnyRequest(Times.Exactly(3));
```

### Matching requests by headers, JSON, query params, etc.

```csharp
// The request helpers can take a predicate for more intricate request matching
handler.SetupRequest(r => r.Headers.Authorization?.Parameter != authToken)
    .ReturnsResponse(HttpStatusCode.Unauthorized);

// This can be async as well to inspect the request body
handler
    .SetupRequest(HttpMethod.Post, url, async request =>
    {
        // This setup will only match calls with the expected id
        var json = await request.Content.ReadAsStringAsync();
        var model = JsonConvert.DeserializeObject<Model>();
        return model.Id == expected.Id;
    })
    .ReturnsResponse(HttpStatusCode.Created);

// We can also use this to check for a specific query param without regard for any others
handler.SetupRequest(r => ((Url) r.RequestUri).QueryParams["foo"].Equals("bar"))
    .ReturnsResponse("stuff");
```

The last example uses [Flurl](https://flurl.io/docs/fluent-url/), a fluent URL builder, to assist in checking the query string. This can also make constructing a URL easier, for instance if we wanted to match an exact URL and query string. See the [request extensions tests](test/Moq.Contrib.HttpClient.Test/RequestExtensionsTests.cs) for an example.

### Setting up a sequence of requests

Moq has two types of sequences:

1. `SetupSequence()` which creates one setup that returns values in sequence, and
2. `InSequence().Setup()` which creates multiple setups under `When()` conditions to ensure that they only match in order.

The latter can be useful for cases where separate requests independent of each other must be made in a certain order; their setups can be defined in a sequence such that one must match before the other. This is similar to other testing libraries that work by queueing responses.

See the [sequence extensions tests](test/Moq.Contrib.HttpClient.Test/SequenceExtensionsTests.cs) for examples of each.

### Composing responses based on the request body

Since it's Moq, we can use the normal Returns method together with the request helpers for more complex responses:

```csharp
handler.SetupRequest("https://example.com/hello")
    .Returns(async (HttpRequestMessage request, CancellationToken cancellationToken) =>
        new HttpResponseMessage()
        {
            Content = new StringContent($"Hello, {await request.Content.ReadAsStringAsync()}")
        });

var response = await client.PostAsync("https://example.com/hello", new StringContent("world"));
var body = await response.Content.ReadAsStringAsync(); // Hello, world
```

### Using IHttpClientFactory

It is common to see HttpClient wrapped in a `using` since it's IDisposable, but this is [actually incorrect and can lead to your application eating up sockets](https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/). The standard advice is to keep a static or singleton HttpClient for the lifetime of the application, but this has the drawback of not responding to DNS changes.

ASP.NET Core introduces a new [IHttpClientFactory](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests) which "manages the pooling and lifetime of underlying HttpClientMessageHandler instances to avoid common DNS problems that occur when manually managing HttpClient lifetimes." As a bonus, it also makes more accessible a little-known feature in HttpClient: the ability to plug in middleware (for example, using [Polly](https://github.com/App-vNext/Polly#polly) to automatically handle retries and failures).

Depending on the usage, your constructors may simply take an HttpClient injected via IHttpClientFactory. If the constructor takes the factory itself instead, this can be mocked the same way:

```csharp
var handler = new Mock<HttpMessageHandler>();
var factory = handler.CreateClientFactory();

// Named clients can be configured as well (overriding the default)
Mock.Get(factory).Setup(x => x.CreateClient("api"))
    .Returns(() =>
    {
        var client = handler.CreateClient();
        client.BaseAddress = ApiBaseUrl;
        return client;
    });
```

The factory can then be passed into the class or [injected via AutoMocker](https://github.com/moq/Moq.AutoMocker), and code calling `factory.CreateClient()` will receive clients backed by the mock handler.

### Complete unit test examples

Though it may be a faux pas to point to the unit tests as documentation, in this case the library is specifically for testing, and so they were written with this in mind. Thus, for some more complete working examples (with comments), please see here:

- **[Request extensions tests](test/Moq.Contrib.HttpClient.Test/RequestExtensionsTests.cs)** &mdash; these strictly cover the Setup & Verify helpers
- **[Response extensions tests](test/Moq.Contrib.HttpClient.Test/ResponseExtensionsTests.cs)** &mdash; these focus on the ReturnsResponse helpers
- **[Sequence extensions tests](test/Moq.Contrib.HttpClient.Test/SequenceExtensionsTests.cs)** &mdash; these demonstrate mocking sequences, as mentioned above

## License / Donate

MIT license.

If you found this useful, please consider buying me a ~~coffee~~ cup of tea!

[![PayPal.me](https://www.paypalobjects.com/digitalassets/c/website/marketing/apac/C2/logos-buttons/optimize/34_Blue_PayPal_Pill_Button.png)](https://paypal.me/maxkagamine)

<p>
<details>
<summary>&nbsp;<img src="https://github.com/maxkagamine/maxkagamine.com/raw/master/src/public/images/crypto.png" width="600" align="middle" /></summary>
<br />

```
BTC  32SEpPUowijZWET5tbErgskNSBkjSmLwmo
BCH  1DsWr5aoyQgcD6vYCTSViVmUcRSiYPPvPw
DOGE DAEiyJP7F7YqZN9GJ12Z2RhfkPahyyN1DY

Thank you! I wish you a safe journey to the moon :)
```
</details>
</p>
