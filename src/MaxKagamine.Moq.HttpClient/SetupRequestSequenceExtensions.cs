using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Language;
using Moq.Protected;

namespace MaxKagamine.Moq.HttpClient
{
    public static partial class MockHttpMessageHandlerExtensions
    {
        /**
         * Moq has two types of sequences:
         * 1. SetupSequence() which creates one setup that returns values in sequence, and
         * 2. InSequence().Setup() which creates multiple setups under When() conditions
         *    to ensure that they only match in order
         *
         * This is the former; the latter is under SetupRequestWhenExtensions
         */

        /// <summary>
        /// Return a sequence of values, one per call.
        /// </summary>
        /// <typeparam name="TResult">Type of the return value. Typically omitted as it can be inferred from the expression.</typeparam>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="expression">Lambda expression that specifies the expected method invocation.</param>
        private static ISetupSequentialResult<TResult> SetupSequence<TResult>(this Mock<HttpMessageHandler> handler, Expression<Func<IHttpMessageHandler, TResult>> expression)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            return handler.Protected().As<IHttpMessageHandler>().SetupSequence(expression);
        }

        /// <summary>
        /// Specifies a setup matching any sequence of requests.
        /// </summary>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> SetupAnyRequestSequence(this Mock<HttpMessageHandler> handler)
            => handler.SetupSequence(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a sequence of requests matching the given predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> SetupRequestSequence(
            this Mock<HttpMessageHandler> handler, Predicate<HttpRequestMessage> match)
            => handler.SetupSequence(x => x.SendAsync(RequestMatcher.Is(match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a sequence of requests matching the given predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> SetupRequestSequence(
            this Mock<HttpMessageHandler> handler, Func<HttpRequestMessage, Task<bool>> match)
            => handler.SetupSequence(x => x.SendAsync(RequestMatcher.Is(match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a sequence of requests matching the given <see cref="Uri" />.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> SetupRequestSequence(
            this Mock<HttpMessageHandler> handler, Uri requestUri)
            => handler.SetupSequence(x => x.SendAsync(RequestMatcher.Is(requestUri), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a sequence of requests matching the given URL.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> SetupRequestSequence(
            this Mock<HttpMessageHandler> handler, string requestUrl)
            => handler.SetupSequence(x => x.SendAsync(RequestMatcher.Is(requestUrl), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a sequence of requests matching the given <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> SetupRequestSequence(
            this Mock<HttpMessageHandler> handler, Uri requestUri, Predicate<HttpRequestMessage> match)
            => handler.SetupSequence(x => x.SendAsync(RequestMatcher.Is(requestUri, match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a sequence of requests matching the given <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> SetupRequestSequence(
            this Mock<HttpMessageHandler> handler, Uri requestUri, Func<HttpRequestMessage, Task<bool>> match)
            => handler.SetupSequence(x => x.SendAsync(RequestMatcher.Is(requestUri, match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a sequence of requests matching the given URL as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> SetupRequestSequence(
            this Mock<HttpMessageHandler> handler, string requestUrl, Predicate<HttpRequestMessage> match)
            => handler.SetupSequence(x => x.SendAsync(RequestMatcher.Is(requestUrl, match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a sequence of requests matching the given URL as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> SetupRequestSequence(
            this Mock<HttpMessageHandler> handler, string requestUrl, Func<HttpRequestMessage, Task<bool>> match)
            => handler.SetupSequence(x => x.SendAsync(RequestMatcher.Is(requestUrl, match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a sequence of requests matching the given method and <see cref="Uri" />.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> SetupRequestSequence(
            this Mock<HttpMessageHandler> handler, HttpMethod method, Uri requestUri)
            => handler.SetupSequence(x => x.SendAsync(RequestMatcher.Is(method, requestUri), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a sequence of requests matching the given method and URL.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> SetupRequestSequence(
            this Mock<HttpMessageHandler> handler, HttpMethod method, string requestUrl)
            => handler.SetupSequence(x => x.SendAsync(RequestMatcher.Is(method, requestUrl), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a sequence of requests matching the given method and <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> SetupRequestSequence(
            this Mock<HttpMessageHandler> handler, HttpMethod method, Uri requestUri, Predicate<HttpRequestMessage> match)
            => handler.SetupSequence(x => x.SendAsync(RequestMatcher.Is(method, requestUri, match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a sequence of requests matching the given method and <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> SetupRequestSequence(
            this Mock<HttpMessageHandler> handler, HttpMethod method, Uri requestUri, Func<HttpRequestMessage, Task<bool>> match)
            => handler.SetupSequence(x => x.SendAsync(RequestMatcher.Is(method, requestUri, match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a sequence of requests matching the given method and URL as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> SetupRequestSequence(
            this Mock<HttpMessageHandler> handler, HttpMethod method, string requestUrl, Predicate<HttpRequestMessage> match)
            => handler.SetupSequence(x => x.SendAsync(RequestMatcher.Is(method, requestUrl, match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a sequence of requests matching the given method and URL as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static ISetupSequentialResult<Task<HttpResponseMessage>> SetupRequestSequence(
            this Mock<HttpMessageHandler> handler, HttpMethod method, string requestUrl, Func<HttpRequestMessage, Task<bool>> match)
            => handler.SetupSequence(x => x.SendAsync(RequestMatcher.Is(method, requestUrl, match), It.IsAny<CancellationToken>()));
    }
}
