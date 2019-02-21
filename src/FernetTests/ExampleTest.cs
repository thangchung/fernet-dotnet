using Fernet;
using Xunit;

namespace FernetTests
{
    public class ExampleTest
    {
        [Fact]
        public void CanEncryptAndDecryptFernetToken()
        {
            var key = "cw_0x689RpI-jtRR7oE8h_eQsKImvJapLeSbXpwF4e4=".Base64UrlDecode();

            var token = SimpleFernet.EncryptFernet(key, "hello".GetBytes());
            var result = SimpleFernet.DecryptFernet(key, token, out var timestamp).GetString();

            Assert.Equal("hello", result);
        }
    }
}
