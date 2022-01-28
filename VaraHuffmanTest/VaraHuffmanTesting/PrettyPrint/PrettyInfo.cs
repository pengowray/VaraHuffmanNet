using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.GZip;
using VaraHuffman;

namespace VaraHuffmanTesting.PrettyPrint;
public static class PrettyInfo {

    private static VHuffman? Vara;

    public static void PrettyPrintInfo(string text) {

        if (Vara == null) {
             Vara = new VHuffman();
        }


        Console.WriteLine($"to encode    : {text}");
        Console.WriteLine($"text length  : {text.Length} chars");
        byte[] bytes = Encoding.ASCII.GetBytes(text);
        Console.WriteLine($"byte length  : {bytes.Length} bytes");
        //Console.WriteLine($"as base64  : {Convert.ToBase64String(bytes)}");

        MemoryStream byteStream2 = new MemoryStream(bytes);
        MemoryStream GzipStream = new MemoryStream();
        GZip.Compress(byteStream2, GzipStream, false, 512, 9);

        MemoryStream byteStream = new MemoryStream(bytes);
        MemoryStream bzip2Stream = new MemoryStream();
        BZip2.Compress(byteStream, bzip2Stream, false, 9);

        var enc = Vara.EncodeByte(bytes, bytes.Length);
        var huffmanEnc = Vara.EncodeByte(bytes, bytes.Length, ForceHuffman:true);

        //string encoded = Base64Encode(bytes); //Encoding.ASCII.GetString(bytes);
        Console.WriteLine($"byte length  : {bytes.Length} bytes (100%)");
        Console.WriteLine($"encoded len  : {enc.Length} bytes ({(double)enc.Length / bytes.Length:P1})");
        Console.WriteLine($"HE0 forced   : {bytes.Length + 4} bytes ({(double)(bytes.Length + 4) / bytes.Length:P1})");
        Console.WriteLine($"HE3 forced   : {huffmanEnc.Length} bytes ({(double)huffmanEnc.Length / bytes.Length:P1})");
        var gziplen = GzipStream.Length + 8;
        var bziplen = bzip2Stream.Length + 8;
        Console.WriteLine($"gzip  len    : {gziplen} bytes (with additional 8 byte header)  ({(double)gziplen / bytes.Length:P1})");
        Console.WriteLine($"bzip2 len    : {bziplen} bytes (with additional 8 byte header)  ({(double)bziplen / bytes.Length:P1})");
        //Console.WriteLine($"encoded      : {Base64Encode(enc)}");
        Console.WriteLine($"PlainHeader  : {BitConverter.ToString(Data.ZeroBytesEncoded)}"); // + " ({Encoding.ASCII.GetString(ZeroBytesEncoded)})");
        Console.WriteLine($"HuffmanHeader: {BitConverter.ToString(Data.NormalStartEncoded)}"); // + " ({Encoding.ASCII.GetString(NormalStartEncoded)})");
        if (enc.Length >= 4) {
            Console.WriteLine($"encoded-head : {BitConverter.ToString(enc.Take(4).ToArray())}");
        }
        Console.WriteLine($"encoded-hex  : {BitConverter.ToString(enc)}");
        //Console.WriteLine($"encoded-asc:\r\n{Encoding.ASCII.GetString(enc)}");

        //enc[3] = 51; // force non-null

        var decoded = Vara.DecodeByte(enc, enc.Length);

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

