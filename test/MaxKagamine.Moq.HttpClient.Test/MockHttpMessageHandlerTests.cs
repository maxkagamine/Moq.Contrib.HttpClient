using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace MaxKagamine.Moq.HttpClient.Test
{
    public class MockHttpMessageHandlerTests
    {
        [Fact]
        public async Task MatchesAnyRequest()
        {
            var handler = new MockHttpMessageHandler();
            var client = handler.CreateClient();
            
            var response = new HttpResponseMessage()
            {
                Content = new StringContent("foo")
            };

            handler.SetupAnyRequest()
                .ReturnsAsync(response);
            
            (await client.GetAsync("http://localhost")).Should().BeSameAs(response);
            (await client.PostAsync("https://example.com/foo", new StringContent("data"))).Should().BeSameAs(response);
            (await client.GetStringAsync("https://example.com/bar")).Should().Be("foo");

            // Verify methods are provided to match the setup helpers
            handler.VerifyAnyRequest(Times.Exactly(3));
        }

        [Fact]
        public async Task MatchesRequestByUrl()
        {
            var handler = new MockHttpMessageHandler(MockBehavior.Strict);
            var client = handler.CreateClient();

            // Here we're basing the language off the url, but since it's Moq, we could also specify
            // a setup that checks the Accept-Language header or any other criteria (a helper is
            // provided to simplify checking the HttpRequestMessage; see other test below)
            var enUrl = "https://example.com/en-US/hello";
            var jaUrl = "https://example.com/ja-JP/hello";

            // The helpers return the same fluent api as the regular Setup, so we can use the normal
            // Moq methods for more complex responses
            handler.SetupRequest(enUrl)
                .ReturnsAsync((HttpRequestMessage request, CancellationToken cancellationToken) =>
                    new HttpResponseMessage()
                    {
                        Content = new StringContent($"Hello, {request.Content.ReadAsStringAsync().Result}")
                    });

            handler.SetupRequest(jaUrl)
                .ReturnsAsync((HttpRequestMessage request, CancellationToken cancellationToken) =>
                    new HttpResponseMessage()
                    {
                        Content = new StringContent($"こんにちは、{request.Content.ReadAsStringAsync().Result}")
                    });

            var enResponse = await client.PostAsync(enUrl, new StringContent("world"));
            var jaResponse = await client.PostAsync(jaUrl, new StringContent("世界"));

            (await enResponse.Content.ReadAsStringAsync()).Should().Be("Hello, world");
            (await jaResponse.Content.ReadAsStringAsync()).Should().Be("こんにちは、世界"); // Konnichiwa, sekai

            // This handler was created with MockBehavior.Strict which throws for invocations without setups
            Action esAttempt = () => client.PostAsync("https://example.com/es-ES/hello", new StringContent("mundo"));
            esAttempt.Should().Throw<MockException>("a setup for Spanish was not configured");

            handler.VerifyRequest(enUrl, Times.Once());
            handler.VerifyRequest(jaUrl, Times.Once());
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        public void MatchesRequestByMethod(string methodStr)
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void MatchesCustomPredicate()
        {
            // TODO: Demonstrate matching request body json by hand

            throw new NotImplementedException();
        }
    }
}
