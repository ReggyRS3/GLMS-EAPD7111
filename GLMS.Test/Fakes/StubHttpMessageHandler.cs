using System.Net;
using System.Text;

namespace GLMS.Test.Fakes
{
    public class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _json;

        public int CallCount { get; private set; }

        public StubHttpMessageHandler(HttpStatusCode statusCode, string json)
        {
            _statusCode = statusCode;
            _json = json;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_json, Encoding.UTF8, "application/json")
            });
        }
    }
}