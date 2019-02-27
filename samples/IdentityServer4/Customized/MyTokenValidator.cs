using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Fernet;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace MyIdentityServer4.Customized
{
    public class MyTokenValidator : ITokenValidator
    {
        public Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null)
        {
            // this key should store in the KeyVault service, then we can securely access in anywhere
            var key = "cw_0x689RpI-jtRR7oE8h_eQsKImvJapLeSbXpwF4e4=".UrlSafe64Decode();
            var jwt64Token = SimpleFernet.Decrypt(key, token, out var timestamp);
            var jwtToken = jwt64Token.UrlSafe64Encode().FromBase64String();

            if (string.IsNullOrEmpty(jwtToken))
                return Task.FromResult(new TokenValidationResult
                {
                    IsError = true
                });

            var result = new TokenValidationResult
            {
                IsError = false,
                Claims =  new List<Claim>
                {
                    new Claim(JwtClaimTypes.Scope, jwtToken)
                }
            };

            return Task.FromResult(result);
        }

        public Task<TokenValidationResult> ValidateRefreshTokenAsync(string token, Client client = null)
        {
            // this is just for demo
            throw new System.NotImplementedException();
        }

        public Task<TokenValidationResult> ValidateIdentityTokenAsync(string token, string clientId = null, bool validateLifetime = true)
        {
            // this is just for demo
            throw new System.NotImplementedException();
        }
    }
}
