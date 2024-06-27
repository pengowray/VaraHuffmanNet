using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.GZip;
using VaraVBNetHuffman;

namespace VaraHuffmanTesting.DebugInfo;
public static class DebugInfoPrinter {

    private static VaraHuffmanVB? Vara;

    public static void PrintDebugInfo(string text) {
        Console.WriteLine($"to encode    : {text}");
        Console.WriteLine($"text length  : {text.Length} chars");
        byte[] bytes = Encoding.ASCII.GetBytes(text);
        PrintDebugInfo(bytes);
    }

    public static void PrintDebugInfo(byte[] bytes) {

        if (Vara == null) {
            Vara = new VaraHuffmanVB();
            Vara.ShowDebug = false;
        }

        Console.WriteLine($"byte length  : {bytes.Length} bytes");
        Console.WriteLine($"symbol count : {bytes.Distinct().Count()}");

        //Console.WriteLine($"as base64  : {Convert.ToBase64String(bytes)}");

        MemoryStream byteStream2 = new MemoryStream(bytes);
        MemoryStream GzipStream = new MemoryStream();
        GZip.Compress(byteStream2, GzipStream, false, 512, 9);

        MemoryStream byteStream = new MemoryStream(bytes);
        MemoryStream bzip2Stream = new MemoryStream();
        BZip2.Compress(byteStream, bzip2Stream, false, 9);

        var encHEx = Vara.EncodeByte(bytes, bytes.Length);
        //Console.WriteLine("---no compression---");
        var encHE0 = Vara.EncodeByte(bytes, bytes.Length, VaraHuffmanVB.MessageType.NoCompression);
        //Console.WriteLine("---end no compression---");
        var encHE3 = Vara.EncodeByte(bytes, bytes.Length, VaraHuffmanVB.MessageType.Huffman);

        //string encoded = Base64Encode(bytes); //Encoding.ASCII.GetString(bytes);
        Console.WriteLine($"byte length  : {bytes.Length} bytes (100%)");
        Console.WriteLine($"encoded len  : {encHEx.Length} bytes ({(double)encHEx.Length / bytes.Length:P1})");
        Console.WriteLine($"as HE0       : {encHE0.Length /* == bytes.Length + 4 */} bytes ({(double)encHE0.Length / bytes.Length:P1})");
        Console.WriteLine($"as HE3       : {encHE3.Length} bytes ({(double)encHE3.Length / bytes.Length:P1})");
        var gziplen = GzipStream.Length + 4;
        var bziplen = bzip2Stream.Length + 4;
        Console.WriteLine($"gzip in HE0  : {gziplen} bytes (gzip + 4 byte header) ({(double)gziplen / bytes.Length:P1})");
        Console.WriteLine($"bzip2 in HE0 : {bziplen} bytes (bz2 + 4 byte header)  ({(double)bziplen / bytes.Length:P1})");
        //Console.WriteLine($"encoded      : {Base64Encode(enc)}");
        Console.WriteLine($"PlainHeader  : {BitConverter.ToString(Data.ZeroBytesEncoded)}"); // + " ({Encoding.ASCII.GetString(ZeroBytesEncoded)})");
        Console.WriteLine($"HuffmanHeader: {BitConverter.ToString(Data.NormalStartEncoded)}"); // + " ({Encoding.ASCII.GetString(NormalStartEncoded)})");
        if (encHEx.Length >= 4) {
            Console.WriteLine($"encoded-head : {BitConverter.ToString(encHEx.Take(4).ToArray())}");
        }
        Console.WriteLine($"encoded-hex  : {BitConverter.ToString(encHEx)}");
        Console.WriteLine($"HE0-hex      : {BitConverter.ToString(encHE0)}");
        Console.WriteLine($"HE3-hex      : {BitConverter.ToString(encHE3)}");
        Console.WriteLine($"gzip-hex     : {BitConverter.ToString(GzipStream.ToArray())}");
        //Console.WriteLine($"encoded-asc:\r\n{Encoding.ASCII.GetString(enc)}");

        var decoded = Vara.DecodeByte(encHEx);
        //Console.WriteLine("--decoding H3--");
        var decodeHuff = Vara.DecodeByte(encHE3);
        //Console.WriteLine("--end decoding H3--");
        //Console.WriteLine("---no compression dec---");
        var decodeH0 = Vara.DecodeByte(encHE0);
        //Console.WriteLine("---end no compression dec---");

        //TODO: test gzip / bzip decompression too
        //GZip.Decompress()

        Console.WriteLine($"decode match : {(decoded.SequenceEqual(bytes) ? "MATCH" : "ERROR")}");
        bool decodeH3Match = decodeHuff.SequenceEqual(bytes);
        bool decodeH0Match = decodeH0.SequenceEqual(bytes);
        Console.WriteLine($"decode HE0   : {(decodeH0Match ? "MATCH" : "ERROR")}");
        if (!decodeH0Match) {
            Console.WriteLine($"decoded HE0 sequence (FAILED MATCH; length:{decodeH0.Length}):\n{BitConverter.ToString(decodeH0)}");
        }
        Console.WriteLine($"decode HE3   : {(decodeH3Match ? "MATCH" : "ERROR")}");
        if (!decodeH3Match) {
            Console.WriteLine($"decoded HE3 sequence (FAILED MATCH):\n{BitConverter.ToString(decodeHuff)}");
        }

        //Console.WriteLine($"decoded64     : {Base64Encode(decoded)}");
        //Console.WriteLine($"decoded ascii : {Encoding.ASCII.GetString(decoded)}");
        //Console.WriteLine($"decoded utf-8 : {Encoding.UTF8.GetString(decoded)}");
        //Console.WriteLine($"decoded 2  : {string.Join("", decoded.Select(d => (char)d))}");
        Console.WriteLine($"decoded length: {decoded.Length}");

        static string Base64Encode(byte[] bytes) {
            return System.Convert.ToBase64String(bytes);
        }

        static string Base64Encode2(string plainText) {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        static string Base64Decode(string base64EncodedData) {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

    }
}

