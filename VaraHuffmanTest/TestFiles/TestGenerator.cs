using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Numerics;


static class TestCaseGenerator {

    static string BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HuffmanTestCases");
    static int Seed = 0x123456;

    static void GenerateTestsProgram(string[] args) {
        GenerateTests();
    }

    public static void GenerateTests() {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        string testFolder = "tests";
        var testCaseDir = BasePath == null ? testFolder : Path.Combine(BasePath, testFolder);

        Directory.CreateDirectory(testCaseDir);

        Console.WriteLine($"Outputting tests to: {testCaseDir}");

        // test cases
        WriteTestCase(testCaseDir, "empty_ascii.txt", "", Encoding.ASCII);
        WriteTestCase(testCaseDir, "empty_utf8.txt", "", Encoding.UTF8);
        WriteTestCase(testCaseDir, "single_char.txt", "a", Encoding.ASCII);
        WriteTestCase(testCaseDir, "repeated_char.txt", new string('a', 1000), Encoding.ASCII);
        WriteTestCase(testCaseDir, "alternating_chars.txt", string.Join("", Enumerable.Repeat("ab", 500)), Encoding.ASCII);
        WriteTestCase(testCaseDir, "all_ascii.txt", string.Join("", Enumerable.Range(32, 95).Select(i => (char)i)), Encoding.ASCII);
        WriteTestCase(testCaseDir, "quick_brown.txt", "The quick brown fox jumps over the lazy dog 1234567890.", Encoding.ASCII);
        WriteTestCase(testCaseDir, "alphabet.txt", "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz", Encoding.ASCII);
        WriteTestCase(testCaseDir, "hi.txt", "Hi!", Encoding.ASCII);
        WriteTestCase(testCaseDir, "example.txt", "This is an example message. 73", Encoding.ASCII);
        WriteTestCase(testCaseDir, "newline-N.txt", "Line one\nLine two\n", Encoding.ASCII);
        WriteTestCase(testCaseDir, "newline-RN.txt", "Line one\r\nLine two\r\n", Encoding.ASCII);
        WriteBinaryTestCase(testCaseDir, "sub.txt", new byte[] { (byte)'S', (byte)'U', (byte)'B', 0x1A }); // end in "SUB" character
        WriteTestCase(testCaseDir, "abracadabra.txt", "abracadabraabracadabraabracadabra", Encoding.ASCII);
        WriteTestCase(testCaseDir, "skewed_freq15.txt", "aaaaaaaabbbccde", Encoding.ASCII);
        WriteTestCase(testCaseDir, "skewed_freq38.txt", "aaaaaaaaaaaaaaabbbbbbbbccccccdddddeeee", Encoding.ASCII);
        WriteTestCase(testCaseDir, "random_ascii.txt", GenerateRandomAscii(1000), Encoding.ASCII);
        WriteTestCase(testCaseDir, "long_random_ascii.txt", GenerateRandomAscii(100000), Encoding.ASCII);
        //WriteTestCase(testCaseDir, "binary_data.txt", GenerateBinaryData(1000));
        WriteTestCase(testCaseDir, "fibonacci.txt", GenerateFibonacciSequence(1000), Encoding.ASCII); // 1000 numbers
        WriteTestCase(testCaseDir, "palindrome.txt", GeneratePalindrome(1000), Encoding.ASCII);
        WriteTestCase(testCaseDir, "utf8_text.txt", GenerateUTF8Text(), Encoding.UTF8);
        WriteTestCase(testCaseDir, "utf16_text.txt", GenerateUTF16Text(), Encoding.Unicode);
        //WriteTestCase(testCaseDir, "ascii_extended.txt", GenerateExtendedASCII()); //TODO: change to binary?

        WriteBinaryTestCase(testCaseDir, "binary_255_unique.bin", Generate255UniqueBytes());
        WriteBinaryTestCase(testCaseDir, "binary_256_unique.bin", Generate256UniqueBytes());
        WriteBinaryTestCase(testCaseDir, "binary_random_1mb.bin", GenerateRandomBinaryData(1024 * 1024));

        // Windows 95 era encoding test cases
        WriteTestCase(testCaseDir, "windows1252.txt", GenerateWindows1252Text(), Encoding.GetEncoding(1252));
        WriteTestCase(testCaseDir, "iso8859_1.txt", GenerateISO8859_1Text(), Encoding.GetEncoding("iso-8859-1"));
        WriteTestCase(testCaseDir, "dos_cp437.txt", GenerateDOSCP437Text(), Encoding.GetEncoding(437));
        WriteTestCase(testCaseDir, "windows1251_cyrillic.txt", GenerateWindows1251Text(), Encoding.GetEncoding(1251));
        WriteTestCase(testCaseDir, "shift_jis.txt", GenerateShiftJISText(), Encoding.GetEncoding("shift-jis"));

        // email test cases
        WriteTestCase(testCaseDir, "short_email.txt", GenerateShortEmail(), Encoding.ASCII);
        WriteTestCase(testCaseDir, "long_email.txt", GenerateLongEmail(), Encoding.ASCII);
        WriteTestCase(testCaseDir, "multiple_emails.txt", GenerateMultipleEmails(10), Encoding.ASCII);

        Console.WriteLine("Test cases generated successfully.");
    }

    static void WriteTestCase(string directory, string filename, string content, Encoding encoding = null) {
        encoding = encoding ?? Encoding.UTF8;
        File.WriteAllText(Path.Combine(directory, filename), content, encoding);
    }

    static void WriteBinaryTestCase(string directory, string filename, byte[] content) {
        File.WriteAllBytes(Path.Combine(directory, filename), content);
    }

    static string GenerateRandomAscii(int length) {
        Random random = new Random(Seed);
        return new string(Enumerable.Repeat(0, length)
            .Select(_ => (char)random.Next(32, 127))
            .ToArray());
    }

    static string GenerateBinaryData(int length) {
        Random random = new Random(Seed);
        return string.Join("", Enumerable.Repeat(0, length)
            .Select(_ => (char)random.Next(0, 256)));
    }

    static string GenerateFibonacciSequence(int length) {
        StringBuilder sb = new StringBuilder();
        BigInteger a = 0, b = 1;
        while (sb.Length < length) {
            sb.Append(a);
            BigInteger temp = a;
            a = b;
            b = temp + b;
        }
        return sb.ToString(0, length);
    }

    static string GeneratePalindrome(int length) {
        Random random = new Random(Seed);
        string firstHalf = new string(Enumerable.Repeat(0, length / 2)
            .Select(_ => (char)random.Next(97, 123))
            .ToArray());
        string secondHalf = new string(firstHalf.Reverse().ToArray());
        return firstHalf + (length % 2 == 1 ? "x" : "") + secondHalf;
    }

    static string GenerateUTF8Text() {
        return "Hello, 世界! Здравствуй, мир! مرحبا بالعالم! 🌍🌎🌏";
    }

    static string GenerateUTF16Text() {
        return "UTF-16 Test: Hello, 世界! Здравствуй, мир! مرحبا بالعالم! 🌍🌎🌏";
    }

    static string GenerateExtendedASCII() {
        return string.Join("", Enumerable.Range(128, 128).Select(i => (char)i));
    }

    static byte[] Generate255UniqueBytes() {
        return Enumerable.Range(1, 255).Select(i => (byte)i).ToArray();
    }

    static byte[] Generate256UniqueBytes() {
        return Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
    }

    static byte[] GenerateRandomBinaryData(int length) {
        Random random = new Random(Seed);
        byte[] data = new byte[length];
        random.NextBytes(data);
        return data;
    }

    static string GenerateWindows1252Text() {
        return "Windows-1252 encoding: æøåÆØÅ œŒ ß ©®€ ¥£¢";
    }

    static string GenerateISO8859_1Text() {
        return "ISO-8859-1 encoding: æøåÆØÅ ðÐ þÞ ýÝ";
    }

    static string GenerateDOSCP437Text() {
        StringBuilder sb = new StringBuilder();
        for (int i = 1; i <= 255; i++) {
            sb.Append((char)i);
            if (i % 32 == 0) sb.Append("\n");
        }
        return sb.ToString();
    }

    static string GenerateWindows1251Text() {
        return "Windows-1251 (Cyrillic) encoding: Привет, мир! Как дела? Здравствуйте!";
    }

    static string GenerateShiftJISText() {
        return "Shift-JIS encoding: こんにちは、世界！ガンバレ！頑張れ！";
    }

    static string GenerateShortEmail() {
        return @"Date: 1998-06-25 08:00:00
From: KC5XYZ@winlink.org
To: W7ABC@winlink.org
Subject: Field Day Reminder

Field Day is this weekend. Don't forget your radio and antenna. See you at the park at 0900.

73,
Sender";
    }

    static string GenerateLongEmail() {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(@"Date: 1999-09-15 14:30:00
From: N0DEF@winlink.org
To: KA9GHI@winlink.org
Subject: Emergency Preparedness Drill Report

Hello Tom,

I hope this message finds you well. I wanted to provide a summary of our recent emergency preparedness drill conducted on September 12th.

Participants: 15 operators from our local club
Duration: 4 hours
Scenario: Simulated widespread power outage

Key observations:
1. All stations were operational within 30 minutes
2. Message relay efficiency improved by 20% compared to last drill
3. Two operators experienced antenna issues - need follow-up training

Action items:
- Schedule antenna workshop for next month
- Update emergency contact list
- Acquire two more portable power sources

Overall, the drill was a success. We identified areas for improvement and strengthened our readiness for real emergencies.

Please let me know if you need any additional information.

73,
Sarah
N0DEF
Emergency Coordinator");

        return sb.ToString();
    }

    static string GenerateMultipleEmails(int count) {
        StringBuilder sb = new StringBuilder();
        string[] subjects = { "Meeting reminder", "Project update", "Quick question", "Lunch tomorrow?", "Document review" };
        string[] bodies = {
            "Don't forget our meeting at 2 PM.",
            "The project is progressing well. We're on track to finish by Friday.",
            "Can you clarify the requirements for the new feature?",
            "Would you like to grab lunch tomorrow? Thinking of trying that new place down the street.",
            "Please review the attached document and provide your feedback by EOD."
        };

        for (int i = 0; i < count; i++) {
            sb.AppendLine($"From: sender{i}@example.com");
            sb.AppendLine($"To: recipient{i}@example.com");
            sb.AppendLine($"Subject: {subjects[i % subjects.Length]}");
            sb.AppendLine($"Date: {new DateTime(1999, 2, 15, 9, 0, 0, DateTimeKind.Utc).AddDays(i).ToString("ddd, d MMM yyyy HH:mm:ss K")}");
            sb.AppendLine();
            sb.AppendLine($"Dear Recipient {i},");
            sb.AppendLine();
            sb.AppendLine(bodies[i % bodies.Length]);
            sb.AppendLine();
            sb.AppendLine($"Best regards,");
            sb.AppendLine($"Sender {i}");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}