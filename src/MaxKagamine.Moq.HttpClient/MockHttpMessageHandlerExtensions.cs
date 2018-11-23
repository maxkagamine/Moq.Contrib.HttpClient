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

        /// <summary>
        /// <para>
        ///   Creates an <see cref="IHttpClientFactory" /> that returns new
        ///   <see cref="HttpClient" /> instances backed by this handler.
        /// </para>
        /// <para>
        ///   To configure a named client, use <see cref="Mock.Get{T}(T)" />
        ///   to retrieve the mock and add additional setups.
        /// </para>
        /// </summary>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
        public static IHttpClientFactory CreateClientFactory(this Mock<HttpMessageHandler> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var mock = new Mock<IHttpClientFactory>();

            mock.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(() => handler.CreateClient());

            return mock.Object;
        }
    }
}
