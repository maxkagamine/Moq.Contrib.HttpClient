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
        // For details on IHttpClientFactory, see https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests
        // (Note that ASP.NET Core's dependency injection and IHttpClientFactory can be used outside a web project)

        [Fact]
        public async Task CreatesFactoryFromHandler()
        {
            var handler = new Mock<HttpMessageHandler>();
            var factory = handler.CreateClientFactory();
            var client = factory.CreateClient();

            factory.CreateClient().Should().NotBeSameAs(client, "each client should be a new instance");

            handler.SetupAnyRequest().ReturnsResponse(HttpStatusCode.OK);
            var response = await client.GetAsync("https://example.com");
            response.StatusCode.Should().Be(HttpStatusCode.OK, "the client should be backed by the handler mock");
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

            factory.CreateClient("api").BaseAddress.Should().Be(apiBaseUrl, "the api named client had a BaseAddress configured");
            factory.CreateClient().BaseAddress.Should().NotBe(apiBaseUrl, "other clients should not be affected");
        }
    }
}
