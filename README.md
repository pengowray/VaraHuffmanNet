# VaraHuffmanNet

VARA HF's Huffman Encoding algorithm, updated to VB.NET.

## Project Goals

* Document the Huffman Encoding format of the VARA protocol, which is used with WinLink on amateur radio. (Note: WinLink might disable VARA's compression and use its own FBB B1 compression instead)
* Create a reference implementation which runs on modern operating systems and compiles on current compilers.

## Notes about VARA 

* VARA does not stand for anything. 
* It is proprietary software is developed by EA5HVK.

# VARA's message formats

There are two message formats, Plain Bytes and Huffman Encoded.

## Plain Bytes message format

* Plain bytes messages begins with a four byte header: `48 45 30 0D` "HE0\r" followed by the message text or arbitrary bytes.

  *  There's no compression with the format

  *  `\r` — carriage return <CR> — was used by MacOS 9 as a line separator. If an output message is displayed on a Window terminal, the CR will "hide" the header by moving the cursor back to the start of the line, so the message displayed following it will overwrite the header.

  *  This mode will be used when Huffman compression would not decrease the message size. But VARA's provided source code appears to be a little suboptimial in making this comparison as it doesn't appear to consider the size of the Huffman tables, so it underestimates the size of the Huffman compressed message. (Is this the case with the release version too? Can we double check this behavior?)

  *  Empty messages are made up of just the four header bytes and nothing else.

## Huffman Message format

* Bytes 0 to 4: begin with the 4 byte header `48 45 33 0D` "HE3\r"  
  *  Presumably stands for "Huffman Encoding 3". (Anyone know which version of which VARA products this was introduced or is compatible with?)
  *  Only byte 2 differs from Plain Bytes message header (ascii 3 vs 0)
  *  Messages which don't contain either header ("HE0\r" nor "HE3\r") are rejected
* Byte 5: Parity byte
  *  It's calculated by XORing all bytes of original message (uncompressed without the header); Wiki's [longitudinal parity check](https://en.wikipedia.org/wiki/Longitudinal_redundancy_check) points out that "with this checksum, any transmission error which flips a single bit of the message, or an odd number of bits, will be detected as an incorrect checksum." Note that the message must be decoded before parity is checked.
  *  It is variously referred in the source code as a [CRC](https://en.wikipedia.org/wiki/Cyclic_redundancy_check) or Checksum. It is _not_ CRC-8.
* Bytes 6,7,8,9: Integer with length of decoded message in bytes (without header). Little endian like the rest of integers, I guess.
* Bytes 10 and 11: SymbolCount: The number of unique/distinct symbols (8-bit characters) used in the original message
  *  Same as the number of entries in the Huffman Table 1 (or double the number for the table's byte length)
  *  Which is also the number of leaf nodes in the Huffman tree
  *  Possible values are 1 to 256 (hex: `01 00` to `00 01`). Zero-symbol or empty messages are sent as an empty Plain Bytes message and not Huffman encoded, so this field does not have a 0 value in normal operation (unless ForceHuffman option is used)
  *  For example, the message `Hi!!` will give a value of 3. `03 00`
  *  Bytes 10 and 11 together make up a little-endian 16-bit integer. Unless the message uses all 256 characters, byte 11 has a value of 0. (TODO: confirm this)
  *  All symbols are 8-bit bytes (e.g. typically ascii characters). There's no support for other sized symbols or sequences.
 
* Huffman Table 1 (starting byte 12)
  *  2-byte structure repeated SymbolCount times.
    *  1 byte: an 8-bit symbol (character) found in the message
    *  1 byte: integer length of its prefix code in number of bits
  *  Example: "A\2" `65 02` (the letter A will be represented with a bit sequence two bits long)
  *  The prefix code (bit sequence) for the letter is stored at the end of the message in Huffman Table 2
  *  Ordered by ascii value (0 to 255) in the source code but any order would probably work.
 
* Encoded Message 
  *  Any number of bytes long made up of variable length Huffman prefix codes. 
  *  Each prefix code represents one 8-bit symbol (such as an ascii character)
  *  If the encoded message ends with a partial byte, the remaining bits are set to 0 (It's zero padded)
 
* Huffman Table 2: List of prefix codes
  *  Each prefix code listed one after another.
  *  Huffman Table 1 gives the length of each prefix code, and what symbol (e.g. ascii character) it represeents
  *  If Table 2 ends with a partial byte, the remaining bits is are set to 0 (It's zero padded)
  *  A prefix code has a length 1 to 255 (theoretically at least?)
  *  A prefix code may also be called a bit sequence, Huffman code, prefix-free binary code or codeword

* Notes:
  *  Partial decoding (streaming) of Huffman Encoded messages cannot be supported: The entire Huffman encoded message must be received before any of it can be practically decoded (because Huffman Table 2, needed to decode the message, comes at the end, after the encoded message)
   
Background:

* Protocols used for amateur radio are required by the FCC to be published. 
* This protocol is published by VARA's author as source code without comments. 
* It is published as a PDF hosted on mega.nz (which occassionally decides to give a paywall or hour delay for downloads). 
* The code cannot be pasted into a text file without then making manual corrections for formatting.
* There are no instructions on using the code or even what language it is in.
* It appears to be a form of [Classic VisualBasic](https://en.wikipedia.org/wiki/Visual_Basic_(classic)) (circa 1991 to 1998) such as VB6. It has many incompatibilities with modern VisualBasic.Net.
* There is no documentation on which versions of VARA the huffman message format is compatible with. 
* The VB6 code does not run easily on modern systems. 
* Unlike modern VB.NET, the IDE, compiler and runtime environments for VB6 are not free or open source.
 
To investigate still:
  
* **I have not yet verified my version gives identical output to the original** (I haven't set up a VB6(?) environment) Particular things to test in VB6 version and in current and previous products:
   * Does it have identical output? (eg check byte order, input size limitations, one unique symbol message encoding/decoding, and 256 unique symbol encoding/decoding)
   * What's the preferred/default character encoding used by the original? Does it vary with Windows configuration? (though it's capable of sending any arbitrary byte sequences anyway)
   * Which bugs still occur? (e.g. has choice between HE0 and HE3 improved)
   * Memory copying issues
   * Does it support reading 0-length Huffman Encoding messages? (It checks for them but not sure what original code returns. It never generates them). The original Huffman encoder will avoid encoding an empty message, but it looks like: `48 45 33 0D 00 00 00 00 00 00 00`
* Compare efficency of gz or bzip2 (in a small number of tests they appear much better for anything of a non-trivial length, such as short or long email messages)
* What compression is used by other Winlink related protocols and clients? (ardop, [pat](https://github.com/la5nta/pat))
* What error correction is done in practice? (other than the single parity byte) Is it done on another layer? (There's some kind of ECC in the related ROS project, used for Weak Signal Radio Chat)
* Was the provided source code actually used in production? How long ago? Have there been changes and bugfixes? Has efficiency been improved? The inclusion of progression events throughout the code (to give updates to the client about ongoing processing progress) implies it was actually used in production.
* Does decoder still work if order of Huffman Tables are rearranged?
* Does WinLink software directly interface with the Huffman encoder/decoder at all?
 
License

* This project may contain public propritory code that is otherwise difficult to access and use. The intention is to eventually replace any propritory code that remains in this project.
* All my own contributions are MIT licensed. 
* Documentation such as this readme or any lengthy comments is duel licensed CC-BY (any) and MIT (take your pick).

Links

* [VARA HF](https://www.sigidwiki.com/wiki/VARA_HF) on Signal Identification Wiki (sigidwik)
* [VARA news](https://www.winlink.org/tags/vara) on Winlink
* Winlink "Open B2F" (Winlink Message Structure and B2 Forwarding Protocol): 
   * [Documentation](https://winlink.org/B2F): FBB B1 compression, using LZH or LZHUF compression algorithm 
   * [VB.Net](https://github.com/ARSFI/Winlink-Compression) "actual source code used in Winlink programs"
   * [FBB for Pat (GO source)](https://github.com/la5nta/wl2k-go/tree/master/fbb)
