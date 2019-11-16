using System;
using System.Collections.Generic;
using System.Text;

namespace CLI
{
    public enum ECompressionType : byte
    {
        None = 0,
        LzMa = 1,
        Lz4 = 2,
        Lz4Hc = 3,
        LzHam = 4
    }
}
