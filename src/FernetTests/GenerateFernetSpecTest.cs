using Fernet;
using FernetTests.Internal;
using Xunit;

namespace FernetTests
{
    public class GenerateFernetSpecTest
    {
        private const string JsonData = @"{ 
            token: 'gAAAAAAdwJ6wAAECAwQFBgcICQoLDA0ODy021cpGVWKZ_eEwCGM4BLLF_5CV9dOPmrhuVUPgJobwOz7JcbmrR64jVmpU4IwqDA==',
            now: '1985-10-26T01:20:00-07:00',
            iv: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15],
            src: 'hello',
            secret: 'cw_0x689RpI-jtRR7oE8h_eQsKImvJapLeSbXpwF4e4='
        }";

        [Fact]
        public void CanGenerateFernet()
        {
            // Arrange
            var data = JsonData.DeserializeObject<FernetTokenData>();
            var src64 = data.Src.ToBase64String();

            // Act
            var encoded = SimpleFernet.Encrypt(data.Secret.UrlSafe64Decode(), src64.UrlSafe64Decode(), data.Now, data.Iv);

            // Assert
            Assert.Equal(encoded, data.Token);
        }
    }
}
