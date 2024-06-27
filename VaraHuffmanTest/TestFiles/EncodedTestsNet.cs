using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Numerics;
using VaraHuffman;
using VaraVBNetHuffman;

static class EncodedTestsNet {

    static string BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HuffmanTestCases");

    public static void GenerateNetEncodings() {

        string testFolder = "tests";
        var testCaseDir = BasePath == null ? testFolder : Path.Combine(BasePath, testFolder);

        string outputFolder = "test-vb-net";
        var testCaseNetDir = BasePath == null ? outputFolder : Path.Combine(BasePath, outputFolder);
        Directory.CreateDirectory(testCaseNetDir);

        string outputFolderCS = "test-csharp-net";
        var testCaseNetDirCS = BasePath == null ? outputFolderCS : Path.Combine(BasePath, outputFolderCS);
        Directory.CreateDirectory(testCaseNetDirCS);

        Console.WriteLine($"Opening tests in: {testCaseDir}");
        Console.WriteLine($"Outputting encoded (vb-net) to: {testCaseNetDir}");
        Console.WriteLine($"Outputting encoded to: {testCaseNetDir}");

        var files = Directory.GetFiles(testCaseDir, "*.txt")
                .Concat(Directory.GetFiles(testCaseDir, "*.bin"));

        foreach (var file in files) {
            //Console.WriteLine($"Running test: {file}");
            RunTestVB(file, testCaseNetDir);
            RunTestVaraHuffman(file, testCaseNetDirCS);
        }
    }

    private static void RunTestVB(string file, string outDir) {
        byte[] bytes = File.ReadAllBytes(file);

        // note: Can reuse same Vara instance for all tests, but wont.
        var vara = new VaraHuffmanVB();
        vara.ShowDebug = false;

        var encHEx = vara.EncodeBytes(bytes, bytes.Length); // auto choose best
        var encHE0 = vara.EncodeBytes(bytes, bytes.Length, VaraHuffmanVB.MessageType.NoCompression); 
        var encHE3 = vara.EncodeBytes(bytes, bytes.Length, VaraHuffmanVB.MessageType.Huffman); 

        string filename = Path.GetFileName(file);
        string fileNameHEx = filename + ".HEx";
        string fileNameHE0 = filename + ".HE0";
        string fileNameHE3 = filename + ".HE3";

        File.WriteAllBytes(Path.Combine(outDir, fileNameHEx), encHEx);
        File.WriteAllBytes(Path.Combine(outDir, fileNameHE0), encHE0);
        File.WriteAllBytes(Path.Combine(outDir, fileNameHE3), encHE3);
    }

    private static void RunTestVaraHuffman(string file, string outDir) {
        byte[] bytes = File.ReadAllBytes(file);

        // note: Can reuse same Vara instance for all tests, but wont.
        var vara = new VHuffman();
        vara.ShowDebug = false;

        var encHEx = vara.EncodeBytes(bytes, VHuffman.MessageType.Auto); // auto choose best
        var encHE0 = vara.EncodeBytes(bytes, VHuffman.MessageType.NoCompression);
        var encHE3 = vara.EncodeBytes(bytes, VHuffman.MessageType.Huffman);

        string filename = Path.GetFileName(file);
        string fileNameHEx = filename + ".HEx";
        string fileNameHE0 = filename + ".HE0";
        string fileNameHE3 = filename + ".HE3";

        File.WriteAllBytes(Path.Combine(outDir, fileNameHEx), encHEx);
        File.WriteAllBytes(Path.Combine(outDir, fileNameHE0), encHE0);
        File.WriteAllBytes(Path.Combine(outDir, fileNameHE3), encHE3);
    }

}