using System;
using System.ComponentModel;
using Moq;

namespace MaxKagamine.Moq.HttpClient
{
    using System.Net.Http;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static partial class MockHttpMessageHandlerExtensions
    {
        /// <summary>
        /// Creates a new <see cref="HttpClient" /> backed by this handler.
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        public static HttpClient CreateClient(this Mock<HttpMessageHandler> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            return new HttpClient(handler.Object, false);
        }
    }
}
