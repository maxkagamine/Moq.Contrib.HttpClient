using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Moq.Contrib.HttpClient
{
    /// <summary>
    /// An interface to facilitate mocking the protected <see cref="HttpMessageHandler.SendAsync(HttpRequestMessage, CancellationToken)" /> method.
    /// </summary>
    public interface IHttpMessageHandler
    {
        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">The request was null.</exception>
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    }
}
