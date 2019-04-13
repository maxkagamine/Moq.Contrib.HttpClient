using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Moq.Language;
using Moq.Language.Flow;

namespace Moq.Contrib.HttpClient
{
    public static partial class MockHttpMessageHandlerExtensions
    {
        private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, Action<HttpResponseMessage> configure, HttpContent content = null, string mediaType = null)
        {
            var response = new HttpResponseMessage(statusCode);

            if (content != null)
            {
                if (mediaType != null)
                    content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

                response.Content = content;
            }

            configure?.Invoke(response);
            return response;
        }

        /// <summary>
        /// Specifies the response to return.
        /// </summary>
        /// <param name="setup">The setup.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="configure">An action to further configure the response such as setting headers.</param>
        public static IReturnsResult<HttpMessageHandler> ReturnsResponse(
            this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup,
            HttpStatusCode statusCode, Action<HttpResponseMessage> configure = null)
            => setup.ReturnsAsync(CreateResponse(statusCode, configure));

        /// <summary>
        /// Specifies the response to return in sequence.
        /// </summary>
        /// <param name="setup">The setup.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="configure">An action to further configure the response such as setting headers.</param>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsResponse(
            this ISetupSequentialResult<Task<HttpResponseMessage>> setup,
            HttpStatusCode statusCode, Action<HttpResponseMessage> configure = null)
            => setup.ReturnsAsync(CreateResponse(statusCode, configure));

        /// <summary>
        /// Specifies the response to return.
        /// </summary>
        /// <param name="setup">The setup.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="content">The response content.</param>
        /// <param name="configure">An action to further configure the response such as setting headers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
        public static IReturnsResult<HttpMessageHandler> ReturnsResponse(
            this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup,
            HttpStatusCode statusCode, HttpContent content, Action<HttpResponseMessage> configure = null)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            return setup.ReturnsAsync(CreateResponse(statusCode, configure, content));
        }

        /// <summary>
        /// Specifies the response to return in sequence.
        /// </summary>
        /// <param name="setup">The setup.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="content">The response content.</param>
        /// <param name="configure">An action to further configure the response such as setting headers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsResponse(
            this ISetupSequentialResult<Task<HttpResponseMessage>> setup,
            HttpStatusCode statusCode, HttpContent content, Action<HttpResponseMessage> configure = null)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            return setup.ReturnsAsync(CreateResponse(statusCode, configure, content));
        }

        /// <summary>
        /// Specifies the response to return, as <see cref="StringContent" />.
        /// </summary>
        /// <param name="setup">The setup.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="content">The response body.</param>
        /// <param name="mediaType">The media type. Defaults to text/plain.</param>
        /// <param name="encoding">The character encoding. Defaults to <see cref="Encoding.UTF8" />.</param>
        /// <param name="configure">An action to further configure the response such as setting headers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
        public static IReturnsResult<HttpMessageHandler> ReturnsResponse(
            this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup,
            HttpStatusCode statusCode, string content, string mediaType = null, Encoding encoding = null, Action<HttpResponseMessage> configure = null)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            return setup.ReturnsAsync(CreateResponse(statusCode, configure, new StringContent(content, encoding, mediaType)));
        }

        /// <summary>
        /// Specifies the response to return in sequence, as <see cref="StringContent" />.
        /// </summary>
        /// <param name="setup">The setup.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="content">The response body.</param>
        /// <param name="mediaType">The media type. Defaults to text/plain.</param>
        /// <param name="encoding">The character encoding. Defaults to <see cref="Encoding.UTF8" />.</param>
        /// <param name="configure">An action to further configure the response such as setting headers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsResponse(
            this ISetupSequentialResult<Task<HttpResponseMessage>> setup,
            HttpStatusCode statusCode, string content, string mediaType = null, Encoding encoding = null, Action<HttpResponseMessage> configure = null)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            return setup.ReturnsAsync(CreateResponse(statusCode, configure, new StringContent(content, encoding, mediaType)));
        }

        /// <summary>
        /// Specifies the response to return, as <see cref="StringContent" /> with <see cref="HttpStatusCode.OK" />.
        /// </summary>
        /// <param name="setup">The setup.</param>
        /// <param name="content">The response body.</param>
        /// <param name="mediaType">The media type. Defaults to text/plain.</param>
        /// <param name="encoding">The character encoding. Defaults to <see cref="Encoding.UTF8" />.</param>
        /// <param name="configure">An action to further configure the response such as setting headers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
        public static IReturnsResult<HttpMessageHandler> ReturnsResponse(
            this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup,
            string content, string mediaType = null, Encoding encoding = null, Action<HttpResponseMessage> configure = null)
            => setup.ReturnsResponse(HttpStatusCode.OK, content, mediaType, encoding, configure);

        /// <summary>
        /// Specifies the response to return in sequence, as <see cref="StringContent" /> with <see cref="HttpStatusCode.OK" />.
        /// </summary>
        /// <param name="setup">The setup.</param>
        /// <param name="content">The response body.</param>
        /// <param name="mediaType">The media type. Defaults to text/plain.</param>
        /// <param name="encoding">The character encoding. Defaults to <see cref="Encoding.UTF8" />.</param>
        /// <param name="configure">An action to further configure the response such as setting headers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsResponse(
            this ISetupSequentialResult<Task<HttpResponseMessage>> setup,
            string content, string mediaType = null, Encoding encoding = null, Action<HttpResponseMessage> configure = null)
            => setup.ReturnsResponse(HttpStatusCode.OK, content, mediaType, encoding, configure);

        /// <summary>
        /// Specifies the response to return, as <see cref="ByteArrayContent" />.
        /// </summary>
        /// <param name="setup">The setup.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="content">The response body.</param>
        /// <param name="mediaType">The media type.</param>
        /// <param name="configure">An action to further configure the response such as setting headers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
        public static IReturnsResult<HttpMessageHandler> ReturnsResponse(
            this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup,
            HttpStatusCode statusCode, byte[] content, string mediaType = null, Action<HttpResponseMessage> configure = null)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            return setup.ReturnsAsync(CreateResponse(statusCode, configure, new ByteArrayContent(content), mediaType));
        }

        /// <summary>
        /// Specifies the response to return in sequence, as <see cref="ByteArrayContent" />.
        /// </summary>
        /// <param name="setup">The setup.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="content">The response body.</param>
        /// <param name="mediaType">The media type.</param>
        /// <param name="configure">An action to further configure the response such as setting headers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsResponse(
            this ISetupSequentialResult<Task<HttpResponseMessage>> setup,
            HttpStatusCode statusCode, byte[] content, string mediaType = null, Action<HttpResponseMessage> configure = null)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            return setup.ReturnsAsync(CreateResponse(statusCode, configure, new ByteArrayContent(content), mediaType));
        }

        /// <summary>
        /// Specifies the response to return, as <see cref="ByteArrayContent" /> with <see cref="HttpStatusCode.OK" />.
        /// </summary>
        /// <param name="setup">The setup.</param>
        /// <param name="content">The response body.</param>
        /// <param name="mediaType">The media type.</param>
        /// <param name="configure">An action to further configure the response such as setting headers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
        public static IReturnsResult<HttpMessageHandler> ReturnsResponse(
            this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup,
            byte[] content, string mediaType = null, Action<HttpResponseMessage> configure = null)
            => setup.ReturnsResponse(HttpStatusCode.OK, content, mediaType, configure);

        /// <summary>
        /// Specifies the response to return in sequence, as <see cref="ByteArrayContent" /> with <see cref="HttpStatusCode.OK" />.
        /// </summary>
        /// <param name="setup">The setup.</param>
        /// <param name="content">The response body.</param>
        /// <param name="mediaType">The media type.</param>
        /// <param name="configure">An action to further configure the response such as setting headers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsResponse(
            this ISetupSequentialResult<Task<HttpResponseMessage>> setup,
            byte[] content, string mediaType = null, Action<HttpResponseMessage> configure = null)
            => setup.ReturnsResponse(HttpStatusCode.OK, content, mediaType, configure);

        /// <summary>
        /// Specifies the response to return, as <see cref="StreamContent" />.
        /// </summary>
        /// <param name="setup">The setup.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="content">The response body.</param>
        /// <param name="mediaType">The media type.</param>
        /// <param name="configure">An action to further configure the response such as setting headers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
        public static IReturnsResult<HttpMessageHandler> ReturnsResponse(
            this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup,
            HttpStatusCode statusCode, Stream content, string mediaType = null, Action<HttpResponseMessage> configure = null)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            return setup.ReturnsAsync(CreateResponse(statusCode, configure, new StreamContent(content), mediaType));
        }

        /// <summary>
        /// Specifies the response to return in sequence, as <see cref="StreamContent" />.
        /// </summary>
        /// <param name="setup">The setup.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="content">The response body.</param>
        /// <param name="mediaType">The media type.</param>
        /// <param name="configure">An action to further configure the response such as setting headers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsResponse(
            this ISetupSequentialResult<Task<HttpResponseMessage>> setup,
            HttpStatusCode statusCode, Stream content, string mediaType = null, Action<HttpResponseMessage> configure = null)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            return setup.ReturnsAsync(CreateResponse(statusCode, configure, new StreamContent(content), mediaType));
        }

        /// <summary>
        /// Specifies the response to return, as <see cref="StreamContent" /> with <see cref="HttpStatusCode.OK" />.
        /// </summary>
        /// <param name="setup">The setup.</param>
        /// <param name="content">The response body.</param>
        /// <param name="mediaType">The media type.</param>
        /// <param name="configure">An action to further configure the response such as setting headers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
        public static IReturnsResult<HttpMessageHandler> ReturnsResponse(
            this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup,
            Stream content, string mediaType = null, Action<HttpResponseMessage> configure = null)
            => setup.ReturnsResponse(HttpStatusCode.OK, content, mediaType, configure);

        /// <summary>
        /// Specifies the response to return in sequence, as <see cref="StreamContent" /> with <see cref="HttpStatusCode.OK" />.
        /// </summary>
        /// <param name="setup">The setup.</param>
        /// <param name="content">The response body.</param>
        /// <param name="mediaType">The media type.</param>
        /// <param name="configure">An action to further configure the response such as setting headers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsResponse(
            this ISetupSequentialResult<Task<HttpResponseMessage>> setup,
            Stream content, string mediaType = null, Action<HttpResponseMessage> configure = null)
            => setup.ReturnsResponse(HttpStatusCode.OK, content, mediaType, configure);
    }
}
