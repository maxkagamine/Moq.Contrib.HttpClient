using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Language;
using Moq.Language.Flow;
using Moq.Protected;

namespace MaxKagamine.Moq.HttpClient
{
    using System.Net.Http;

    public class MockHttpMessageHandler : Mock<HttpMessageHandler>, IProtectedAsMock<HttpMessageHandler, IHttpMessageHandler>
    {
        public MockHttpMessageHandler()
        { }

        public MockHttpMessageHandler(MockBehavior mockBehavior) : base(mockBehavior)
        { }

        /// <summary>
        /// Creates a new <see cref="HttpClient" /> backed by this handler.
        /// </summary>
        public HttpClient CreateClient() => new HttpClient(Object, false);

        /// <summary>
        /// Specifies a setup matching any request.
        /// </summary>
        public ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupAnyRequest()
            => Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given predicate.
        /// </summary>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            Predicate<HttpRequestMessage> match)
            => Setup(x => x.SendAsync(RequestMatcher.Is(match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given predicate.
        /// </summary>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            Func<HttpRequestMessage, Task<bool>> match)
            => Setup(x => x.SendAsync(RequestMatcher.Is(match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given <see cref="Uri" />.
        /// </summary>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        public ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            Uri requestUri)
            => Setup(x => x.SendAsync(RequestMatcher.Is(requestUri), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given URL.
        /// </summary>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        public ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            string requestUrl)
            => Setup(x => x.SendAsync(RequestMatcher.Is(requestUrl), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            Uri requestUri, Predicate<HttpRequestMessage> match)
            => Setup(x => x.SendAsync(RequestMatcher.Is(requestUri, match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            Uri requestUri, Func<HttpRequestMessage, Task<bool>> match)
            => Setup(x => x.SendAsync(RequestMatcher.Is(requestUri, match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given URL as well as a predicate.
        /// </summary>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            string requestUrl, Predicate<HttpRequestMessage> match)
            => Setup(x => x.SendAsync(RequestMatcher.Is(requestUrl, match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given URL as well as a predicate.
        /// </summary>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            string requestUrl, Func<HttpRequestMessage, Task<bool>> match)
            => Setup(x => x.SendAsync(RequestMatcher.Is(requestUrl, match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given method and <see cref="Uri" />.
        /// </summary>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        public ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            HttpMethod method, Uri requestUri)
            => Setup(x => x.SendAsync(RequestMatcher.Is(method, requestUri), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given method and URL.
        /// </summary>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        public ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            HttpMethod method, string requestUrl)
            => Setup(x => x.SendAsync(RequestMatcher.Is(method, requestUrl), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given method and <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            HttpMethod method, Uri requestUri, Predicate<HttpRequestMessage> match)
            => Setup(x => x.SendAsync(RequestMatcher.Is(method, requestUri, match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given method and <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            HttpMethod method, Uri requestUri, Func<HttpRequestMessage, Task<bool>> match)
            => Setup(x => x.SendAsync(RequestMatcher.Is(method, requestUri, match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given method and URL as well as a predicate.
        /// </summary>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            HttpMethod method, string requestUrl, Predicate<HttpRequestMessage> match)
            => Setup(x => x.SendAsync(RequestMatcher.Is(method, requestUrl, match), It.IsAny<CancellationToken>()));

        /// <summary>
        /// Specifies a setup for a request matching the given method and URL as well as a predicate.
        /// </summary>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(
            HttpMethod method, string requestUrl, Func<HttpRequestMessage, Task<bool>> match)
            => Setup(x => x.SendAsync(RequestMatcher.Is(method, requestUrl, match), It.IsAny<CancellationToken>()));
    
        /// <summary>
        /// Verifies that any request was sent.
        /// </summary>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        public void VerifyAnyRequest(Times? times = null, string failMessage = null)
            => Verify(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given predicate.
        /// </summary>
        /// <param name="match">The predicate used to match the request.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public void VerifyRequest(
            Predicate<HttpRequestMessage> match, Times? times = null, string failMessage = null)
            => Verify(x => x.SendAsync(RequestMatcher.Is(match), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given predicate.
        /// </summary>
        /// <param name="match">The predicate used to match the request.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public void VerifyRequest(
            Func<HttpRequestMessage, Task<bool>> match, Times? times = null, string failMessage = null)
            => Verify(x => x.SendAsync(RequestMatcher.Is(match), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given <see cref="Uri" />.
        /// </summary>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        public void VerifyRequest(
            Uri requestUri, Times? times = null, string failMessage = null)
            => Verify(x => x.SendAsync(RequestMatcher.Is(requestUri), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given URL.
        /// </summary>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        public void VerifyRequest(
            string requestUrl, Times? times = null, string failMessage = null)
            => Verify(x => x.SendAsync(RequestMatcher.Is(requestUrl), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public void VerifyRequest(
            Uri requestUri, Predicate<HttpRequestMessage> match, Times? times = null, string failMessage = null)
            => Verify(x => x.SendAsync(RequestMatcher.Is(requestUri, match), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public void VerifyRequest(
            Uri requestUri, Func<HttpRequestMessage, Task<bool>> match, Times? times = null, string failMessage = null)
            => Verify(x => x.SendAsync(RequestMatcher.Is(requestUri, match), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given URL as well as a predicate.
        /// </summary>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public void VerifyRequest(
            string requestUrl, Predicate<HttpRequestMessage> match, Times? times = null, string failMessage = null)
            => Verify(x => x.SendAsync(RequestMatcher.Is(requestUrl, match), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given URL as well as a predicate.
        /// </summary>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public void VerifyRequest(
            string requestUrl, Func<HttpRequestMessage, Task<bool>> match, Times? times = null, string failMessage = null)
            => Verify(x => x.SendAsync(RequestMatcher.Is(requestUrl, match), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given method and <see cref="Uri" />.
        /// </summary>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        public void VerifyRequest(
            HttpMethod method, Uri requestUri, Times? times = null, string failMessage = null)
            => Verify(x => x.SendAsync(RequestMatcher.Is(method, requestUri), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given method and URL.
        /// </summary>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        public void VerifyRequest(
            HttpMethod method, string requestUrl, Times? times = null, string failMessage = null)
            => Verify(x => x.SendAsync(RequestMatcher.Is(method, requestUrl), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given method and <see cref="Uri" /> as well as a predicate.
        /// </summary>
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
        public void VerifyRequest(
            HttpMethod method, Uri requestUri, Predicate<HttpRequestMessage> match, Times? times = null, string failMessage = null)
            => Verify(x => x.SendAsync(RequestMatcher.Is(method, requestUri, match), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given method and <see cref="Uri" /> as well as a predicate.
        /// </summary>
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
        public void VerifyRequest(
            HttpMethod method, Uri requestUri, Func<HttpRequestMessage, Task<bool>> match, Times? times = null, string failMessage = null)
            => Verify(x => x.SendAsync(RequestMatcher.Is(method, requestUri, match), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given method and URL as well as a predicate.
        /// </summary>
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
        public void VerifyRequest(
            HttpMethod method, string requestUrl, Predicate<HttpRequestMessage> match, Times? times = null, string failMessage = null)
            => Verify(x => x.SendAsync(RequestMatcher.Is(method, requestUrl, match), It.IsAny<CancellationToken>()), times, failMessage);

        /// <summary>
        /// Verifies that a request was sent matching the given method and URL as well as a predicate.
        /// </summary>
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
        public void VerifyRequest(
            HttpMethod method, string requestUrl, Func<HttpRequestMessage, Task<bool>> match, Times? times = null, string failMessage = null)
            => Verify(x => x.SendAsync(RequestMatcher.Is(method, requestUrl, match), It.IsAny<CancellationToken>()), times, failMessage);

        #region IProtectedAsMock

        private IProtectedAsMock<HttpMessageHandler, IHttpMessageHandler> Protected => this.Protected().As<IHttpMessageHandler>();

        /// <summary>
        /// Specifies a setup on the mocked type for a call to a <see langword="void"/> method.
        /// </summary>
        /// <param name="expression">Lambda expression that specifies the expected method invocation.</param>
        /// <seealso cref="Mock{T}.Setup(Expression{Action{T}})"/>
        public ISetup<HttpMessageHandler> Setup(Expression<Action<IHttpMessageHandler>> expression) => Protected.Setup(expression);

        /// <summary>
        /// Specifies a setup on the mocked type for a call to a value-returning method.
        /// </summary>
        /// <typeparam name="TResult">Type of the return value. Typically omitted as it can be inferred from the expression.</typeparam>
        /// <param name="expression">Lambda expression that specifies the expected method invocation.</param>
        /// <seealso cref="Mock{T}.Setup{TResult}(Expression{Func{T, TResult}})"/>
        public ISetup<HttpMessageHandler, TResult> Setup<TResult>(Expression<Func<IHttpMessageHandler, TResult>> expression) => Protected.Setup(expression);

        /// <summary>
        /// Specifies a setup on the mocked type for a call to a property getter.
        /// </summary>
        /// <typeparam name="TProperty">Type of the property. Typically omitted as it can be inferred from the expression.</typeparam>
        /// <param name="expression">Lambda expression that specifies the property getter.</param>
        public ISetupGetter<HttpMessageHandler, TProperty> SetupGet<TProperty>(Expression<Func<IHttpMessageHandler, TProperty>> expression) => Protected.SetupGet(expression);

        /// <summary>
        /// Specifies that the given property should have "property behavior",
        /// meaning that setting its value will cause it to be saved and later returned when the property is requested.
        /// (This is also known as "stubbing".)
        /// </summary>
        /// <typeparam name="TProperty">Type of the property. Typically omitted as it can be inferred from the expression.</typeparam>
        /// <param name="expression">Lambda expression that specifies the property.</param>
        /// <param name="initialValue">Initial value for the property.</param>
        public Mock<HttpMessageHandler> SetupProperty<TProperty>(Expression<Func<IHttpMessageHandler, TProperty>> expression, TProperty initialValue = default(TProperty)) => Protected.SetupProperty(expression, initialValue);

        /// <summary>
        /// Return a sequence of values, once per call.
        /// </summary>
        /// <typeparam name="TResult">Type of the return value. Typically omitted as it can be inferred from the expression.</typeparam>
        /// <param name="expression">Lambda expression that specifies the expected method invocation.</param>
        public ISetupSequentialResult<TResult> SetupSequence<TResult>(Expression<Func<IHttpMessageHandler, TResult>> expression) => Protected.SetupSequence(expression);

        /// <summary>
        /// Performs a sequence of actions, one per call.
        /// </summary>
        /// <param name="expression">Lambda expression that specifies the expected method invocation.</param>
        public ISetupSequentialAction SetupSequence(Expression<Action<IHttpMessageHandler>> expression) => Protected.SetupSequence(expression);

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the mock.
        /// Use in conjunction with the default <see cref="MockBehavior.Loose"/>.
        /// </summary>
        /// <param name="expression">Lambda expression that specifies the method invocation.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        public void Verify(Expression<Action<IHttpMessageHandler>> expression, Times? times = null, string failMessage = null) => Protected.Verify(expression, times, failMessage);

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the mock.
        /// Use in conjunction with the default <see cref="MockBehavior.Loose"/>.
        /// </summary>
        /// <typeparam name="TResult">Type of the return value. Typically omitted as it can be inferred from the expression.</typeparam>
        /// <param name="expression">Lambda expression that specifies the method invocation.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        public void Verify<TResult>(Expression<Func<IHttpMessageHandler, TResult>> expression, Times? times = null, string failMessage = null) => Protected.Verify(expression, times, failMessage);

        /// <summary>
        /// Verifies that a property was read on the mock.
        /// </summary>
        /// <typeparam name="TProperty">Type of the property. Typically omitted as it can be inferred from the expression.</typeparam>
        /// <param name="expression">Lambda expression that specifies the method invocation.</param>
        /// <param name="times">
        /// Number of times that the invocation is expected to have occurred.
        /// If omitted, assumed to be <see cref="Times.AtLeastOnce"/>.
        /// </param>
        /// <param name="failMessage">Message to include in the thrown <see cref="MockException"/> if verification fails.</param>
        /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
        public void VerifyGet<TProperty>(Expression<Func<IHttpMessageHandler, TProperty>> expression, Times? times = null, string failMessage = null) => Protected.VerifyGet(expression, times, failMessage);

        #endregion
    }
}
