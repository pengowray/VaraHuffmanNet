# VaraHuffmanNet

VARA HF's Huffman Encoding algorithm, updated to VB.NET.

## Project Goals

* Document the Huffman Encoding format of VARA protocol, which is used with WinLink on amateur radio.
* Create a reference implementation which runs on modern operating systems and compiles on current compilers.

## Notes about VARA 

* VARA does not stand for anything. 
* It is proprietary software is developed by EA5HVK.

# VARA's message formats

There are two message formats, plain bytes and Huffman Encoded.

## Plain Bytes message format

* Plain bytes messages begins with a four byte header: `48 45 30 0D` "HE0\r" followed by the message text or arbitrary bytes.

  *  There's no compression with the format

  *  `\r` — carriage return <CR> — was used by MacOS 9 as a line separator. If an output message is displayed on a Window terminal, the CR will "hide" the header by moving the cursor back to the start of the line, so the message displayed following it will overwrite the header.

  *  This mode will be used when Huffman compression would not decrease the message size. But VARA's provided source code appears to be a little suboptimial in making this comparison as it doesn't appear to consider the size of the Huffman Index and Bit Sequence Table, so it underestimates the size of the Huffman compressed message. (Is this the case with the release version too? Can we double check this behavior?)

  *  Empty messages are made up of just the four header bytes and nothing else.

## Huffman Message format

* Bytes 0 to 4: begin with the 4 byte header `48 45 33 0D` "HE3\r"  
  *  Presumably stands for "Huffman Encoding 3". (Anyone know which version of which VARA products this was introduced or is compatible with?)
  *  Note: '3' instead of '0' used for empty and plain text messages
  *  Messages which don't contain either header ("HE0\r" nor "HE3\r") are rejected
* Byte 5 is referred in the source code as a [CRC](https://en.wikipedia.org/wiki/Cyclic_redundancy_check). 
  *  It's calculated by XORing all bytes of original message (uncompressed without the header). It is _not_ CRC-8.
* Bytes 6,7,8,9: Integer with length of decoded message in bytes (without header). Little endian I guess.
* Bytes 10 and 11: SymbolCount: The number of unique/distinct symbols (8-bit characters) used in the message
  *  Same as the number of entries in the Huffman Index (or half its length in bytes)
  *  Which is also the number of nodes in the Huffman tree
  *  Possible values are 1 to 256. (0 would be sent as an empty unencoded message instead)
  *  For example, the message `Hi!!` will give a value of 3. 
  *  Bytes 10 and 11 together make up a little-endian 16-bit integer. Unless the message uses all 256 characters, byte 11 has a value of 0. (TODO: confirm this)
  *  All symbols are 8-bit bytes. Longer or shorter symbols or sequences cannot be encoded.
* Huffman Index (starting byte 12), 2-byte structure repeated SymbolCount times.
  *  1 byte: a character (byte) found in the message
  *  1 byte: integer length of its Huffman bit sequence (number of bits)
  *  Example: "H\2" (the letter H will be represented with a bit sequence two bits long)
  *  The bit sequences themselves are not specified yet, but found after the message
  *  Ordered by ascii value (0 to 255) in the source code but any order would probably work.
* Huffman Encoded Message 
  *  Any number of bytes long made up of huffman bit sequences representing 8-bit symbols (characters)
  *  If it ends with a partial byte, the remaining bits is are set to 0
* Huffman Bit Sequence Table
  *  The bit sequences matching each symbol (character) in the Huffman table
  *  If it ends with a partial byte, the remaining bits is are set to 0

Background:

* Protocols used for amateur radio are required by the FCC to be published. 
* This protocol is published by VARA's author as source code without comments. 
* It is published as a PDF hosted on mega.nz (which occassionally decides to give a paywall or hour delay for downloads). 
* The code cannot be pasted into a text file without then making manual corrections for formatting.
* There are no instructions on using the code or even what language it is in.
* It appears to be a form of [Classic VisualBasic](https://en.wikipedia.org/wiki/Visual_Basic_(classic)) (circa 1991 to 1998) such as VB6. It has many incompatibilities with modern VisualBasic.Net.
* There is no documentation on which versions of VARA it is compatible with. It does not run easily on modern systems. 
  
To investigate still:
  
* **I have not yet verified my version gives identical output to the original** (I haven't set up a VB6(?) environment) 
* The preferred character encoding used by the original I'm really not sure about, though it's capable of sending any arbitrary byte sequence.
* Compare efficency with gz or bz
* What compression is used by other Winlink related protocols and clients (ardop, [pat](https://github.com/la5nta/pat))
* What error correction is done? (other than the single parity byte) Is it done on another layer?
  
License

* This project may contain public propritory code that is otherwise difficult to access and use. The intention is to replace any propritory code that remains.
* All my own contributions are MIT licensed. 
* Documentation such as this readme or any lengthy comments is CC-BY (any) and MIT licensed (take your pick).

Links

* [VARA HF](https://www.sigidwiki.com/wiki/VARA_HF) on Signal Identification Wiki (sigidwik)
* [VARA news](https://www.winlink.org/tags/vara) on Winlink
