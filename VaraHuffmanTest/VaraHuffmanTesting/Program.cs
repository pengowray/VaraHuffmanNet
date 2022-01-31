using System.Text;
using VaraHuffman;
using VaraHuffmanTesting.DebugInfo;


void UsageExample() {
    // encoding:
    string message = "This is an example message. 73";
    byte[] bytesSequenceToEncode = Encoding.ASCII.GetBytes(message); // UTF8 and other encodings will work successfully, but may not be compatible with other systems (todo: find next best encoding for compatibility)
    var encoder = new VHuffman();
    byte[] encodedMessage = encoder.EncodeByte(bytesSequenceToEncode);
    Console.WriteLine($"Encoded message: {BitConverter.ToString(encodedMessage)}");

    // decoding:
    var decodedMessageBytes = encoder.DecodeByte(encodedMessage);
    string decodedMessage = Encoding.ASCII.GetString(decodedMessageBytes);
    Console.WriteLine($"Decoded message: {decodedMessage}");
}

void DebugInfoExample() {
    //var text = "what is up guys? "; // incorrectly chooses huffman compression
    //var text = "what!!";
    //var text = "a";
    //var text = "Sing, O goddess, the anger of Achilles son of Peleus, that brought countless ills upon the Achaeans. Many a brave soul did it send hurrying down to Hades, and many a hero did it yield a prey to dogs and vultures, for so were the counsels of Jove fulfilled from the day on which the son of Atreus, king of men, and great Achilles, first fell out with one another. And which of the gods was it that set them on to quarrel? It was the son of Jove and Leto; for he was angry with the king and sent a pestilence upon the host to plague the people, because the son of Atreus had dishonoured Chryses his priest. Now Chryses had come to the ships of the Achaeans to free his daughter, and had brought with him a great ransom: moreover he bore in his hand the sceptre of Apollo wreathed with a suppliant’s wreath, and he besought the Achaeans, but most of all the two sons of Atreus, who were their chiefs. “Sons of Atreus,” he cried, “and all other Achaeans, may the gods who dwell in Olympus grant you to sack the city of Priam, and to reach your homes in safety; but free my daughter, and accept a ransom for her, in reverence to Apollo, son of Jove.” On this the rest of the Achaeans with one voice were for respecting the priest and taking the ransom that he offered; but not so Agamemnon, who spoke fiercely to him and sent him roughly away. “Old man,” said he, “let me not find you tarrying about our ships, nor yet coming hereafter. Your sceptre of the god and your wreath shall profit you nothing. I will not free her. She shall grow old in my house at Argos far from her own home, busying herself with her loom and visiting my couch; so go, and do not provoke me or it shall be the worse for you.” The old man feared him and obeyed. Not a word he spoke, but went by the shore of the sounding sea and prayed apart to King Apollo whom lovely Leto had borne. “Hear me,” he cried, “O god of the silver bow, that protectest Chryse and holy Cilla and rulest Tenedos with thy might, hear me oh thou of Sminthe. If I have ever decked your temple with garlands, or burned your thigh-bones in fat of bulls or goats, grant my prayer, and let your arrows avenge these my tears upon the Danaans.” Thus did he pray, and Apollo heard his prayer. He came down furious from the summits of Olympus, with his bow and his quiver upon his shoulder, and the arrows rattled on his back with the rage that trembled within him. He sat himself down away from the ships with a face as dark as night, and his silver bow rang death as he shot his arrow in the midst of them. First he smote their mules and their hounds, but presently he aimed his shafts at the people themselves, and all day long the pyres of the dead were burning.";
    //text = text + text + text + text + text + text + text;

    ////test with every possibly character value (0-255) with some extra 39's to unbalance the tree
    //var text = Enumerable.Range(0, 256).Select(x => (byte)x).Append((byte)39).Append((byte)39).Append((byte)39).Append((byte)39).Append((byte)39).ToArray();
    //var text = "\0\0\0\0\0";
    //var text = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    //var text = "";

    //var text = "ababababababab";
    //var text = "01234567890";

    var text = "Here's another example message, this time for showing debug info.";
    DebugInfoPrinter.PrintDebugInfo(text);
}


Console.WriteLine("Usage example");
UsageExample();

Console.WriteLine();

Console.WriteLine("Debug Info Example");
DebugInfoExample();



// example Winlink email with headers
//var text = File.ReadAllText(@"c:\datasets\winlink-message.txt");
//DebugInfoPrinter.PrintDebugInfo(text);
//byte length  : 2055 bytes (100%)
//encoded len  : 1661 bytes (80.8%)
//HE0 forced   : 2059 bytes (100.2%)
//HE3 forced   : 1661 bytes (80.8%)
//gzip  len    : 987 bytes (with additional 8 byte header)  (48.0%)
//bzip2 len    : 1042 bytes (with additional 8 byte header)  (50.7%)