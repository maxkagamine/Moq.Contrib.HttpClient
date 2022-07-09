using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Moq.Contrib.HttpClient
{
    /// <summary>
    /// Memoizes the result of a `match` predicate for a given request to prevent the predicate from being called
    /// repeatedly, which may cause it to throw if the request content had been consumed and its stream closed.
    /// </summary>
    internal class RequestPredicate
    {
        private readonly Func<HttpRequestMessage, Task<bool>> match;
        private readonly ConditionalWeakTable<HttpRequestMessage, Task<bool>> memoizedResults = new ConditionalWeakTable<HttpRequestMessage, Task<bool>>();

        public RequestPredicate(Func<HttpRequestMessage, Task<bool>> match)
        {
            this.match = match ?? throw new ArgumentNullException(nameof(match));
        }

        public RequestPredicate(Predicate<HttpRequestMessage> match)
            : this(r => Task.FromResult(match(r)))
        { }

        public Task<bool> MatchesAsync(HttpRequestMessage request)
        {
            if (!memoizedResults.TryGetValue(request, out var result))
            {
                result = match(request);
                memoizedResults.Add(request, result);
            }

            return result;
        }

        public bool Matches(HttpRequestMessage request) => MatchesAsync(request).Result;
    }
}
