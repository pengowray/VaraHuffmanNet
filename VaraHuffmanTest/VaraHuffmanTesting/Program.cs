using System.Text;
using VaraHuffman;
using VaraHuffmanTesting.PrettyPrint;

// See https://aka.ms/new-console-template for more information
// Console.WriteLine("Hello, World!");

// naming ideas
// OpenVaraHuffman
// VelociHuffman: VARA HF Modem Huffman Encoder/Decoder (just for the Huffman encoding part)
// (Veloci from Velocidad)

// todo:
// - calculate how many bytes huffman tree takes (for 0 to 255 characters used)

// Non-breaking improvements:
// - rewrite it
// - break messages into separate frames when:
//    * max message length exceeded (uint length)
//    * an improvement could be made by creating a new huffman table
// - Improve HE0/HE3 selection: comparison between uncompressed and compressed sending options does not consider size of huffman tables

// Breaking Improvements:
// - Why isn't there a predefined huffman tree for plain text? (optimized for email/winlink)
// - Re-use previously used huffman tree
// - Allow multibyte huffman nodes
// - Allow decoding before entire message sent (Don't place part of the huffman code table at the end)
// - Just replace huffman with more modern compression, e.g. bzip2 or gz
// - UTF8 as default mode for unicode
// - Shorter header sequence
// - error correction (is this done at another level?)
// - CRC/Hash on the end, for the message sent, not the decoded message, instead of a not-CRC parity byte thing
// - Suggest compression for ARDOP (Amateur Radio Digital Open Protocol) -- the alternative to VARA. Why is VARA used instead?

// Code issues: (too many)
// - Too many ReDims; just calculate size once
// - It's VB6 or something very old (unspecified)
// - It's in a PDF (lines wraps break the code if you copy paste)
// - It's hosted on mega, a semi-paywall which prevents access
// - No comments
// - CRC is primitive (XOR all letters together)
// - Redundant code: Code for writing bits is done twice (for huffman encoded message, and then again for huffman symbols)
// - Reuse of variables for completely different things (Count);  checksum byte ("Char", renamed CharC and then Checksum) was reused as a temp byte for building huffman tables instead of reusing one just used before hand (ByteValue now renamed TempByte)
// - Bizarre global variables (parameter NodesCount in CreateTree expects a global scope / by reference)
// - Uses ByteArray as name of variable and a type. The variable is not a ByteArray.
// - BitValue lookup table (very doubious optimization): BitValue(0) = 2 ^ 0; BitValue(1) = 2 ^ 1; etc
// - heavy use of unsafe CopyMem
// - ReDim resizing of large arrays (for message input and output) multiple times
// - Resizes and replaces contents of input byte array and expects that to be available to the caller as if it's C code or something
// - has a variable named Char which isn't even legal in VB.Net

//Where's the ECC?:
//New ROS v7.4.0
//26 October, 2016
//A new error correction code have been implemented in ROS mode and improves slightly decodification in HF conditions.
//The new algorithm will start operating on November 9 with the v7.4.0

/*
var x = vara.EncodeString("test");
Console.WriteLine(x);
var y = vara.DecodeString(x);
Console.WriteLine(y);
*/

// example Winlink email with headers
//var text = File.ReadAllText(@"c:\datasets\winlink-message.txt");
//byte length  : 2055 bytes (100%)
//encoded len  : 1661 bytes (80.8%)
//HE0 forced   : 2059 bytes (100.2%)
//HE3 forced   : 1661 bytes (80.8%)
//gzip  len    : 987 bytes (with additional 8 byte header)  (48.0%)
//bzip2 len    : 1042 bytes (with additional 8 byte header)  (50.7%)

//var text = "what is up guys? "; // incorrectly chooses huffman compression
//var text = "what!!";
//var text = "a";
var text = "Sing, O goddess, the anger of Achilles son of Peleus, that brought countless ills upon the Achaeans. Many a brave soul did it send hurrying down to Hades, and many a hero did it yield a prey to dogs and vultures, for so were the counsels of Jove fulfilled from the day on which the son of Atreus, king of men, and great Achilles, first fell out with one another. And which of the gods was it that set them on to quarrel? It was the son of Jove and Leto; for he was angry with the king and sent a pestilence upon the host to plague the people, because the son of Atreus had dishonoured Chryses his priest. Now Chryses had come to the ships of the Achaeans to free his daughter, and had brought with him a great ransom: moreover he bore in his hand the sceptre of Apollo wreathed with a suppliant’s wreath, and he besought the Achaeans, but most of all the two sons of Atreus, who were their chiefs. “Sons of Atreus,” he cried, “and all other Achaeans, may the gods who dwell in Olympus grant you to sack the city of Priam, and to reach your homes in safety; but free my daughter, and accept a ransom for her, in reverence to Apollo, son of Jove.” On this the rest of the Achaeans with one voice were for respecting the priest and taking the ransom that he offered; but not so Agamemnon, who spoke fiercely to him and sent him roughly away. “Old man,” said he, “let me not find you tarrying about our ships, nor yet coming hereafter. Your sceptre of the god and your wreath shall profit you nothing. I will not free her. She shall grow old in my house at Argos far from her own home, busying herself with her loom and visiting my couch; so go, and do not provoke me or it shall be the worse for you.” The old man feared him and obeyed. Not a word he spoke, but went by the shore of the sounding sea and prayed apart to King Apollo whom lovely Leto had borne. “Hear me,” he cried, “O god of the silver bow, that protectest Chryse and holy Cilla and rulest Tenedos with thy might, hear me oh thou of Sminthe. If I have ever decked your temple with garlands, or burned your thigh-bones in fat of bulls or goats, grant my prayer, and let your arrows avenge these my tears upon the Danaans.” Thus did he pray, and Apollo heard his prayer. He came down furious from the summits of Olympus, with his bow and his quiver upon his shoulder, and the arrows rattled on his back with the rage that trembled within him. He sat himself down away from the ships with a face as dark as night, and his silver bow rang death as he shot his arrow in the midst of them. First he smote their mules and their hounds, but presently he aimed his shafts at the people themselves, and all day long the pyres of the dead were burning.";
//text = text + text + text + text + text + text + text;

PrettyInfo.PrettyPrintInfo(text);

