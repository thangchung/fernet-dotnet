using System;
using System.Collections.Generic;
using Fernet;
using FernetTests.Internal;
using Xunit;

namespace FernetTests
{
    public class InvalidFernetTest
    {
        private const string JsonData = @"[
            {
                'desc': 'incorrect mac',
                'token': 'gAAAAAAdwJ6xAAECAwQFBgcICQoLDA0OD3HkMATM5lFqGaerZ-fWPAl1-szkFVzXTuGb4hR8AKtwcaX1YdykQUFBQUFBQUFBQQ==',
                'now': '1985-10-26T01:20:01-07:00',
                'ttlSec': 60,
                'secret': 'cw_0x689RpI-jtRR7oE8h_eQsKImvJapLeSbXpwF4e4='
            },
            {
                'desc': 'too short',
                'token': 'gAAAAAAdwJ6xAAECAwQFBgcICQoLDA0OD3HkMATM5lFqGaerZ-fWPA==',
                'now': '1985-10-26T01:20:01-07:00',
                'ttlSec': 60,
                'secret': 'cw_0x689RpI-jtRR7oE8h_eQsKImvJapLeSbXpwF4e4='
            },
            {
                'desc': 'invalid base64',
                'token': '%%%%%%%%%%%%%AECAwQFBgcICQoLDA0OD3HkMATM5lFqGaerZ-fWPAl1-szkFVzXTuGb4hR8AKtwcaX1YdykRtfsH-p1YsUD2Q==',
                'now': '1985-10-26T01:20:01-07:00',
                'ttlSec': 60,
                'secret': 'cw_0x689RpI-jtRR7oE8h_eQsKImvJapLeSbXpwF4e4='
            },
            {
                'desc': 'payload size not multiple of block size',
                'token': 'gAAAAAAdwJ6xAAECAwQFBgcICQoLDA0OD3HkMATM5lFqGaerZ-fWPOm73QeoCk9uGib28Xe5vz6oxq5nmxbx_v7mrfyudzUm',
                'now': '1985-10-26T01:20:01-07:00',
                'ttlSec': 60,
                'secret': 'cw_0x689RpI-jtRR7oE8h_eQsKImvJapLeSbXpwF4e4='
            },
            {
                'desc': 'payload padding error',
                'token': 'gAAAAAAdwJ6xAAECAwQFBgcICQoLDA0ODz4LEpdELGQAad7aNEHbf-JkLPIpuiYRLQ3RtXatOYREu2FWke6CnJNYIbkuKNqOhw==',
                'now': '1985-10-26T01:20:01-07:00',
                'ttlSec': 60,
                'secret': 'cw_0x689RpI-jtRR7oE8h_eQsKImvJapLeSbXpwF4e4='
            },
            {
                'desc': 'far-future TS (unacceptable clock skew)',
                'token': 'gAAAAAAdwStRAAECAwQFBgcICQoLDA0OD3HkMATM5lFqGaerZ-fWPAnja1xKYyhd-Y6mSkTOyTGJmw2Xc2a6kBd-iX9b_qXQcw==',
                'now': '1985-10-26T01:20:01-07:00',
                'ttlSec': 60,
                'secret': 'cw_0x689RpI-jtRR7oE8h_eQsKImvJapLeSbXpwF4e4='
            },
            {
                'desc': 'expired TTL',
                'token': 'gAAAAAAdwJ6xAAECAwQFBgcICQoLDA0OD3HkMATM5lFqGaerZ-fWPAl1-szkFVzXTuGb4hR8AKtwcaX1YdykRtfsH-p1YsUD2Q==',
                'now': '1985-10-26T01:21:31-07:00',
                'ttlSec': 60,
                'secret': 'cw_0x689RpI-jtRR7oE8h_eQsKImvJapLeSbXpwF4e4='
            },
            {
                'desc': 'incorrect IV (causes padding error)',
                'token': 'gAAAAAAdwJ6xBQECAwQFBgcICQoLDA0OD3HkMATM5lFqGaerZ-fWPAkLhFLHpGtDBRLRTZeUfWgHSv49TF2AUEZ1TIvcZjK1zQ==',
                'now': '1985-10-26T01:20:01-07:00',
                'ttlSec': 60,
                'secret': 'cw_0x689RpI-jtRR7oE8h_eQsKImvJapLeSbXpwF4e4='
            }
        ]";

        [Fact]
        public void CanTestInvalidFernetToken()
        {
            // Arrange
            var data = JsonData.DeserializeObject<List<FernetTokenData>>();

            // Act
            foreach (var fernetToken in data)
            {
                // Assert
                Assert.Throws<FernetException>(() =>
                {
                    var resultByte = SimpleFernet.Decrypt(
                        fernetToken.Secret.UrlSafe64Decode(),
                        fernetToken.Token,
                        out var timestamp,
                        fernetToken.TtlSec);
                });
            }
        }
    }
}