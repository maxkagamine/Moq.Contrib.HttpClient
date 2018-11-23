using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Polly;
using Xunit;

namespace MaxKagamine.Moq.HttpClient.Test
{
    public class SequenceExtensionsTests
    {
        private readonly Mock<HttpMessageHandler> handler;
        private readonly System.Net.Http.HttpClient client;

        public SequenceExtensionsTests()
        {
            handler = new Mock<HttpMessageHandler>();
            client = handler.CreateClient();

            client.BaseAddress = new Uri("https://example.com");
        }

        [Fact]
        public async Task CanReturnSequenceOfResponses()
        {
            var url = new Uri(client.BaseAddress, "value");

            // Simulate a service outage to test retry logic
            handler.SetupRequestSequence(url)
                .ReturnsResponse(HttpStatusCode.ServiceUnavailable)
                .ReturnsResponse(HttpStatusCode.ServiceUnavailable)
                .ReturnsResponse(HttpStatusCode.ServiceUnavailable)
                .ReturnsResponse("success");

            // Imaginary service method using Polly to handle retries
            // See https://github.com/App-vNext/Polly#polly for details
            async Task<string> GetValue()
            {
                var result = await Policy
                    .HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.ServiceUnavailable)
                    .RetryAsync(3)
                    .ExecuteAsync(() => client.GetAsync("value"));

                result.EnsureSuccessStatusCode();
                return await result.Content.ReadAsStringAsync();
            }

            // The method should try the request four times, receiving
            // a 503 the first three before it gets the success value
            string value = await GetValue();
            value.Should().Be("success");
            handler.VerifyRequest(url, Times.Exactly(4));
        }
    }
}
