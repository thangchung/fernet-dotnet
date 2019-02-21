using Xunit;

namespace FernetTests
{
    public class VerifyFernetTest
    {
        private const string jsonData = @"{ 
            'token': 'gAAAAAAdwJ6wAAECAwQFBgcICQoLDA0ODy021cpGVWKZ_eEwCGM4BLLF_5CV9dOPmrhuVUPgJobwOz7JcbmrR64jVmpU4IwqDA==',
            'now': '1985-10-26T01:20:01-07:00',
            'ttlSec': 60,
            'src': 'hello',
            'secret': 'cw_0x689RpI-jtRR7oE8h_eQsKImvJapLeSbXpwF4e4='
        }";

        [Fact]
        public void CanVerifyFernetToken()
        {
            /*var data = JsonConvert.DeserializeObject<FernetTokenData>(jsonData); 
            byte[] key = "cw_0x689RpI-jtRR7oE8h_eQsKImvJapLeSbXpwF4e4=".Base64UrlDecode();
            DateTime timestamp;

            var result = SimpleFernet.DecryptFernet(key, data.Token, out timestamp);
            Assert.NotNull(result);*/
        }
    }
}
