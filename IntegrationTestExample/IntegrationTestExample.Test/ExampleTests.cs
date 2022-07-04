using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using IntegrationTestExample.Web;
using IntegrationTestExample.Web.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Contrib.HttpClient;
using Xunit;

namespace IntegrationTestExample.Test
{
    // This shows how you can mock HttpClient at the service collection level in integration tests. Note: "factory" and
    // "client" here are part of the integration test framework, TestServer, not our mock. For more info on integration
    // tests in ASP.NET Core, see https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests
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
                    // This will configure the "github" named client to use our mock handler while keeping the default
                    // IHttpClientFactory infrastructure and any configurations made in Startup intact (as opposed to
                    // replacing the IHttpClientFactory implementation outright). This will work with typed clients,
                    // too. For the default (unnamed) client, use `Options.DefaultName`.
                    services.AddHttpClient("github")
                        .ConfigurePrimaryHttpMessageHandler(() => githubHandler.Object);
                });
            });
        }

        [Fact]
        public async Task GetReposTest()
        {
            // This is the integration test client used to make requests against the ASP.NET Core app under test
            var client = factory.CreateClient();

            var mockRepos = new GitHubRepository[]
            {
                new()
                {
                    Name = "Foo",
                    Language = "TypeScript",
                    Stars = 39
                },
                new()
                {
                    Name = "Bar",
                    Language = "C#",
                    Stars = 9001 // It's over 9000!!!
                }
            };

            var expectedResponse =
                "Foo (TypeScript, ★39)\n" +
                "Bar (C#, ★9001)\n";

            // Notice there was no need to set a BaseAddress in this test class. All of the dependency injection and the
            // AddHttpClient() in ConfigureServices() are essentially unchanged and working as they would normally.
            githubHandler.SetupRequest(HttpMethod.Get, "https://api.github.com/users/maxkagamine/repos")
                .ReturnsResponse(JsonSerializer.Serialize(mockRepos), "application/vnd.github.v3+json");

            var response = await client.GetStringAsync("/");

            response.Should().Be(expectedResponse,
                "the app's requests should hit the mock handler now instead of the real API");
        }
    }
}
