using System.Collections.Specialized;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace MyIdentityServer4.Customized
{
    public class MyIntrospectionRequestValidator : IIntrospectionRequestValidator
    {
        private readonly ITokenValidator _tokenValidator;

        public MyIntrospectionRequestValidator(ITokenValidator tokenValidator)
        {
            _tokenValidator = tokenValidator;
        }
        public async Task<IntrospectionRequestValidationResult> ValidateAsync(NameValueCollection parameters, ApiResource api)
        {
            // this is just for demo
            var token = parameters.Get("token");
            var tokenValidationResult = await _tokenValidator.ValidateAccessTokenAsync(token);

            return await Task.FromResult(new IntrospectionRequestValidationResult
            {
                IsActive = true,
                IsError = false,
                Token = token,
                Claims = tokenValidationResult.Claims,
                Api = api,
                Parameters = parameters
            });
        }
    }
}
