using Fernet;
using FernetTests.Internal;
using Xunit;

namespace FernetTests
{
    public class VerifyFernetTest
    {
        private const string JsonData = @"{ 
            'token': 'gAAAAAAdwJ6wAAECAwQFBgcICQoLDA0ODy021cpGVWKZ_eEwCGM4BLLF_5CV9dOPmrhuVUPgJobwOz7JcbmrR64jVmpU4IwqDA==',
            'now': '1985-10-26T01:20:01-07:00',
            'ttlSec': 60,
            'src': 'hello',
            'secret': 'cw_0x689RpI-jtRR7oE8h_eQsKImvJapLeSbXpwF4e4='
        }";

        [Fact]
        public void CanVerifyFernetToken()
        {
            // Arrange
            var data = JsonData.DeserializeObject<FernetTokenData>();

            // Act
            var resultByte =
                SimpleFernet.Decrypt(
                    data.Secret.UrlSafe64Decode(),
                    data.Token,
                    out var timestamp);
            var result = resultByte.UrlSafe64Encode().FromBase64String();

            // Assert
            Assert.Equal(result, data.Src);
        }
    }
}
