# Moq.HttpClient

[![NuGet](https://img.shields.io/nuget/v/MaxKagamine.Moq.HttpClient.svg)](https://www.nuget.org/packages/MaxKagamine.Moq.HttpClient/) [![Travis](https://img.shields.io/travis/com/maxkagamine/Moq.HttpClient.svg)](https://travis-ci.com/maxkagamine/Moq.HttpClient)

[ブログ投稿](https://maxkagamine.jp/blog/moq-de-httpclient-to-ihttpclientfactory-o-mokkusuru-kantanna-houhou) &nbsp;&middot;&nbsp; [English](README.md)

MoqでHttpClientとIHttpClientFactoryのテストダブルを作るため拡張メソッドのセットです

HttpClientを直にモックすることが難しいのは[よく知られています](https://github.com/dotnet/corefx/issues/1624)。普通の仕方はモックできられる何らかのラッパーを作るかHttpClientのために特別にテスティングライブラリを使うことですけど、前者はよく望ましくなくて、後者はHTTPリクエストだけのために別のモックシステムに切り替えることが必要で、あのシステムの方が柔軟ではないか他のモックと一緒に不味そうですかもしれない。この拡張ではたくさんのボイラプレートなしで他のすべてと同じようにMoqでHttpClientをモックできます

- [インストール](#%E3%82%A4%E3%83%B3%E3%82%B9%E3%83%88%E3%83%BC%E3%83%AB)
- [API](#api)
  - [リクエスト](#%E3%83%AA%E3%82%AF%E3%82%A8%E3%82%B9%E3%83%88)
  - [レスポンス](#%E3%83%AC%E3%82%B9%E3%83%9D%E3%83%B3%E3%82%B9)
- [用例](#%E7%94%A8%E4%BE%8B)
  - [一般的な使用法](#%E4%B8%80%E8%88%AC%E7%9A%84%E3%81%AA%E4%BD%BF%E7%94%A8%E6%B3%95)
  - [ヘッダーとかJSONとかクエリパラメータでリクエストをマッチする](#%E3%83%98%E3%83%83%E3%83%80%E3%83%BC%E3%81%A8%E3%81%8Bjson%E3%81%A8%E3%81%8B%E3%82%AF%E3%82%A8%E3%83%AA%E3%83%91%E3%83%A9%E3%83%A1%E3%83%BC%E3%82%BF%E3%81%A7%E3%83%AA%E3%82%AF%E3%82%A8%E3%82%B9%E3%83%88%E3%82%92%E3%83%9E%E3%83%83%E3%83%81%E3%81%99%E3%82%8B)
  - [リクエストのシークエンスをセットアップする](#%E3%83%AA%E3%82%AF%E3%82%A8%E3%82%B9%E3%83%88%E3%81%AE%E3%82%B7%E3%83%BC%E3%82%AF%E3%82%A8%E3%83%B3%E3%82%B9%E3%82%92%E3%82%BB%E3%83%83%E3%83%88%E3%82%A2%E3%83%83%E3%83%97%E3%81%99%E3%82%8B)
  - [リクエストの本体に基づいてレスポンスを書く](#%E3%83%AA%E3%82%AF%E3%82%A8%E3%82%B9%E3%83%88%E3%81%AE%E6%9C%AC%E4%BD%93%E3%81%AB%E5%9F%BA%E3%81%A5%E3%81%84%E3%81%A6%E3%83%AC%E3%82%B9%E3%83%9D%E3%83%B3%E3%82%B9%E3%82%92%E6%9B%B8%E3%81%8F)
  - [IHttpClientFactoryの使い方](#ihttpclientfactory%E3%81%AE%E4%BD%BF%E3%81%84%E6%96%B9)
  - [完全なユニットテストの用例](#%E5%AE%8C%E5%85%A8%E3%81%AA%E3%83%A6%E3%83%8B%E3%83%83%E3%83%88%E3%83%86%E3%82%B9%E3%83%88%E3%81%AE%E7%94%A8%E4%BE%8B)
- [ライセンス / 寄付](#%E3%83%A9%E3%82%A4%E3%82%BB%E3%83%B3%E3%82%B9--%E5%AF%84%E4%BB%98)

## インストール

`Install-Package MaxKagamine.Moq.HttpClient`

または `dotnet add package MaxKagamine.Moq.HttpClient`

## API

このライブラリは普通のMoqメソッドのリクエストとレスポンス版を追加します：

- **Setup** → SetupRequest, SetupAnyRequest
- **SetupSequence** → SetupRequestSequence, SetupAnyRequestSequence
- **Verify** → VerifyRequest, VerifyAnyRequest
- **Returns(Async)** → ReturnsResponse

`InSequence().Setup()` もサポートされます

### リクエスト

すべとのSetupとVerifyのヘルパーはここで略した同じオーバーロードがあります：

```csharp
SetupAnyRequest()
SetupRequest([HttpMethod method, ]Predicate<HttpRequestMessage> match)
SetupRequest(string|Uri requestUrl[, Predicate<HttpRequestMessage> match])
SetupRequest(HttpMethod method, string|Uri requestUrl[, Predicate<HttpRequestMessage> match])
```

その`match`という述語を使うともっと複雑にリクエスト（とヘッダー）をマッチすることができます。本体を調べるためにasyncになれます

### レスポンス

レスポンスのヘルパーはStringContent、ByteArrayContent、StreamContentかステータスコードだけを送ることを簡単にします。すべてのオーバーロードはレスポンスをもっとコンフィグする（つまり、ヘッダーを設定する）ためActionを受け取ります

```csharp
ReturnsResponse(HttpStatusCode statusCode[, HttpContent content], Action<HttpResponseMessage> configure = null)
ReturnsResponse([HttpStatusCode statusCode, ]string content, string mediaType = null, Encoding encoding = null, Action<HttpResponseMessage> configure = null))
ReturnsResponse([HttpStatusCode statusCode, ]byte[]|Stream content, string mediaType = null, Action<HttpResponseMessage> configure = null)
```

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

// 任意なconfigureアクションでレスポンスにもっとヘッダーを設定する
handler.SetupRequest("https://example.com/api/stuff")
    .ReturnsResponse(bytes, configure: response =>
    {
        response.Content.Headers.LastModified = new DateTime(2018, 3, 9);
    })
    .Verifiable(); // 当然Moqのメソッドも使えます

// Setupヘッダーと同じようVerifyメソッドが備えられる
handler.VerifyAnyRequest(Times.Exactly(3));
```

### ヘッダーとかJSONとかクエリパラメータでリクエストをマッチする

```csharp
// リクエストのヘルパーはもっと複雑にマッチするために述語を受け取ります
handler.SetupRequest(r => r.Headers.Authorization?.Parameter != authToken)
    .ReturnsResponse(HttpStatusCode.Unauthorized);

// これは本体を調べるためにasyncにもなれます
handler
    .SetupRequest(HttpMethod.Post, url, async request =>
    {
        // このセットアップは予期された (expected) IDがあるリクエストのみをマッチする
        var json = await request.Content.ReadAsStringAsync();
        var model = JsonConvert.DeserializeObject<Model>();
        return model.Id == expected.Id;
    })
    .ReturnsResponse(HttpStatusCode.Created);

// 他に関係なく特定のクエリパラメータをチェックするためも使えます
handler.SetupRequest(r => ((Url) r.RequestUri).QueryParams["hoge"].Equals("piyo"))
    .ReturnsResponse("stuff");
```

最後の例えはクエリ文字列をチェックするのを手伝うために[Flurl](https://flurl.io/docs/fluent-url/)というフルーエントURLビルダーを使います。これはURLを作るも簡単にすることができます、例えば精密なURLとクエリ文字列をマッチしたかった。用例は[リクエスト拡張のテスト](test/MaxKagamine.Moq.HttpClient.Test/RequestExtensionsTests.cs)を見てください

### リクエストのシークエンスをセットアップする

Moqは2つシークエンスのタイプがあります：

1. `SetupSequence()` は順に値を返す一つセットアップを作る
2. `InSequence().Setup()` は必ず順にマッチするのために`When()`条件で複数のセットアップを作る

互いに独立しているリクエストが特定の順序にマッチするの必要がある場合は後者が便利です。セットアップは一方が他方の前に必ずマッチするためにシークエンスで定義される。これはレスポンスをキューに入れることによって動作する他のライブラリと似ています

両方の用例は[シークエンス拡張のテスト](test/MaxKagamine.Moq.HttpClient.Test/SequenceExtensionsTests.cs)を見てください

### リクエストの本体に基づいてレスポンスを書く

まだMoqだからもっと複雑のレスポンスのためにリクエストのヘルパーと一緒に普通のMoqメソッドを使えます：

```csharp
handler.SetupRequest("https://example.com/hello")
    .Returns(async (HttpRequestMessage request, CancellationToken cancellationToken) =>
        new HttpResponseMessage()
        {
            Content = new StringContent($"こんにちは、{await request.Content.ReadAsStringAsync()}")
        });

var response = await client.PostAsync("https://example.com/hello", new StringContent("世界"));
var body = await response.Content.ReadAsStringAsync(); // こんにちは、世界
```

### IHttpClientFactoryの使い方

HttpClientが`IDisposable`だから`using`の中でよく見られますが、これは[実は正しくないしアプリがソケットを平らげているのにつながります](https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/)。普通の忠告はアプリケーションのライフタイムの全期間staticかsingletonなHttpClientを保つことですがこれはDNSの変更に反応しないというマイナスがあります

ASP.NET Coreは「HttpClientのライフタイムを手動で管理するときに発生する一般的なDNSの問題を回避のために基礎となるHttpClientMessageHandlerインスタンスのプーリングとライフタイムを管理する」という新しい[IHttpClientFactory](https://docs.microsoft.com/ja-jp/aspnet/core/fundamentals/http-requests)を導入します。ボーナスとして、これはHttpClientのあまり知られていないフィーチャーをもっととっつきやすいにする：ミドルウェアをプラグインする能力（例えば、再試行と失敗を自動的に処理するために[Polly](https://github.com/App-vNext/Polly#polly)を使う）

利用法次第では、コンストラクタが単にIHttpClientFactoryによって注入したHttpClientを受け取るかもしれない。コンストラクタはファクトリーそのものを受け取れば、これは同じようにモックされる：

```csharp
var handler = new Mock<HttpMessageHandler>();
var factory = handler.CreateClientFactory();

// 名前付きクライアントも設定できる（デフォルトを無効にする）
Mock.Get(factory).Setup(x => x.CreateClient("api"))
    .Returns(() =>
    {
        var client = handler.CreateClient();
        client.BaseAddress = ApiBaseUrl;
        return client;
    });
```

このファクトリーはクラスに渡せる、それとも[AutoMockerによって注入](https://github.com/moq/Moq.AutoMocker)できる、そして`factory.CreateClient()`を呼び出すコードがモックなハンドラを使うクライアントを受けます

### 完全なユニットテストの用例

文書化としてユニットテストに向けるのが過ちかもしれないけど、この場合はこのライブラリがテストのためだしテストがこれを念頭に置いて書かれたから、もっと完全な用例（コメントと）は、こちら見てください：

- **[リクエスト拡張のテスト](test/MaxKagamine.Moq.HttpClient.Test/RequestExtensionsTests.cs)** &mdash; これらはSetupとVerifyのヘルパーのみを使います
- **[レスポンス拡張のテスト](test/MaxKagamine.Moq.HttpClient.Test/ResponseExtensionsTests.cs)** &mdash; これらはReturnsResponseヘルパーに焦点をあてます
- **[シークエンス拡張のテスト](test/MaxKagamine.Moq.HttpClient.Test/SequenceExtensionsTests.cs)** &mdash; これらはシークエンスをモックすることを実証します

## ライセンス / 寄付

MITライセンス

決して必要ではないけれど、もしこれが役立つならば、寄付がすごく感謝されます：

[![PayPal.me](https://www.paypalobjects.com/digitalassets/c/website/marketing/apac/C2/logos-buttons/optimize/34_Blue_PayPal_Pill_Button.png)](https://www.paypal.me/maxkagamine/0jpy?country.x=JP&locale.x=ja_JP)

<p>
<details>
<summary>&nbsp;<img src="https://github.com/maxkagamine/maxkagamine.com/raw/master/src/public/images/crypto.png" width="600" align="middle" /></summary>
<br />

```
BTC  32SEpPUowijZWET5tbErgskNSBkjSmLwmo
BCH  1DsWr5aoyQgcD6vYCTSViVmUcRSiYPPvPw
DOGE DAEiyJP7F7YqZN9GJ12Z2RhfkPahyyN1DY

ありがとうございます！
```
</details>
</p>
