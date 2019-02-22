using System;

namespace FernetTests.Internal
{
    public class FernetTokenData
    {
        public string Token { get; set; }
        public DateTime Now { get; set; }
        public byte[] Iv { get; set; }
        public string Src { get; set; }
        public string Secret { get; set; }
        public int TtlSec { get; set; }
        public string Desc { get; set; }
    }
}
