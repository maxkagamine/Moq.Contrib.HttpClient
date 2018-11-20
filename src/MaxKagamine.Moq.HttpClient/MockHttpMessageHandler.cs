using System;
using System.Linq.Expressions;
using System.Net.Http;
using Moq;
using Moq.Language;
using Moq.Language.Flow;
using Moq.Protected;

namespace MaxKagamine.Moq.HttpClient
{
    public class MockHttpMessageHandler : Mock<HttpMessageHandler>, IProtectedAsMock<HttpMessageHandler, IHttpMessageHandler>
    {
        public MockHttpMessageHandler() : base(MockBehavior.Strict)
        { }

        public MockHttpMessageHandler(MockBehavior mockBehavior) : base(mockBehavior)
        { }

        #region IProtectedAsMock
        private IProtectedAsMock<HttpMessageHandler, IHttpMessageHandler> Protected => this.Protected().As<IHttpMessageHandler>();

        public ISetup<HttpMessageHandler> Setup(Expression<Action<IHttpMessageHandler>> expression) => Protected.Setup(expression);
        public ISetup<HttpMessageHandler, TResult> Setup<TResult>(Expression<Func<IHttpMessageHandler, TResult>> expression) => Protected.Setup(expression);
        public ISetupGetter<HttpMessageHandler, TProperty> SetupGet<TProperty>(Expression<Func<IHttpMessageHandler, TProperty>> expression) => Protected.SetupGet(expression);
        public Mock<HttpMessageHandler> SetupProperty<TProperty>(Expression<Func<IHttpMessageHandler, TProperty>> expression, TProperty initialValue = default(TProperty)) => Protected.SetupProperty(expression, initialValue);
        public ISetupSequentialResult<TResult> SetupSequence<TResult>(Expression<Func<IHttpMessageHandler, TResult>> expression) => Protected.SetupSequence(expression);
        public ISetupSequentialAction SetupSequence(Expression<Action<IHttpMessageHandler>> expression) => Protected.SetupSequence(expression);
        public void Verify(Expression<Action<IHttpMessageHandler>> expression, Times? times = null, string failMessage = null) => Protected.Verify(expression, times, failMessage);
        public void Verify<TResult>(Expression<Func<IHttpMessageHandler, TResult>> expression, Times? times = null, string failMessage = null) => Protected.Verify(expression, times, failMessage);
        public void VerifyGet<TProperty>(Expression<Func<IHttpMessageHandler, TProperty>> expression, Times? times = null, string failMessage = null) => Protected.VerifyGet(expression, times, failMessage);
        #endregion
    }
}
