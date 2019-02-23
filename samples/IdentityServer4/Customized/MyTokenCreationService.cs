using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Fernet;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace MyIdentityServer4.Customized
{
    /// <summary>
    /// https://github.com/IdentityServer/IdentityServer4/blob/63a50d7838af25896fbf836ea4e4f37b5e179cd8/src/Services/Default/DefaultTokenCreationService.cs
    /// https://github.com/IdentityServer/IdentityServer4/blob/63a50d7838af25896fbf836ea4e4f37b5e179cd8/src/Validation/Default/TokenValidator.cs
    /// https://github.com/IdentityServer/IdentityServer4/issues/1847
    /// https://github.com/IdentityServer/IdentityServer4/issues/1781
    /// https://sikora.humanaction.eu/multiple-authentication-services-using-identityserver4-with-net-core-2-0/
    /// /// </summary>
    public class MyTokenCreationService : DefaultTokenCreationService
    {
        public MyTokenCreationService(ISystemClock clock, IKeyMaterialService keys, ILogger<DefaultTokenCreationService> logger)
            : base(clock, keys, logger)
        {
        }

        protected override async Task<string> CreateJwtAsync(JwtSecurityToken jwt)
        {
            var jwtToken = await base.CreateJwtAsync(jwt);
            var jwt64Token = jwtToken.ToBase64String();

            // this key should store in the KeyVault service, then we can securely access in anywhere
            var key = "cw_0x689RpI-jtRR7oE8h_eQsKImvJapLeSbXpwF4e4=".UrlSafe64Decode();
            var fernetToken = SimpleFernet.Encrypt(key, jwt64Token.UrlSafe64Decode());

            return fernetToken;
        }
    }
}
