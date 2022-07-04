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

            // Setting a BaseAddress only affects HttpClient's methods; the handler doesn't know about the BaseAddress
            // and will receive the resolved url, so setups still need to specify the full request url
            client.BaseAddress = new Uri("https://example.com");
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.Unauthorized)]
        public async Task RespondsWithStatusCode(HttpStatusCode statusCode)
        {
            handler.SetupAnyRequest()
                .ReturnsResponse(statusCode);

            var response = await client.GetAsync("");

            response.StatusCode.Should().Be(statusCode);
        }

        [Theory]
        [InlineData(null, "foo", null, null)]
        [InlineData(HttpStatusCode.OK, @"{ ""foo"": ""bar"" }", "application/json", null)]
        [InlineData(HttpStatusCode.Created, "<foo>bar</foo>", "text/xml", 932 /* Shift JIS */)]
        public async Task RespondsWithString(HttpStatusCode? statusCode, string content, string mediaType, int? encodingCodePage)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = encodingCodePage.HasValue ? Encoding.GetEncoding(encodingCodePage.Value) : null;

            if (statusCode.HasValue)
            {
                // Media/content type and encoding are optional; StringContent defaults these to text/plain and UTF-8
                handler.SetupAnyRequest()
                    .ReturnsResponse(statusCode.Value, content, mediaType, encoding);
            }
            else
            {
                // Status code can be omitted and defaults to OK
                handler.SetupAnyRequest()
                    .ReturnsResponse(content, mediaType, encoding);
            }

            var response = await client.GetAsync("");
            var responseString = await response.Content.ReadAsStringAsync();

            response.Content.Should().BeOfType<StringContent>();
            response.StatusCode.Should().Be(statusCode ?? HttpStatusCode.OK);
            responseString.Should().Be(content);

            var contentType = response.Content.Headers.ContentType;
            contentType.MediaType.Should().Be(mediaType ?? "text/plain");
            contentType.CharSet.Should().Be((encoding ?? Encoding.UTF8).WebName);
        }

        [Theory]
        [InlineData(null, new byte[] { 39, 39, 39, 39 }, "image/png")]
        [InlineData(HttpStatusCode.BadRequest, new byte[] { }, null)]
        public async Task RespondsWithBytes(HttpStatusCode? statusCode, byte[] content, string mediaType)
        {
            if (statusCode.HasValue)
            {
                handler.SetupAnyRequest()
                    .ReturnsResponse(statusCode.Value, content, mediaType);
            }
            else
            {
                // Status code can be omitted and defaults to OK
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
        [InlineData(null, new byte[] { 39, 39, 39, 39 }, "image/png")]
        [InlineData(HttpStatusCode.BadRequest, new byte[] { }, null)]
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
                    // Status code can be omitted and defaults to OK
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
        public async Task CanSetHeaders()
        {
            // All overloads take an action to configure the response and set custom headers; this is more
            // useful than a Dictionary as it allows for using the typed header properties
            handler.SetupAnyRequest()
                .ReturnsResponse("response body", configure: response =>
                {
                    response.Headers.Server.Add(new ProductInfoHeaderValue("Nginx", null));
                    response.Headers.Add("X-Powered-By", "ASP.NET");
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "example.txt"
                    };
                });

            var res = await client.GetAsync("");
            var body = await res.Content.ReadAsStringAsync();
            var server = res.Headers.Server?.ToString();
            var poweredBy = res.Headers.GetValues("X-Powered-By").FirstOrDefault();
            var contentDisposition = res.Content.Headers.ContentDisposition?.ToString();

            body.Should().Be("response body");
            server.Should().Be("Nginx");
            poweredBy.Should().Be("ASP.NET");
            contentDisposition.Should().Be("attachment; filename=example.txt");
        }

        [Fact]
        public async Task CanSimulateNetworkErrors()
        {
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = handler.CreateClient();

            // Triggering a network error (e.g. connection refused) can be done using the standard Throws()
            handler.SetupAnyRequest()
                .Throws<HttpRequestException>();

            // Fancier version:
            //var inner = new System.Net.Sockets.SocketException((int)System.Net.Sockets.SocketError.ConnectionRefused);
            //handler.SetupAnyRequest()
            //    .Throws(new HttpRequestException(inner.Message, inner));

            Func<Task> attempt = () => client.GetAsync("http://example.com");
            await attempt.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task ReturnsNewResponseInstanceEachRequest()
        {
            handler.SetupRequest(HttpMethod.Get, "https://example.com/foo")
                .ReturnsResponse("bar");

            var response1 = await client.GetAsync("foo");
            var response2 = await client.GetAsync("foo");

            // New instances are returned for each request to ensure that subsequent requests don't receive a disposed
            // HttpResponseMessage or HttpContent
            response2.Should().NotBeSameAs(response1, "each request should get its own response object");
            response2.Content.Should().NotBeSameAs(response1.Content, "each response should have its own content object");

            // HttpClient.GetStringAsync() wraps the HttpResponseMessage in a `using` (up until at least .NET 5) which
            // would cause the second attempt to read the content to throw if it were given the same response object
            (await client.GetStringAsync("foo")).Should().Be("bar");
            (await client.GetStringAsync("foo")).Should().Be("bar", "the HttpContent should not be disposed");

            handler.VerifyRequest(HttpMethod.Get, "https://example.com/foo", Times.Exactly(4));
        }

        [Fact]
        public async Task StreamsReadFromSamePositionEachRequest()
        {
            // ReturnsResponse includes overloads that take a stream. If the stream is seekable (such as a MemoryStream),
            // it will be wrapped so that each request maintains an independent stream position. This allows multiple
            // requests (and setups) to read from the same stream without interfering with each other. The wrapper also
            // prevents a disposing HttpContent from closing the underlying stream. If you have a non-seekable stream
            // that needs to be used in multiple responses, copy it to a MemoryStream or byte array first.
            var bytes = new byte[]
            {
                0x79, 0x6F, 0x75, 0x74, 0x75, 0x62, 0x65, 0x2E, 0x63, 0x6F, 0x6D, 0x2F, 0x70, 0x6C, 0x61, 0x79,
                0x6C, 0x69, 0x73, 0x74, 0x3F, 0x6C, 0x69, 0x73, 0x74, 0x3D, 0x50, 0x4C, 0x59, 0x6F, 0x6F, 0x45,
                0x41, 0x46, 0x55, 0x66, 0x68, 0x44, 0x66, 0x65, 0x76, 0x57, 0x46, 0x4B, 0x4C, 0x61, 0x37, 0x67,
                0x68, 0x33, 0x42, 0x6F, 0x67, 0x42, 0x55, 0x41, 0x65, 0x62, 0x59, 0x4F
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
