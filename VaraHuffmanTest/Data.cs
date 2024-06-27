using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaraHuffmanTesting;

public static class Data {

    public static readonly byte[] ZeroBytesEncoded = new byte[4] { 72, 69, 48, 13 }; // hex: 48-45-30-0D "HE0<CR>"
    public static readonly byte[] NormalStartEncoded = new byte[4] { 72, 69, 51, 13 }; // hex: 48-45-33-0D "HE3<CR>" or "HE3\r" (old MacOS line ending)

}
