using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Moq.Contrib.HttpClient.Test
{
    public class ResponseExtensionsTests
    {
        private readonly Mock<HttpMessageHandler> handler;
        private readonly System.Net.Http.HttpClient client;

        public ResponseExtensionsTests()
        {
            handler = new Mock<HttpMessageHandler>();
            client = handler.CreateClient();

            client.BaseAddress = new Uri("https://example.com");
        }

        [Fact]
        public async Task CanSetHeaders()
        {
            // All overloads here take an action to further configure the response; this is
            // more useful than a Dictionary as it allows for using the typed header properties
            handler.SetupAnyRequest()
                .ReturnsResponse("jinrui ni eikou are", configure: response =>
                {
                    response.Headers.Server.Add(new ProductInfoHeaderValue("Bunker", null));
                    response.Headers.Add("X-Powered-By", "2B");
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "yorha.txt"
                    };
                });

            var res = await client.GetAsync("");
            var body = await res.Content.ReadAsStringAsync();

            body.Should().Be("jinrui ni eikou are", "glory to mankind");
            res.Headers.Server.Should().NotBeNull()
                .And.Subject.ToString().Should().Be("Bunker");
            res.Headers.GetValues("X-Powered-By").FirstOrDefault().Should().Be("2B");
            res.Content.Headers.ContentDisposition.Should().NotBeNull()
                .And.Subject.ToString().Should().Be("attachment; filename=yorha.txt");
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.Unauthorized)]
        public async Task RespondsWithStatusCode(HttpStatusCode statusCode)
        {
            // These tests primarily cover the response helpers; see the other
            // test class for the various request helpers besides SetupAnyRequest
            handler.SetupAnyRequest()
                .ReturnsResponse(statusCode);

            var response = await client.GetAsync("");

            response.StatusCode.Should().Be(statusCode);
        }

        [Theory]
        [InlineData(null, "miku", null, null)]
        [InlineData(HttpStatusCode.OK, "{\"name\":\"rin\"}", "application/json", null)]
        [InlineData(HttpStatusCode.Created, "<luka></luka><!--night fever-->", "text/xml", 932 /* Shift JIS */)]
        public async Task RespondsWithString(HttpStatusCode? statusCode, string content, string mediaType, int? encodingCodePage)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = encodingCodePage.HasValue ? Encoding.GetEncoding(encodingCodePage.Value) : null;

            if (statusCode.HasValue)
            {
                // When using a string, byte array, or stream, the status code, media
                // type (i.e. content type), and (for string) encoding are optional
                handler.SetupAnyRequest()
                    .ReturnsResponse(statusCode.Value, content, mediaType, encoding);
            }
            else
            {
                // Omitting status code defaults to OK
                handler.SetupAnyRequest()
                    .ReturnsResponse(content, mediaType, encoding);
            }

            var response = await client.GetAsync("");
            var responseString = await response.Content.ReadAsStringAsync();

            response.Content.Should().BeOfType<StringContent>();
            response.StatusCode.Should().Be(statusCode ?? HttpStatusCode.OK);
            responseString.Should().Be(content);

            var contentType = response.Content.Headers.ContentType;
            contentType.Should().NotBeNull("StringContent sets a default content type");
            contentType.MediaType.Should().Be(mediaType ?? "text/plain",
                mediaType == null ? "this is the default for StringContent" : "");
            contentType.CharSet.Should().Be((encoding ?? Encoding.UTF8).WebName,
                encoding == null ? "this is the default for StringContent" : "");
        }

        [Theory]
        [InlineData(null, new byte[] { 39, 39, 39, 39 }, "image/png")]
        [InlineData(HttpStatusCode.BadRequest, new byte[] { 39, 39 }, null)]
        public async Task RespondsWithBytes(HttpStatusCode? statusCode, byte[] content, string mediaType)
        {
            if (statusCode.HasValue)
            {
                handler.SetupAnyRequest()
                    .ReturnsResponse(statusCode.Value, content, mediaType);
            }
            else
            {
                // Omitting status code defaults to OK
                handler.SetupAnyRequest()
                    .ReturnsResponse(content, mediaType);
            }

            var response = await client.GetAsync("");
            var responseBytes = await response.Content.ReadAsByteArrayAsync();

            response.Content.Should().BeOfType<ByteArrayContent>();
            response.StatusCode.Should().Be(statusCode ?? HttpStatusCode.OK);
            responseBytes.Should().BeEquivalentTo(content);
            
            var responseMediaType = response.Content.Headers.ContentType?.MediaType;
            responseMediaType.Should().Be(mediaType);
        }

        [Theory]
        [InlineData(null, new byte[] { 39 }, null)]
        [InlineData(HttpStatusCode.InternalServerError, new byte[] { 39, 39, 39, 39 }, "audio/flac")]
        public async Task RespondsWithStream(HttpStatusCode? statusCode, byte[] bytes, string mediaType)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                if (statusCode.HasValue)
                {
                    handler.SetupAnyRequest()
                        .ReturnsResponse(statusCode.Value, stream, mediaType);
                }
                else
                {
                    // Omitting status code defaults to OK
                    handler.SetupAnyRequest()
                        .ReturnsResponse(stream, mediaType);
                }

                var response = await client.GetAsync("");
                var responseBytes = await response.Content.ReadAsByteArrayAsync();

                response.Content.Should().BeOfType<StreamContent>();
                response.StatusCode.Should().Be(statusCode ?? HttpStatusCode.OK);
                responseBytes.Should().BeEquivalentTo(bytes);
                
                var responseMediaType = response.Content.Headers.ContentType?.MediaType;
                responseMediaType.Should().Be(mediaType);
            }
        }

        [Fact]
        public async Task RespondsWithProvidedHttpContent()
        {
            var content = new MultipartContent();

            handler.SetupAnyRequest()
                .ReturnsResponse(HttpStatusCode.OK, content);

            var response = await client.GetAsync("");
            response.Content.Should().BeSameAs(content);
        }

        [Fact]
        public async Task ReturnsNewResponseInstanceEachRequest()
        {
            handler.SetupRequest(HttpMethod.Get, "https://example.com/foo") // Handler doesn't know about client's BaseAddress
                .ReturnsResponse("bar");

            var response1 = await client.GetAsync("foo");
            var response2 = await client.GetAsync("foo");
            var response3 = await client.GetAsync("foo");

            // New instances are returned for each request to ensure that subsequent requests don't receive a disposed
            // HttpResponseMessage or HttpContent
            (new[] { response1, response2, response3 }).Should()
                .OnlyHaveUniqueItems("each request should get its own response object");
            (new[] { response1.Content, response2.Content, response3.Content }).Should()
                .OnlyHaveUniqueItems("each response should have its own HttpContent object");
            (await response1.Content.ReadAsStringAsync()).Should().Be("bar");
            (await response2.Content.ReadAsStringAsync()).Should().Be("bar", "the HttpContent should not be disposed");
            (await response3.Content.ReadAsStringAsync()).Should().Be("bar", "the HttpContent should not be disposed");

            handler.VerifyRequest(HttpMethod.Get, "https://example.com/foo", Times.Exactly(3));
        }

        [Fact]
        public async Task StreamsReadFromSamePositionEachRequest()
        {
            var bytes = new byte[]
            {
                121, 111, 117, 116, 117, 98, 101, 46, 99, 111, 109, 47, 112, 108, 97, 121, 108, 105, 115, 116, 63, 108,
                105, 115, 116, 61, 80, 76, 89, 111, 111, 69, 65, 70, 85, 102, 104, 68, 102, 101, 118, 87, 70, 75, 76,
                97, 55, 103, 104, 51, 66, 111, 103, 66, 85, 65, 101, 98, 89, 79
            };

            int offsetStreamPosition = 39;
            byte[] expectedOffsetBytes = bytes.Skip(offsetStreamPosition).ToArray();

            using (MemoryStream stream = new MemoryStream(bytes))
            using (MemoryStream offsetStream = new MemoryStream(bytes))
            {
                handler.SetupRequest(HttpMethod.Get, "https://example.com/normal")
                    .ReturnsResponse(stream);

                // Multiple setups can share the same stream as well
                handler.SetupRequest(HttpMethod.Get, "https://example.com/normal2")
                    .ReturnsResponse(stream);

                // This stream is the same but seeked forward; each request should read from this position rather than
                // seeking back to the beginning
                offsetStream.Seek(offsetStreamPosition, SeekOrigin.Begin);
                handler.SetupRequest(HttpMethod.Get, "https://example.com/offset")
                    .ReturnsResponse(offsetStream);

                var responseBytes1 = await client.GetByteArrayAsync("normal");
                var responseBytes2 = await client.GetByteArrayAsync("normal");
                var responseBytes3 = await client.GetByteArrayAsync("normal2");

                var offsetResponseBytes1 = await client.GetByteArrayAsync("offset");
                var offsetResponseBytes2 = await client.GetByteArrayAsync("offset");

                responseBytes1.Should().BeEquivalentTo(bytes);
                responseBytes2.Should().BeEquivalentTo(bytes,
                    "the stream should be returned to its original position after being read");
                responseBytes3.Should().BeEquivalentTo(bytes,
                    "the stream should be reusable not just between requests to one setup but also between setups");

                offsetResponseBytes1.Should().BeEquivalentTo(expectedOffsetBytes,
                    "the stream should read from its initial (offset) position, not necessarily the beginning");
                offsetResponseBytes2.Should().BeEquivalentTo(expectedOffsetBytes,
                    "the stream should be returned to its original (offset, not zero) position after being read");
            }
        }
    }
}
