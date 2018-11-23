using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace MaxKagamine.Moq.HttpClient.Test
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
        [InlineData(null, "miku", null, null)]
        [InlineData(HttpStatusCode.OK, "{\"name\":\"rin\"}", "application/json", null)]
        [InlineData(HttpStatusCode.Created, "<luka></luka><!--night fever-->", "text/xml", 932 /* Shift JIS */)]
        public async Task RespondsWithString(HttpStatusCode? statusCode, string content, string mediaType, int? encodingCodePage)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = encodingCodePage.HasValue ? Encoding.GetEncoding(encodingCodePage.Value) : null;

            if (statusCode.HasValue)
            {
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
            using (MemoryStream requestStream = new MemoryStream(bytes))
            {
                if (statusCode.HasValue)
                {
                    handler.SetupAnyRequest()
                        .ReturnsResponse(statusCode.Value, requestStream, mediaType);
                }
                else
                {
                    // Omitting status code defaults to OK
                    handler.SetupAnyRequest()
                        .ReturnsResponse(requestStream, mediaType);
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
            // All overloads take an action to further configure the response; this is more
            // useful than a Dictionary as it allows for using the typed header properties
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
    }
}
