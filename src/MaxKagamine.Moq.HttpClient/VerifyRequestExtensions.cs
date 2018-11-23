using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;

namespace MaxKagamine.Moq.HttpClient
{
    public static partial class MockHttpMessageHandlerExtensions
    {
        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the mock.
        /// Use in conjunction with the default <see cref="MockBehavior.Loose"/>.
        /// </summary>
        /// <typeparam name="TResult">Type of the return value. Typically omitted as it can be inferred from the expression.</typeparam>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="expression">Lambda expression that specifies the method invocation.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        public static void Verify<TResult>(this Mock<HttpMessageHandler> handler, Expression<Func<IHttpMessageHandler, TResult>> expression, Times? times = null, string failMessage = null)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            handler.Protected().As<IHttpMessageHandler>().Verify(expression, times, failMessage);
        }

        /// <summary>
        /// Verifies that any request was sent.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        public static void VerifyAnyRequest(this Mock<HttpMessageHandler> handler, Times? times = null, string failMessage = null)
            => handler.Verify(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static void VerifyRequest(
            this Mock<HttpMessageHandler> handler, Predicate<HttpRequestMessage> match, Times? times = null, string failMessage = null)
            => handler.Verify(x => x.SendAsync(RequestMatcher.Is(match), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static void VerifyRequest(
            this Mock<HttpMessageHandler> handler, Func<HttpRequestMessage, Task<bool>> match, Times? times = null, string failMessage = null)
            => handler.Verify(x => x.SendAsync(RequestMatcher.Is(match), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given <see cref="Uri" />.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        public static void VerifyRequest(
            this Mock<HttpMessageHandler> handler, Uri requestUri, Times? times = null, string failMessage = null)
            => handler.Verify(x => x.SendAsync(RequestMatcher.Is(requestUri), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given URL.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        public static void VerifyRequest(
            this Mock<HttpMessageHandler> handler, string requestUrl, Times? times = null, string failMessage = null)
            => handler.Verify(x => x.SendAsync(RequestMatcher.Is(requestUrl), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static void VerifyRequest(
            this Mock<HttpMessageHandler> handler, Uri requestUri, Predicate<HttpRequestMessage> match, Times? times = null, string failMessage = null)
            => handler.Verify(x => x.SendAsync(RequestMatcher.Is(requestUri, match), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static void VerifyRequest(
            this Mock<HttpMessageHandler> handler, Uri requestUri, Func<HttpRequestMessage, Task<bool>> match, Times? times = null, string failMessage = null)
            => handler.Verify(x => x.SendAsync(RequestMatcher.Is(requestUri, match), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given URL as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static void VerifyRequest(
            this Mock<HttpMessageHandler> handler, string requestUrl, Predicate<HttpRequestMessage> match, Times? times = null, string failMessage = null)
            => handler.Verify(x => x.SendAsync(RequestMatcher.Is(requestUrl, match), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given URL as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static void VerifyRequest(
            this Mock<HttpMessageHandler> handler, string requestUrl, Func<HttpRequestMessage, Task<bool>> match, Times? times = null, string failMessage = null)
            => handler.Verify(x => x.SendAsync(RequestMatcher.Is(requestUrl, match), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given method and <see cref="Uri" />.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        public static void VerifyRequest(
            this Mock<HttpMessageHandler> handler, HttpMethod method, Uri requestUri, Times? times = null, string failMessage = null)
            => handler.Verify(x => x.SendAsync(RequestMatcher.Is(method, requestUri), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given method and URL.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        public static void VerifyRequest(
            this Mock<HttpMessageHandler> handler, HttpMethod method, string requestUrl, Times? times = null, string failMessage = null)
            => handler.Verify(x => x.SendAsync(RequestMatcher.Is(method, requestUrl), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given method and <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static void VerifyRequest(
            this Mock<HttpMessageHandler> handler, HttpMethod method, Uri requestUri, Predicate<HttpRequestMessage> match, Times? times = null, string failMessage = null)
            => handler.Verify(x => x.SendAsync(RequestMatcher.Is(method, requestUri, match), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given method and <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static void VerifyRequest(
            this Mock<HttpMessageHandler> handler, HttpMethod method, Uri requestUri, Func<HttpRequestMessage, Task<bool>> match, Times? times = null, string failMessage = null)
            => handler.Verify(x => x.SendAsync(RequestMatcher.Is(method, requestUri, match), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given method and URL as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static void VerifyRequest(
            this Mock<HttpMessageHandler> handler, HttpMethod method, string requestUrl, Predicate<HttpRequestMessage> match, Times? times = null, string failMessage = null)
            => handler.Verify(x => x.SendAsync(RequestMatcher.Is(method, requestUrl, match), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given method and URL as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static void VerifyRequest(
            this Mock<HttpMessageHandler> handler, HttpMethod method, string requestUrl, Func<HttpRequestMessage, Task<bool>> match, Times? times = null, string failMessage = null)
            => handler.Verify(x => x.SendAsync(RequestMatcher.Is(method, requestUrl, match), It.IsAny<CancellationToken>()), times, failMessage);
    }
}
