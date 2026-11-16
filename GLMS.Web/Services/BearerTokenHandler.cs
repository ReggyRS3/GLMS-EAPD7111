using System.Net.Http.Headers;

namespace GLMS.Web.Services
{
    public class BearerTokenHandler : DelegatingHandler
    {
        public const string AccessTokenClaim = "access_token";

        private readonly IHttpContextAccessor _accessor;

        public BearerTokenHandler(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // The token was saved in the login cookie when the user logged in
            var token = _accessor.HttpContext?.User.FindFirst(AccessTokenClaim)?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}