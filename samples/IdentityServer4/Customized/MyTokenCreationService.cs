using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Fernet;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace MyIdentityServer4.Customized
{
    public class MyTokenCreationService : DefaultTokenCreationService
    {
        public MyTokenCreationService(ISystemClock clock, IKeyMaterialService keys, ILogger<DefaultTokenCreationService> logger)
            : base(clock, keys, logger)
        {
        }

        protected override Task<string> CreateJwtAsync(JwtSecurityToken jwt)
        {
            // this is just for demo so that we will put the required scope into the fernet token
            var jwtToken = "api1";
            var jwt64Token = jwtToken.ToBase64String();

            // this key should store in the KeyVault service, then we can securely access in anywhere
            var key = "cw_0x689RpI-jtRR7oE8h_eQsKImvJapLeSbXpwF4e4=".UrlSafe64Decode();
            var fernetToken = SimpleFernet.Encrypt(key, jwt64Token.UrlSafe64Decode());

            return Task.FromResult(fernetToken);
        }
    }
}
