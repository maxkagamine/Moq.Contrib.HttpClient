using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Flurl;
using Xunit;

namespace Moq.Contrib.HttpClient.Test
{
    public class RequestExtensionsTests
    {
        [Fact]
        public async Task MatchesAnyRequest()
        {
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = handler.CreateClient(); // Equivalent to `new HttpClient(handler.Object, false)`

            // See ResponseExtensionsTests for response examples; this could be shortened to `.ReturnsResponse("foo")`
            var response = new HttpResponseMessage()
            {
                Content = new StringContent("foo")
            };

            handler.SetupAnyRequest()
                .ReturnsAsync(response);

            // All requests made with HttpClient go through the handler's SendAsync() which we've mocked
            (await client.GetAsync("http://localhost")).Should().BeSameAs(response);
            (await client.PostAsync("https://example.com/foo", new StringContent("data"))).Should().BeSameAs(response);
            (await client.GetStringAsync("https://example.com/bar")).Should().Be("foo");

            // Verify methods are provided matching the setup helpers, although even without MockBehavior.Strict,
            // HttpClient will throw if the request was not mocked, so in many cases a Verify will be redundant
            handler.VerifyAnyRequest(Times.Exactly(3));
        }

        [Fact]
        public async Task MatchesRequestByUrl()
        {
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = handler.CreateClient();

            // Here we're basing the language off the url, but we could also specify a setup that checks for example the
            // Accept-Language header (see MatchesCustomPredicate below)
            var enUrl = "https://example.com/en-US/hello";
            var jaUrl = "https://example.com/ja-JP/hello";

            // The helpers return the same Moq interface as the regular Setup, so we can use the standard Moq methods
            // for more complex responses, in this case based on the request body
            handler.SetupRequest(enUrl)
                .Returns(async (HttpRequestMessage request, CancellationToken _) => new HttpResponseMessage()
                {
                    Content = new StringContent($"Hello, {await request.Content.ReadAsStringAsync()}")
                });

            handler.SetupRequest(jaUrl)
                .Returns(async (HttpRequestMessage request, CancellationToken _) => new HttpResponseMessage()
                {
                    Content = new StringContent($"こんにちは、{await request.Content.ReadAsStringAsync()}")
                });

            // Imagine we have a service that returns a greeting for a given locale
            async Task<string> GetGreeting(string locale, string name)
            {
                var response = await client.PostAsync($"https://example.com/{locale}/hello", new StringContent(name));
                return await response.Content.ReadAsStringAsync();
            }

            // Call the "service" which we expect to make the requests set up above
            string enGreeting = await GetGreeting("en-US", "world");
            string jaGreeting = await GetGreeting("ja-JP", "世界");

            enGreeting.Should().Be("Hello, world");
            jaGreeting.Should().Be("こんにちは、世界"); // Konnichiwa, sekai

            // The handler was created with MockBehavior.Strict which throws a MockException for invocations without setups
            Func<Task> esAttempt = () => GetGreeting("es-ES", "mundo");
            await esAttempt.Should().ThrowAsync<MockException>(because: "a setup for Spanish was not configured");
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        public async Task MatchesRequestByMethod(string methodStr)
        {
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = handler.CreateClient();

            var method = new HttpMethod(methodStr); // Normally you'd use HttpMethod.Get, etc.
            var url = "https://example.com";
            var expected = $"This is {methodStr}!";

            handler.SetupRequest(method, url)
                .ReturnsResponse(expected);

            var response = await client.SendAsync(new HttpRequestMessage(method, url));
            var actual = await response.Content.ReadAsStringAsync();

            actual.Should().Be(expected);

            // Ensure this isn't simply matching any request
            Func<Task> otherMethodAttempt = () => client.SendAsync(new HttpRequestMessage(HttpMethod.Options, url));
            await otherMethodAttempt.Should().ThrowAsync<MockException>("the setup should not match an OPTIONS request");
        }

        [Fact]
        public async Task MatchesCustomPredicate()
        {
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = handler.CreateClient();

            // Let's simulate posting a song to a music API
            var url = new Uri("https://example.com/api/songs");
            var token = "auth token obtained somehow";

            var expected = new Song()
            {
                Title = "The Disease Called Love",
                Artist = "Neru feat. Kagamine Rin, Kagamine Len",
                Album = "CYNICISM",
                Url = "https://youtu.be/2IH-toUoq3w"
            };

            // Set up a response for a request with this song
            handler
                .SetupRequest(HttpMethod.Post, url, async request =>
                {
                    // Here we can parse the request json. Be sure to use the same serializer options.
                    // ReadFromJsonAsync<T>() should be avoided here, as Moq might run this matcher a second time, and
                    // unlike ReadAsStringAsync() it will throw if it tries to read the content again (maybe a bug?).
                    var json = await request.Content.ReadAsStringAsync();
                    var model = JsonSerializer.Deserialize<Song>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

                    // Anything you would check with It.Is() should go here. Tip: If Song were a record type, we could
                    // compare the entire object at once using `return json == model`.
                    return model.Title == expected.Title /* ... */;
                })
                // Or, if you know the code under test is using the System.Text.Json extensions, you can skip
                // deserialization and access the original class directly:
                // .SetupRequest(HttpMethod.Post, url, async r => ((JsonContent)r.Content).Value == expected)
                .ReturnsResponse(HttpStatusCode.Created);
                // Alternatively, to do asserts on the sent model (like a db insert), use Callback to save the model,
                // and then Verify it was only called once. (You can also just put asserts in the match predicate!)
                // .Callback((HttpRequestMessage request, CancellationToken _) =>
                // {
                //     actual = JsonSerializer.Deserialize<Song>(request.Content.ReadAsStringAsync().Result);
                // });

            // A request without a valid auth token should fail (the last setup takes precedence)
            handler.SetupRequest(r => r.Headers.Authorization?.Parameter != token)
                .ReturnsResponse(HttpStatusCode.Unauthorized);

            // Imaginary service method that calls the API we're mocking
            async Task CreateSong(Song song, string authToken)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                var response = await client.PostAsJsonAsync(url, song);
                response.EnsureSuccessStatusCode();
            }

            // Create the song
            await CreateSong(expected, token);

            // The setup won't match if the request json contains a different song
            Func<Task> wrongSongAttempt = () => CreateSong(new Song()
            {
                Title = "Plug Out (HSP 2012 Remix)",
                Artist = "鼻そうめんP feat. 初音ミク",
                Album = "Hiroyuki ODA pres. HSP WORKS 11-14",
                Url = "https://vocadb.net/S/21567"
            }, token);

            await wrongSongAttempt.Should().ThrowAsync<MockException>("wrong request body");

            // Attempt to create the song again, this time without a valid token (if we were actually testing a service,
            // this would probably be a separate unit test)
            Func<Task> unauthorizedAttempt = () => CreateSong(expected, "expired token");
            await unauthorizedAttempt.Should().ThrowAsync<HttpRequestException>("this should 400, causing EnsureSuccessStatusCode() to throw");
        }

        [Fact]
        public async Task MatchesQueryParameters()
        {
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = handler.CreateClient();

            string baseUrl = "https://example.com:8080/api/v2";

            // A URL builder like Flurl can make dealing with query params easier (https://flurl.io/docs/fluent-url/)
            handler.SetupRequest(baseUrl.AppendPathSegment("search").SetQueryParam("q", "fus ro dah"))
                .ReturnsResponse(HttpStatusCode.OK);

            // Note that the above is still matching an exact url; it's often better instead to use a predicate like so,
            // since params may come in different orders and we may not need to check all of them either
            handler
                .SetupRequest(HttpMethod.Post, r =>
                {
                    Url url = r.RequestUri; // Implicit conversion from Uri to Url
                    return url.Path == baseUrl.AppendPathSegment("followers/enlist") &&
                        url.QueryParams["name"].Equals("Lydia");
                })
                .ReturnsResponse(HttpStatusCode.OK);

            await client.GetAsync("https://example.com:8080/api/v2/search?q=fus%20ro%20dah");

            // In this example we've passed an additional query param that the test doesn't care about
            await client.PostAsync("https://example.com:8080/api/v2/followers/enlist?name=Lydia&carryBurdens=yes", null);

            // Verifying just to show that both setups were invoked, rather than one for both requests (HttpClient would
            // have thrown already if none had matched for a request)
            handler.VerifyAll();
        }

        [Fact]
        public async Task VerifyHelpersThrowAsExpected() // This one is mainly for code coverage
        {
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = handler.CreateClient();

            // Mock two different endpoints
            string fooUrl = "https://example.com/foo";
            string barUrl = "https://example.com/bar";

            handler.SetupRequest(fooUrl)
                .ReturnsAsync(new HttpResponseMessage());

            handler.SetupRequest(barUrl)
                .ReturnsAsync(new HttpResponseMessage());

            // Make various calls
            await client.GetAsync(fooUrl);
            await client.PostAsync(fooUrl, new StringContent("stuff"));
            await client.GetAsync(barUrl);

            // Prepare verify attempts using various overloads
            Action verifyThreeRequests = () => handler.VerifyAnyRequest(Times.Exactly(3));
            Action verifyMoreThanThreeRequests = () => handler.VerifyAnyRequest(Times.AtLeast(4), "oh noes");
            Action verifyTwoFoos = () => handler.VerifyRequest(fooUrl, Times.Exactly(2));
            Action verifyThreeFoos = () => handler.VerifyRequest(fooUrl, Times.Exactly(3), "oh noes");
            Action verifyFooPosted = () => handler.VerifyRequest(HttpMethod.Post, fooUrl);
            Action verifyBarPosted = () => handler.VerifyRequest(HttpMethod.Post, barUrl, failMessage: "oh noes");

            // Assert that these pass or fail accordingly
            verifyThreeRequests.Should().NotThrow("we made three requests");
            verifyMoreThanThreeRequests.Should().Throw<MockException>("we only made three requests");
            verifyTwoFoos.Should().NotThrow("there were two requests to foo");
            verifyThreeFoos.Should().Throw<MockException>("there were two requests to foo, not three");
            verifyFooPosted.Should().NotThrow("we sent a POST to foo");
            verifyBarPosted.Should().Throw<MockException>("we did not send a POST to bar");

            // The fail messages should be passed along as well
            var messages = new[] { verifyMoreThanThreeRequests, verifyThreeFoos, verifyBarPosted }
                .Select(f => { try { f(); return null; } catch (MockException ex) { return ex.Message; } });
            messages.Should().OnlyContain(x => x.Contains("oh noes"), "all verify exceptions should contain the failMessage");
        }
    }
}
