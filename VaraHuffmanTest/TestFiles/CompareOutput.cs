using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

public class CompareOutput {

    static string BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HuffmanTestCases");
    static string TestVb6Path = Path.Combine(BasePath, "test-vb6");
    static string TestCSharpNetPath = Path.Combine(BasePath, "test-csharp-net");
    static string TestVbNetPath = Path.Combine(BasePath, "test-vb-net");

    public static void Main() {
        var filesVb6 = Directory.GetFiles(TestVb6Path, "*.HEx");

        StringBuilder mdContent = new StringBuilder();
        mdContent.AppendLine("| vb-6 HEx | c# HEx | c# HE0 | C# HE3 | vbnet HEx | vbnet HE0 | vbnet HE3 |");
        mdContent.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");

        foreach (var fileVb6 in filesVb6) {
            string fileName = Path.GetFileName(fileVb6);
            string baseName = Path.GetFileNameWithoutExtension(fileVb6);

            var csharpFiles = new[] { $"{baseName}.HEx", $"{baseName}.HE0", $"{baseName}.HE3" }
                .Select(ext => Path.Combine(TestCSharpNetPath, ext));
            var vbnetFiles = new[] { $"{baseName}.HEx", $"{baseName}.HE0", $"{baseName}.HE3" }
                .Select(ext => Path.Combine(TestVbNetPath, ext));

            var results = new[]
            {
                "✅", "✅", "✅", "✅", "✅", "✅" // Defaults to all matched
            };

            // Compare with C# files
            for (int i = 0; i < csharpFiles.Count(); i++) {
                if (!File.Exists(csharpFiles.ElementAt(i))) {
                    results[i] = "—";
                } else if (!FilesMatch(fileVb6, csharpFiles.ElementAt(i))) {
                    results[i] = "❌";
                }
            }

            // Compare with VB.Net files
            for (int i = 0; i < vbnetFiles.Count(); i++) {
                if (!File.Exists(vbnetFiles.ElementAt(i))) {
                    results[i + 3] = "—";
                } else if (!FilesMatch(fileVb6, vbnetFiles.ElementAt(i))) {
                    results[i + 3] = "❌";
                }
            }

            mdContent.AppendLine($"| {fileName} | {results[0]} | {results[1]} | {results[2]} | {results[3]} | {results[4]} | {results[5]} |");
        }

        File.WriteAllText(Path.Combine(BasePath, "ComparisonResults.md"), mdContent.ToString());
        Console.WriteLine("Comparison completed and results written to ComparisonResults.md");
    }

    static bool FilesMatch(string file1, string file2) {
        byte[] file1Bytes = File.ReadAllBytes(file1);
        byte[] file2Bytes = File.ReadAllBytes(file2);

        return file1Bytes.SequenceEqual(file2Bytes);
    }
}
