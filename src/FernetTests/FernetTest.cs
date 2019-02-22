using Fernet;
using Xunit;

namespace FernetTests
{
    public class FernetTest
    {
        [Fact]
        public void CanEncodeDecode()
        {
            // Arrange
            var key = SimpleFernet.GenerateKey().UrlSafe64Decode();
            var src = "hello";
            var src64 = src.ToBase64String();

            // Act
            var token = SimpleFernet.Encrypt(key, src64.UrlSafe64Decode());
            var decoded64 = SimpleFernet.Decrypt(key, token, out var timestamp);
            var decoded = decoded64.UrlSafe64Encode().FromBase64String();

            // Assert
            Assert.Equal(src, decoded);
        }

        [Fact]
        public void CanEncodeDecodeArrayMessage()
        {
            // Arrange
            var key = SimpleFernet.GenerateKey().UrlSafe64Decode();
            var src = "[ 'id': '123456' ]";
            var src64 = src.ToBase64String();

            // Act
            var token = SimpleFernet.Encrypt(key, src64.UrlSafe64Decode());
            var decoded64 = SimpleFernet.Decrypt(key, token, out var timestamp);
            var decoded = decoded64.UrlSafe64Encode().FromBase64String();

            // Assert
            Assert.Equal(src, decoded);
        }
    }
}
