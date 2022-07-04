using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Polly;
using Xunit;

namespace Moq.Contrib.HttpClient.Test
{
    public class SequenceExtensionsTests
    {
        private readonly Mock<HttpMessageHandler> handler;
        private readonly System.Net.Http.HttpClient client;

        public SequenceExtensionsTests()
        {
            handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            client = handler.CreateClient();

            client.BaseAddress = new Uri("https://example.com");
        }

        /**
         * Moq has two types of sequences:
         * 1. SetupSequence() which creates one setup that returns values in sequence, and
         * 2. InSequence().Setup() which creates multiple setups under When() conditions
         *    to ensure that they only match in order
         */

        [Fact]
        public async Task CanReturnSequenceOfResponsesForOneSetup()
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CanDefineMultipleSetupsToMatchInSequence(bool doItCorrectly)
        {
            // For a case where separate requests independent of each other must be made in a certain order, their
            // setups can be defined in a sequence such that one must match before the other. (An alternative approach
            // would be to inspect the Invocations list after the fact.)

            // For example, when dialing the Stargate, Walter likes to say "chevron one encoded" and so on up to the
            // seventh, at which point he changes things up and says "chevron seven locked". Therefore we must be sure
            // to only "encode" six times and then finally "lock" only at the end of the sequence.

            var encodeUrl = new Uri(client.BaseAddress, "chevrons/encode");
            var lockUrl = new Uri(client.BaseAddress, "chevrons/lock");
            var sequence = new MockSequence() { Cyclic = true };

            const string PointOfOrigin = "ᐰ";

            for (int i = 1; i <= 6; i++)
            {
                handler.InSequence(sequence)
                    .SetupRequest(HttpMethod.Post, encodeUrl)
                    .ReturnsResponse($"Chevron {i} encoded!");
            }

            handler.InSequence(sequence)
                .SetupRequest(HttpMethod.Post, lockUrl, async r =>
                    (await r.Content.ReadAsStringAsync()) == PointOfOrigin)
                .ReturnsResponse("Chevron 7, locked!");

            async Task<string> PostAndReadString(Uri uri, string content = "")
            {
                // Even though this is disposing of each response, on the second cycle it will get new response
                // instances (another test of ResponseExtensionsTests.ReturnsNewResponseInstanceEachRequest)
                using (var response = await client.PostAsync(uri, new StringContent(content)))
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }

            Func<Task> dialItUp = async () =>
            {
                (await PostAndReadString(encodeUrl)).Should().Be("Chevron 1 encoded!");
                (await PostAndReadString(encodeUrl)).Should().Be("Chevron 2 encoded!");
                (await PostAndReadString(encodeUrl)).Should().Be("Chevron 3 encoded!");

                if (!doItCorrectly)
                {
                    // Walter you silly
                    (await PostAndReadString(lockUrl, PointOfOrigin)).Should().Be("Chevron 7, locked!");

                    // We should not reach this point. Also, https://youtu.be/Qfgdlw1Z88I?t=360
                    throw new InvalidOperationException("Colonel O'Neill, what the hell are you doing?!");
                }

                (await PostAndReadString(encodeUrl)).Should().Be("Chevron 4 encoded!");
                (await PostAndReadString(encodeUrl)).Should().Be("Chevron 5 encoded!");
                (await PostAndReadString(encodeUrl)).Should().Be("Chevron 6 encoded!");

                (await PostAndReadString(lockUrl, PointOfOrigin)).Should().Be("Chevron 7, locked!");
            };

            if (doItCorrectly)
            {
                await dialItUp.Should().NotThrowAsync();

                // Do it again! Unlikely anyone will actually need to use Cyclic, but it works.
                await dialItUp.Should().NotThrowAsync("we can repeat the sequence with Cyclic = true");
            }
            else
            {
                await dialItUp.Should().ThrowAsync<MockException>();
            }
        }
    }
}
