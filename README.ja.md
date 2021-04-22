![](https://raw.githubusercontent.com/maxkagamine/Moq.Contrib.HttpClient/7981a8dfe9c076b10d2dae2234e2f01f57731b6a/banner.ja.png)

# Moq.Contrib.HttpClient

[![NuGet][nuget badge]][nuget] [![ci build badge]][ci build]

[nuget badge]: https://img.shields.io/nuget/dt/Moq.Contrib.HttpClient?label=Downloads&logo=nuget&logoColor=959da5&labelColor=2d343a
[ci build badge]: https://github.com/maxkagamine/Moq.Contrib.HttpClient/workflows/CI%20build/badge.svg?branch=master&event=push
[nuget]: https://www.nuget.org/packages/Moq.Contrib.HttpClient/
[ci build]: https://github.com/maxkagamine/Moq.Contrib.HttpClient/actions?query=workflow%3A%22CI+build%22

[ブログ投稿](https://kagamine.dev/ja/httpclient-mock-kantan-na-houhou/) &nbsp;&middot;&nbsp; [English](README.md)

MoqでHttpClientとIHttpClientFactoryのテストダブルを作るための拡張メソッドのセットです

HttpClientを直接モックするのは難しいことは[よく知られている](https://github.com/dotnet/corefx/issues/1624)。一般の解決法は何かのラッパーを作って代わりにそれをモックすること（コードを乱雑しても）またはHttpClient固有のテスティングライブラリを使うこと（しかしHTTP呼び出しのために別のモックシステムに切り替える必要があるし他のモックとうまく合わないかも）

この拡張メソッドはHTTPリクエストをモックすることをサービスメソッドをモックするように簡単にします

- [インストール](#インストール)
- [API](#api)
  - [リクエスト](#リクエスト)
  - [レスポンス](#レスポンス)
- [用例](#用例)
  - [一般的な使用法](#一般的な使用法)
  - [クエリパラメータやヘッダーやJSONの本体でリクエストをマッチする](#クエリパラメータやヘッダーやjsonの本体でリクエストをマッチする)
  - [リクエストのシークエンスをセットアップする](#リクエストのシークエンスをセットアップする)
  - [リクエストの本体に基づいてレスポンスを書く](#リクエストの本体に基づいてレスポンスを書く)
  - [IHttpClientFactoryの使い方](#ihttpclientfactoryの使い方)
  - [統合テスト](#統合テスト)
  - [完全なユニットテストの用例](#完全なユニットテストの用例)
- [ライセンス](#ライセンス)

## インストール

`Install-Package Moq.Contrib.HttpClient`

または `dotnet add package Moq.Contrib.HttpClient`

## API

Moqの普通のメソッドにリクエスト版とレスポンス版が追加される：

- **Setup** → SetupRequest, SetupAnyRequest
- **SetupSequence** → SetupRequestSequence, SetupAnyRequestSequence
- **Verify** → VerifyRequest, VerifyAnyRequest
- **Returns(Async)** → ReturnsResponse

### リクエスト

リクエストのヘルパーはすべて同じオーバーロードがある：

```csharp
SetupAnyRequest()
SetupRequest([HttpMethod method, ]Predicate<HttpRequestMessage> match)
SetupRequest(string|Uri requestUrl[, Predicate<HttpRequestMessage> match])
SetupRequest(HttpMethod method, string|Uri requestUrl[, Predicate<HttpRequestMessage> match])
```

`requestUrl`は正確なURLをマッチして、`match`という述語はクエリパラメータやヘッダーでマッチできるしリクエストの本体をチェックするためにasyncになれる

### レスポンス

レスポンスのヘルパーはStringContent、ByteArrayContent、StreamContent、それともステータスコードだけを送ることを簡単にする：

```csharp
ReturnsResponse(HttpStatusCode statusCode[, HttpContent content], Action<HttpResponseMessage> configure = null)
ReturnsResponse([HttpStatusCode statusCode, ]string content, string mediaType = null, Encoding encoding = null, Action<HttpResponseMessage> configure = null))
ReturnsResponse([HttpStatusCode statusCode, ]byte[]|Stream content, string mediaType = null, Action<HttpResponseMessage> configure = null)
```

`statusCode`が省略されると200 OKにディフォルトする。`configure`というアクションがレスポンスのヘッダーを設定するように使える

## 用例

### 一般的な使用法

```csharp
// HttpClientで送ったリクエストがモックされるハンドラのSendAsync()中に通る
var handler = new Mock<HttpMessageHandler>();
var client = handler.CreateClient();

// すべてのリクエストに404を送る簡単な例え
handler.SetupAnyRequest()
    .ReturnsResponse(HttpStatusCode.NotFound);

// JSONを返すエンドポイントへのGETリクエストをマッチする (デフォルトは200 OK)
handler.SetupRequest(HttpMethod.Get, "https://example.com/api/stuff")
    .ReturnsResponse(JsonConvert.SerializeObject(model), "application/json");

// 任意なconfigureアクションでレスポンスにもっとのヘッダーを設定する
handler.SetupRequest("https://example.com/api/stuff")
    .ReturnsResponse(bytes, configure: response =>
    {
        response.Content.Headers.LastModified = new DateTime(2018, 3, 9);
    })
    .Verifiable(); // もちろん、Moqのメソッドも使える

// SetupヘルパーのようなVerifyメソッドがある
handler.VerifyAnyRequest(Times.Exactly(3));
```

### クエリパラメータやヘッダーやJSONの本体でリクエストをマッチする

```csharp
// リクエストのヘルパーはもっと複雑にマッチするために述語を受け取る
handler.SetupRequest(r => r.Headers.Authorization?.Parameter != authToken)
    .ReturnsResponse(HttpStatusCode.Unauthorized);

// 述語が本体をチェックするためにasyncにもなれる
handler
    .SetupRequest(HttpMethod.Post, url, async request =>
    {
        // このセットアップは予期された (expected) IDがあるリクエストのみをマッチする
        var json = await request.Content.ReadAsStringAsync();
        var model = JsonConvert.DeserializeObject<Model>();
        return model.Id == expected.Id;
    })
    .ReturnsResponse(HttpStatusCode.Created);

// とくにはクエリパラメータがあるURLをマッチするために便利です
handler.SetupRequest(r =>
{
    Url url = r.RequestUri;
    return url.Path == baseUrl.AppendPathSegment("endpoint") &&
        url.QueryParams["hoge"].Equals("piyo");
})
    .ReturnsResponse("stuff");
```

最後の例えはクエリ文字列をチェックするのを手伝うために[Flurl](https://flurl.io/docs/fluent-url/)というURLビルダーのライブラリを使う

### リクエストのシークエンスをセットアップする

Moqは2つシークエンスのタイプがある：

1. `SetupSequence()` は順に値を返す一つセットアップを作る
2. `InSequence().Setup()` は必ず順にマッチするのために`When()`条件で複数のセットアップを作る

ほとんどの場合はこれが必要がないし普通のセットアップの方がいいとを注意してください。それでも互いに独立しているリクエストが特定の順序にマッチするの必要がある場合は後者が便利です

両方の用例は[シークエンス拡張のテスト](test/Moq.Contrib.HttpClient.Test/SequenceExtensionsTests.cs)を見てください

### リクエストの本体に基づいてレスポンスを書く

すべてはまだMoqなので、もっと複雑のレスポンスのためには普通のMoqのメソッドがリクエストのヘルパーと一緒に使用できる：

```csharp
handler.SetupRequest("https://example.com/hello")
    .Returns(async (HttpRequestMessage request, CancellationToken _) => new HttpResponseMessage()
    {
        Content = new StringContent($"こんにちは、{await request.Content.ReadAsStringAsync()}")
    });

var response = await client.PostAsync("https://example.com/hello", new StringContent("世界"));
var body = await response.Content.ReadAsStringAsync(); // こんにちは、世界
```

### IHttpClientFactoryの使い方

HttpClientはIDisposableなので`using`中によく入れられてしまう。でも直感に反して、これは間違うし、[ソケットを使い果たしていることにつながる可能性があるんです](https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/)。一般の忠告は単一のHttpClientを再利用することだけどそうしたらDNSの変更に反応しない

ASP.NET Coreが「`HttpClient`のライフタイムを手動で管理するときに発生する一般的なDNSの問題を回避のために基礎となる`HttpClientMessageHandler`インスタンスのプーリングとライフタイムを管理する」[IHttpClientFactory](https://docs.microsoft.com/ja-jp/aspnet/core/fundamentals/http-requests)を導入する。ボーナスとして、これが[ミドルウェアをプラグインするHttpClientの能力](https://docs.microsoft.com/ja-jp/aspnet/core/fundamentals/http-requests#outgoing-request-middleware)をもっととっつきやすいにする。例えば、再試行と失敗を自動的に処理するために[Polly](https://github.com/App-vNext/Polly#polly)を使うこと

利用法次第、コンストラクタが単にIHttpClientFactoryによって注入したHttpClientを受け取るかもしれない。その場合はテストが違わない。コンストラクタがファクトリーそのものを受け取る場合は、同じようにモックできる：

```csharp
var handler = new Mock<HttpMessageHandler>();
var factory = handler.CreateClientFactory();
```

このファクトリーはクラスに渡されたり[AutoMockerによって注入されたり](https://github.com/moq/Moq.AutoMocker)できる。`factory.CreateClient()`を呼び出すコードがモックなハンドラを使うクライアントを受ける

`CreateClientFactory()`という拡張メソッドはディフォルトのクライエントを返すようにセットアップされたモックを返す。[名前付きクライアント](https://docs.microsoft.com/ja-jp/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.1#named-clients)を使っている場合は次のようにセットアップを追加できる：

```csharp
// 名前付きクライアントも設定する（デフォルトを無効にする）
Mock.Get(factory).Setup(x => x.CreateClient("api"))
    .Returns(() =>
    {
        var client = handler.CreateClient();
        client.BaseAddress = ApiBaseUrl;
        return client;
    });
```

> ※「Extension methods (here: HttpClientFactoryExtensions.CreateClient) may not be used in setup / verification expressions.」というエラーが出たら、上に`"api"`がある場所に文字列を渡しているのを確認してください

### 統合テスト

[統合テスト](https://docs.microsoft.com/ja-jp/aspnet/core/test/integration-tests)は、サービスコレクションにあるIHttpClientFactory実装を変えるよりも、既存のDIインフラを活用してプライマリなHttpClientHandlerとしてモックなハンドラを使うように設定できる：

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
                // デフォルト（名前なし）のクライアントの場合は`Options.DefaultName`とを使って
                services.AddHttpClient("github")
                    .ConfigurePrimaryHttpMessageHandler(() => githubHandler.Object);
            });
        });
    }
```

これで、統合テストは普通と同じ`ConfigureServices()`での依存性注入とHttpClient設定を使う

例については、[このASP.NET Coreのサンプルのアプリ](test/IntegrationTestExample/Startup.cs)と[その統合テスト](test/IntegrationTestExample.Test/ExampleTests.cs)を見てください

### 完全なユニットテストの用例

ユニットテストがドキュメントにもなるために書かれたので、もっと完全な使い方の用例はこちら見てください：

- **[リクエスト拡張のテスト](test/Moq.Contrib.HttpClient.Test/RequestExtensionsTests.cs)** &mdash; SetupとVerifyのヘルパーに焦点をあてる
- **[レスポンス拡張のテスト](test/Moq.Contrib.HttpClient.Test/ResponseExtensionsTests.cs)** &mdash; ReturnsResponseのオーバーロードに焦点をあてます
- **[シークエンス拡張のテスト](test/Moq.Contrib.HttpClient.Test/SequenceExtensionsTests.cs)** &mdash; 明示的なシークエンスをモックすることを実証する

## ライセンス

MIT
