using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Language;
using Moq.Language.Flow;
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
         * This is the latter; the former is under SetupRequestSequenceExtensions
         */

        /// <summary>
        /// Specifies a conditional setup for <see cref="IHttpMessageHandler.SendAsync(HttpRequestMessage, CancellationToken)" />
        /// by modifying the expression tree similar to <see cref="ProtectedAsMock{T, TAnalog}" />, as Moq does not currently
        /// support When() conditions or InSequence() in conjunction with Protected().
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock in the context of a When() or InSequence() condition.</param>
        /// <param name="expression">A lambda expression in the form of <c>x => x.SendAsync(...)</c>.</param>
        private static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> Setup(this ISetupConditionResult<HttpMessageHandler> handler, Expression<Func<IHttpMessageHandler, Task<HttpResponseMessage>>> expression)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            // Expression should be a method call
            if (!(expression.Body is MethodCallExpression methodCall))
                throw new ArgumentException("Expression is not a method call.", nameof(expression));

            // The method should be called on the interface parameter
            if (!(methodCall.Object is ParameterExpression left && left.Type == typeof(IHttpMessageHandler)))
                throw new ArgumentException("Object of method call is not the parameter.", nameof(expression));

            // The called method should be SendAsync
            if (methodCall.Method.Name != "SendAsync")
                throw new ArgumentException("Expression is not a SendAsync() method call.", nameof(expression));

            // Use reflection to get the protected method
            MethodInfo targetMethod = typeof(HttpMessageHandler).GetMethod("SendAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            // Replace the method call in the expression
            ParameterExpression targetParameter = Expression.Parameter(typeof(HttpMessageHandler), left.Name);
            MethodCallExpression targetMethodCall = Expression.Call(targetParameter, targetMethod, methodCall.Arguments);

            // Create a new lambda with this method call
            var rewrittenExpression = (Expression<Func<HttpMessageHandler, Task<HttpResponseMessage>>>) Expression.Lambda(targetMethodCall, targetParameter);

            // Use this lambda with the stock Setup() method
            return handler.Setup(rewrittenExpression);
        }

        /// <summary>
        /// Specifies a setup matching any request.
        /// </summary>
        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupAnyRequest(this ISetupConditionResult<HttpMessageHandler> handler)
            => handler.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            this ISetupConditionResult<HttpMessageHandler> handler, Predicate<HttpRequestMessage> match)
            => handler.Setup(x => x.SendAsync(RequestMatcher.Is(match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            this ISetupConditionResult<HttpMessageHandler> handler, Func<HttpRequestMessage, Task<bool>> match)
            => handler.Setup(x => x.SendAsync(RequestMatcher.Is(match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given <see cref="Uri" />.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            this ISetupConditionResult<HttpMessageHandler> handler, Uri requestUri)
            => handler.Setup(x => x.SendAsync(RequestMatcher.Is(requestUri), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given URL.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            this ISetupConditionResult<HttpMessageHandler> handler, string requestUrl)
            => handler.Setup(x => x.SendAsync(RequestMatcher.Is(requestUrl), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            this ISetupConditionResult<HttpMessageHandler> handler, Uri requestUri, Predicate<HttpRequestMessage> match)
            => handler.Setup(x => x.SendAsync(RequestMatcher.Is(requestUri, match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            this ISetupConditionResult<HttpMessageHandler> handler, Uri requestUri, Func<HttpRequestMessage, Task<bool>> match)
            => handler.Setup(x => x.SendAsync(RequestMatcher.Is(requestUri, match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given URL as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            this ISetupConditionResult<HttpMessageHandler> handler, string requestUrl, Predicate<HttpRequestMessage> match)
            => handler.Setup(x => x.SendAsync(RequestMatcher.Is(requestUrl, match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given URL as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            this ISetupConditionResult<HttpMessageHandler> handler, string requestUrl, Func<HttpRequestMessage, Task<bool>> match)
            => handler.Setup(x => x.SendAsync(RequestMatcher.Is(requestUrl, match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given method and <see cref="Uri" />.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            this ISetupConditionResult<HttpMessageHandler> handler, HttpMethod method, Uri requestUri)
            => handler.Setup(x => x.SendAsync(RequestMatcher.Is(method, requestUri), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given method and URL.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            this ISetupConditionResult<HttpMessageHandler> handler, HttpMethod method, string requestUrl)
            => handler.Setup(x => x.SendAsync(RequestMatcher.Is(method, requestUrl), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given method and <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            this ISetupConditionResult<HttpMessageHandler> handler, HttpMethod method, Uri requestUri, Predicate<HttpRequestMessage> match)
            => handler.Setup(x => x.SendAsync(RequestMatcher.Is(method, requestUri, match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given method and <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            this ISetupConditionResult<HttpMessageHandler> handler, HttpMethod method, Uri requestUri, Func<HttpRequestMessage, Task<bool>> match)
            => handler.Setup(x => x.SendAsync(RequestMatcher.Is(method, requestUri, match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given method and URL as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            this ISetupConditionResult<HttpMessageHandler> handler, HttpMethod method, string requestUrl, Predicate<HttpRequestMessage> match)
            => handler.Setup(x => x.SendAsync(RequestMatcher.Is(method, requestUrl, match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given method and URL as well as a predicate.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            this ISetupConditionResult<HttpMessageHandler> handler, HttpMethod method, string requestUrl, Func<HttpRequestMessage, Task<bool>> match)
            => handler.Setup(x => x.SendAsync(RequestMatcher.Is(method, requestUrl, match), It.IsAny<CancellationToken>()));
    }
}
