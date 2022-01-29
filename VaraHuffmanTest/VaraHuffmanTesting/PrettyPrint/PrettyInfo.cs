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
        Console.WriteLine($"to encode    : {text}");
        Console.WriteLine($"text length  : {text.Length} chars");
        byte[] bytes = Encoding.ASCII.GetBytes(text);
        PrettyPrintInfo(bytes);
    }

    public static void PrettyPrintInfo(byte[] bytes) {

        if (Vara == null) {
             Vara = new VHuffman();
        }

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
        Console.WriteLine($"as HE0       : {bytes.Length + 4} bytes ({(double)(bytes.Length + 4) / bytes.Length:P1})");
        Console.WriteLine($"as HE3       : {huffmanEnc.Length} bytes ({(double)huffmanEnc.Length / bytes.Length:P1})");
        var gziplen = GzipStream.Length + 4;
        var bziplen = bzip2Stream.Length + 4;
        Console.WriteLine($"gzip in HE0  : {gziplen} bytes (gzip + 4 byte header) ({(double)gziplen / bytes.Length:P1})");
        Console.WriteLine($"bzip2 in HE0 : {bziplen} bytes (bz2 + 4 byte header)  ({(double)bziplen / bytes.Length:P1})");
        //Console.WriteLine($"encoded      : {Base64Encode(enc)}");
        Console.WriteLine($"PlainHeader  : {BitConverter.ToString(Data.ZeroBytesEncoded)}"); // + " ({Encoding.ASCII.GetString(ZeroBytesEncoded)})");
        Console.WriteLine($"HuffmanHeader: {BitConverter.ToString(Data.NormalStartEncoded)}"); // + " ({Encoding.ASCII.GetString(NormalStartEncoded)})");
        if (enc.Length >= 4) {
            Console.WriteLine($"encoded-head : {BitConverter.ToString(enc.Take(4).ToArray())}");
        }
        Console.WriteLine($"encoded-hex  : {BitConverter.ToString(enc)}");
        Console.WriteLine($"HE3-hex      : {BitConverter.ToString(huffmanEnc)}");
        Console.WriteLine($"gzip-hex     : {BitConverter.ToString(GzipStream.ToArray())}");
        //Console.WriteLine($"encoded-asc:\r\n{Encoding.ASCII.GetString(enc)}");

        var decoded = Vara.DecodeByte(enc, enc.Length);
        var decodeHuff = Vara.DecodeByte(huffmanEnc, huffmanEnc.Length);
        //TODO: test gzip / bzip decompression too
        //GZip.Decompress()

        Console.WriteLine($"decode match : {(decoded.SequenceEqual(bytes) ? "MATCH" : "ERROR")}");
        Console.WriteLine($"decode HE3   : {(decodeHuff.SequenceEqual(bytes) ? "MATCH" : "ERROR")}");

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

