using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace MyIdentityServer4.Customized
{
    public class MySecretValidator : ISecretValidator
    {
        public Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret)
        {
            // this is just for demo
            var success = new SecretValidationResult
            {
                IsError = false,
                Success = true
            };

            return Task.FromResult(success);
        }
    }
}
