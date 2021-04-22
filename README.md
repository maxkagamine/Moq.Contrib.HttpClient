![](https://raw.githubusercontent.com/maxkagamine/Moq.Contrib.HttpClient/7981a8dfe9c076b10d2dae2234e2f01f57731b6a/banner.png)

# Moq.Contrib.HttpClient

[![NuGet][nuget badge]][nuget] [![ci build badge]][ci build]

[nuget badge]: https://img.shields.io/nuget/dt/Moq.Contrib.HttpClient?label=Downloads&logo=nuget&logoColor=959da5&labelColor=2d343a
[ci build badge]: https://github.com/maxkagamine/Moq.Contrib.HttpClient/workflows/CI%20build/badge.svg?branch=master&event=push
[nuget]: https://www.nuget.org/packages/Moq.Contrib.HttpClient/
[ci build]: https://github.com/maxkagamine/Moq.Contrib.HttpClient/actions?query=workflow%3A%22CI+build%22

[Blog post](https://kagamine.dev/en/mock-httpclient-the-easy-way/) &nbsp;&middot;&nbsp; [日本語](README.ja.md)

A set of extension methods for mocking HttpClient and IHttpClientFactory with Moq.

Mocking HttpClient directly is [notoriously difficult](https://github.com/dotnet/corefx/issues/1624). The solution has generally been to either create a wrapper of some form to mock instead (at the cost of cluttering the code) or use an HttpClient-specific testing library (which requires switching to a separate mocking system for HTTP calls and may not fit well alongside other mocks).

These extension methods make mocking HTTP requests as easy as mocking a service method.

- [Install](#install)
- [API](#api)
  - [Request](#request)
  - [Response](#response)
- [Examples](#examples)
  - [General usage](#general-usage)
  - [Matching requests by query params, headers, JSON body, etc.](#matching-requests-by-query-params-headers-json-body-etc)
  - [Setting up a sequence of requests](#setting-up-a-sequence-of-requests)
  - [Composing responses based on the request body](#composing-responses-based-on-the-request-body)
  - [Using IHttpClientFactory](#using-ihttpclientfactory)
  - [Integration tests](#integration-tests)
  - [Complete unit test examples](#complete-unit-test-examples)
- [License](#license)

## Install

`Install-Package Moq.Contrib.HttpClient`

or `dotnet add package Moq.Contrib.HttpClient`

## API

The library adds request/response variants of the standard Moq methods:

- **Setup** → SetupRequest, SetupAnyRequest
- **SetupSequence** → SetupRequestSequence, SetupAnyRequestSequence
- **Verify** → VerifyRequest, VerifyAnyRequest
- **Returns(Async)** → ReturnsResponse

### Request

All Setup and Verify helpers have the same overloads, abbreviated here:

```csharp
SetupAnyRequest()
SetupRequest([HttpMethod method, ]Predicate<HttpRequestMessage> match)
SetupRequest(string|Uri requestUrl[, Predicate<HttpRequestMessage> match])
SetupRequest(HttpMethod method, string|Uri requestUrl[, Predicate<HttpRequestMessage> match])
```

`requestUrl` matches the exact request URL, while the `match` predicate allows for more intricate matching, such as by query params or headers, and may be async as well to inspect the request body.

### Response

The response helpers simplify sending a StringContent, ByteArrayContent, StreamContent, or just a status code:

```csharp
ReturnsResponse(HttpStatusCode statusCode[, HttpContent content], Action<HttpResponseMessage> configure = null)
ReturnsResponse([HttpStatusCode statusCode, ]string content, string mediaType = null, Encoding encoding = null, Action<HttpResponseMessage> configure = null))
ReturnsResponse([HttpStatusCode statusCode, ]byte[]|Stream content, string mediaType = null, Action<HttpResponseMessage> configure = null)
```

The `statusCode` defaults to 200 OK if omitted, and the `configure` action can be used to set response headers.

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

### Matching requests by query params, headers, JSON body, etc.

```csharp
// The request helpers can take a predicate for more intricate request matching
handler.SetupRequest(r => r.Headers.Authorization?.Parameter != authToken)
    .ReturnsResponse(HttpStatusCode.Unauthorized);

// The predicate can be async as well to inspect the request body
handler
    .SetupRequest(HttpMethod.Post, url, async request =>
    {
        // This setup will only match calls with the expected id
        var json = await request.Content.ReadAsStringAsync();
        var model = JsonConvert.DeserializeObject<Model>();
        return model.Id == expected.Id;
    })
    .ReturnsResponse(HttpStatusCode.Created);

// This is particularly useful for matching URLs with query parameters
handler.SetupRequest(r =>
{
    Url url = r.RequestUri;
    return url.Path == baseUrl.AppendPathSegment("endpoint") &&
        url.QueryParams["foo"].Equals("bar");
})
    .ReturnsResponse("stuff");
```

The last example uses a URL builder library called [Flurl](https://flurl.io/docs/fluent-url/) to assist in checking the query string.

### Setting up a sequence of requests

Moq has two types of sequences:

1. `SetupSequence()` which creates one setup that returns values in sequence, and
2. `InSequence().Setup()` which creates multiple setups under `When()` conditions to ensure that they only match in order.

Note that in most cases, these are not necessary, and regular setups should be used instead. However, the latter can be useful for cases where separate requests independent of each other must be made in a certain order.

See the [sequence extensions tests](test/Moq.Contrib.HttpClient.Test/SequenceExtensionsTests.cs) for examples of each.

### Composing responses based on the request body

Since it's all still Moq, we can use the normal Returns method together with the request helpers for more complex responses:

```csharp
handler.SetupRequest("https://example.com/hello")
    .Returns(async (HttpRequestMessage request, CancellationToken _) => new HttpResponseMessage()
    {
        Content = new StringContent($"Hello, {await request.Content.ReadAsStringAsync()}")
    });

var response = await client.PostAsync("https://example.com/hello", new StringContent("world"));
var body = await response.Content.ReadAsStringAsync(); // Hello, world
```

### Using IHttpClientFactory

It's common to see HttpClient wrapped in a `using` since it's IDisposable, but this is, rather counterintuitively, incorrect and [can lead to the application eating up sockets](https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/). The standard advice is to reuse a single HttpClient, yet this has the drawback of not responding to DNS changes.

ASP.NET Core introduces an [IHttpClientFactory](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests) which "manages the pooling and lifetime of underlying HttpClientMessageHandler instances to avoid common DNS problems that occur when manually managing HttpClient lifetimes." As a bonus, it also makes HttpClient's [ability to plug in middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests#outgoing-request-middleware) more accessible &mdash; for example, using [Polly](https://github.com/App-vNext/Polly#polly) to automatically handle retries and failures.

Depending on the usage, your constructors may simply take an HttpClient injected via IHttpClientFactory, in which case the tests don't need to do anything different. If the constructor takes the factory itself instead, this can be mocked the same way:

```csharp
var handler = new Mock<HttpMessageHandler>();
var factory = handler.CreateClientFactory();
```

The factory can then be passed into the class or [injected via AutoMocker](https://github.com/moq/Moq.AutoMocker), and code calling `factory.CreateClient()` will receive clients backed by the mock handler.

The `CreateClientFactory()` extension method returns a mock that's already set up to return a default client. If you're using [named clients](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.1#named-clients), a setup can be added like so:

```csharp
// Configuring a named client (overriding the default)
Mock.Get(factory).Setup(x => x.CreateClient("api"))
    .Returns(() =>
    {
        var client = handler.CreateClient();
        client.BaseAddress = ApiBaseUrl;
        return client;
    });
```

> Note: If you're getting a "Extension methods (here: HttpClientFactoryExtensions.CreateClient) may not be used in setup / verification expressions." error, make sure you're passing a string where it says `"api"` in the example.

### Integration tests

For [integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests), rather than replace the IHttpClientFactory implementation in the service collection, it's possible to leverage the existing DI infrastructure and configure it to use a mock handler as the "primary" HttpMessageHandler instead:

```csharp
public class ExampleTests : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> factory;
    private readonly Mock<HttpMessageHandler> githubHandler = new();

    public ExampleTests(WebApplicationFactory<Startup> factory)
    {
        this.factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // For the default (unnamed) client, use `Options.DefaultName`
                services.AddHttpClient("github")
                    .ConfigurePrimaryHttpMessageHandler(() => githubHandler.Object);
            });
        });
    }
```

This way, the integration tests use the same dependency injection and HttpClient configurations from `ConfigureServices()` as would normally be used.

See [this sample ASP.NET Core app](test/IntegrationTestExample/Startup.cs) and [its integration test](test/IntegrationTestExample.Test/ExampleTests.cs) for a working example.

### Complete unit test examples

Though it may be a faux pas to point to unit tests as documentation, in this case they were written for exactly that purpose, so for more complete usage examples, please see here:

- **[Request extensions tests](test/Moq.Contrib.HttpClient.Test/RequestExtensionsTests.cs)** &mdash; these cover the Setup & Verify helpers
- **[Response extensions tests](test/Moq.Contrib.HttpClient.Test/ResponseExtensionsTests.cs)** &mdash; these cover the ReturnsResponse overloads
- **[Sequence extensions tests](test/Moq.Contrib.HttpClient.Test/SequenceExtensionsTests.cs)** &mdash; these demonstrate mocking explicit sequences, as mentioned above

## License

MIT
