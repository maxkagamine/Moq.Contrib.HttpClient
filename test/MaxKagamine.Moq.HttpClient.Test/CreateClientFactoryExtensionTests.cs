using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace MaxKagamine.Moq.HttpClient.Test
{
    public class CreateClientFactoryExtensionTests
    {
        [Fact]
        public async Task CreatesFactoryFromHandler()
        {
            var handler = new Mock<HttpMessageHandler>();
            var factory = handler.CreateClientFactory();
            var client = factory.CreateClient();

            // Each client should be a new instance
            factory.CreateClient().Should().NotBeSameAs(client);

            // Client should be backed by the handler mock
            handler.SetupAnyRequest().ReturnsResponse(HttpStatusCode.OK);
            var response = await client.GetAsync("https://example.com");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public void CanConfigureNamedClients()
        {
            var handler = new Mock<HttpMessageHandler>();
            var factory = handler.CreateClientFactory();

            var apiBaseUrl = new Uri("https://api.example.com");

            Mock.Get(factory).Setup(x => x.CreateClient("api"))
                .Returns(() =>
                {
                    var client = handler.CreateClient();
                    client.BaseAddress = apiBaseUrl;
                    return client;
                });

            factory.CreateClient("api").BaseAddress.Should().Be(apiBaseUrl);
            factory.CreateClient().BaseAddress.Should().NotBe(apiBaseUrl);
        }
    }
}
