using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;

namespace MaxKagamine.Moq.HttpClient
{
    /// <summary>
    /// Custom Moq matchers for <see cref="HttpRequestMessage" /> using <see cref="Match.Create{T}(Predicate{T})" />.
    /// </summary>
    public static class RequestMatcher
    {
        /// <summary>
        /// A request matching the given predicate.
        /// </summary>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static HttpRequestMessage Is(Predicate<HttpRequestMessage> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            return Match.Create(match, () => Is(match));
        }

        /// <summary>
        /// A request matching the given predicate.
        /// </summary>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static HttpRequestMessage Is(Func<HttpRequestMessage, Task<bool>> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            return Match.Create(r => match(r).Result, () => Is(match)); // Blocking
        }

        /// <summary>
        /// A request matching the given <see cref="Uri" />.
        /// </summary>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        public static HttpRequestMessage Is(Uri requestUri)
            => Match.Create(r => r.RequestUri == requestUri, () => Is(requestUri));

        /// <summary>
        /// A request matching the given URL.
        /// </summary>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        public static HttpRequestMessage Is(string requestUrl)
            => Is(new Uri(requestUrl));

        /// <summary>
        /// A request matching the given <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static HttpRequestMessage Is(Uri requestUri, Predicate<HttpRequestMessage> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            return Match.Create(
                r => r.RequestUri == requestUri && match(r),
                () => Is(requestUri, match));
        }

        /// <summary>
        /// A request matching the given <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static HttpRequestMessage Is(Uri requestUri, Func<HttpRequestMessage, Task<bool>> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            return Match.Create(
                r => r.RequestUri == requestUri && match(r).Result, // Blocking
                () => Is(requestUri, match));
        }

        /// <summary>
        /// A request matching the given URL as well as a predicate.
        /// </summary>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static HttpRequestMessage Is(string requestUrl, Predicate<HttpRequestMessage> match)
            => Is(new Uri(requestUrl), match);

        /// <summary>
        /// A request matching the given URL as well as a predicate.
        /// </summary>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static HttpRequestMessage Is(string requestUrl, Func<HttpRequestMessage, Task<bool>> match)
            => Is(new Uri(requestUrl), match);

        /// <summary>
        /// A request matching the given method and <see cref="Uri" />.
        /// </summary>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        public static HttpRequestMessage Is(HttpMethod method, Uri requestUri)
            => Match.Create(
                r => r.Method == method && r.RequestUri == requestUri,
                () => Is(method, requestUri));

        /// <summary>
        /// A request matching the given method and URL.
        /// </summary>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        public static HttpRequestMessage Is(HttpMethod method, string requestUrl)
            => Is(method, new Uri(requestUrl));

        /// <summary>
        /// A request matching the given method and <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static HttpRequestMessage Is(HttpMethod method, Uri requestUri, Predicate<HttpRequestMessage> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            return Match.Create(
                r => r.Method == method && r.RequestUri == requestUri && match(r),
                () => Is(method, requestUri, match));
        }

        /// <summary>
        /// A request matching the given method and <see cref="Uri" /> as well as a predicate.
        /// </summary>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static HttpRequestMessage Is(HttpMethod method, Uri requestUri, Func<HttpRequestMessage, Task<bool>> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            return Match.Create(
                r => r.Method == method && r.RequestUri == requestUri && match(r).Result, // Blocking
                () => Is(method, requestUri, match));
        }

        /// <summary>
        /// A request matching the given method and URL as well as a predicate.
        /// </summary>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static HttpRequestMessage Is(HttpMethod method, string requestUrl, Predicate<HttpRequestMessage> match)
            => Is(method, new Uri(requestUrl), match);

        /// <summary>
        /// A request matching the given method and URL as well as a predicate.
        /// </summary>
        /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
        /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
        /// <param name="match">The predicate used to match the request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public static HttpRequestMessage Is(HttpMethod method, string requestUrl, Func<HttpRequestMessage, Task<bool>> match)
            => Is(method, new Uri(requestUrl), match);
    }
}
