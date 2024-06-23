using System;
using System.Linq;
using System.Text;

//TODO: test
//TODO: move to separate project

public class VHuffNet {
    public enum MessageType {
        Auto,
        NoCompression,
        Huffman
    }

    public bool ShowDebug { get; set; } = false;

    private const short MaxNodes = 511;

    private class HuffmanTree {
        public int ParentNode { get; set; }
        public int RightNode { get; set; }
        public int LeftNode { get; set; }
        public int Symbol { get; set; }
        public long Weight { get; set; }
    }

    private class ByteArray {
        public byte Count { get; set; }
        public byte[] Data { get; set; }

        public ByteArray Clone() {
            return new ByteArray {
                Count = Count,
                Data = (byte[])Data.Clone()
            };
        }
    }

    public class VaraHuffException : Exception {
        public VaraHuffException(string message) : base(message) {
        }
    }

    private int CreateTree(HuffmanTree[] nodes, int nodesCount, int charC, ByteArray symbolBitSequence) {
        int nodeIndex = 0;
        for (int a = 0; a < symbolBitSequence.Count; a++) {
            if (symbolBitSequence.Data[a] == 0) {
                if (nodes[nodeIndex].LeftNode == -1) {
                    nodes[nodeIndex].LeftNode = nodesCount;
                    nodes[nodesCount].ParentNode = nodeIndex;
                    nodes[nodesCount].LeftNode = -1;
                    nodes[nodesCount].RightNode = -1;
                    nodes[nodesCount].Symbol = -1;
                    nodesCount++;
                }
                nodeIndex = nodes[nodeIndex].LeftNode;
            } else if (symbolBitSequence.Data[a] == 1) {
                if (nodes[nodeIndex].RightNode == -1) {
                    nodes[nodeIndex].RightNode = nodesCount;
                    nodes[nodesCount].ParentNode = nodeIndex;
                    nodes[nodesCount].LeftNode = -1;
                    nodes[nodesCount].RightNode = -1;
                    nodes[nodesCount].Symbol = -1;
                    nodesCount++;
                }
                nodeIndex = nodes[nodeIndex].RightNode;
            } else {
                throw new VaraHuffException("Unexpected bit value in symbolBitSequence");
            }
        }
        nodes[nodeIndex].Symbol = charC;
        return nodesCount;
    }

    public byte[] EncodeByte(byte[] inputArray, int byteLen = -1, MessageType encoding = MessageType.Auto) {
        if (byteLen == -1) {
            byteLen = inputArray.Length;
        }

        if (byteLen == 0 && encoding != MessageType.Huffman) {
            byte[] result = new byte[byteLen + 4];
            if (byteLen > 0) {
                Array.Copy(inputArray, 0, result, 4, byteLen);
            }
            result[0] = 72; // 'H'
            result[1] = 69; // 'E'
            result[2] = 48; // '0'
            result[3] = 13; // '\r'
            Debug("0 ByteLen");
            return result;
        }

        byte[] result = new byte[523];
        result[0] = 72; // 'H'
        result[1] = 69; // 'E'
        result[2] = 51; // '3'
        result[3] = 13; // '\r'
        int resultLen = 4;

        int[] charCount = new int[256];
        for (int i = 0; i < byteLen; i++) {
            charCount[inputArray[i]]++;
        }

        HuffmanTree[] nodes = new HuffmanTree[MaxNodes];
        for (int i = 0; i < MaxNodes; i++) {
            nodes[i] = new HuffmanTree();
        }

        int nodesCount = 0;
        int symbolCount2 = 0;
        for (int i = 0; i < 256; i++) {
            if (charCount[i] > 0) {
                nodes[nodesCount].Weight = charCount[i];
                nodes[nodesCount].Symbol = i;
                nodes[nodesCount].LeftNode = -1;
                nodes[nodesCount].RightNode = -1;
                nodes[nodesCount].ParentNode = -1;
                nodesCount++;
                symbolCount2++;
            }
        }

        for (int lNodes = nodesCount; lNodes >= 2; lNodes--) {
            int lNode1 = -1, lNode2 = -1;
            long lWeight1 = 0, lWeight2 = 0;
            for (int i = 0; i < nodesCount; i++) {
                if (nodes[i].ParentNode == -1) {
                    if (lNode1 == -1) {
                        lWeight1 = nodes[i].Weight;
                        lNode1 = i;
                    } else if (lNode2 == -1) {
                        lWeight2 = nodes[i].Weight;
                        lNode2 = i;
                    } else if (nodes[i].Weight < lWeight1) {
                        if (nodes[i].Weight < lWeight2) {
                            if (lWeight1 < lWeight2) {
                                lWeight2 = nodes[i].Weight;
                                lNode2 = i;
                            } else {
                                lWeight1 = nodes[i].Weight;
                                lNode1 = i;
                            }
                        } else {
                            lWeight1 = nodes[i].Weight;
                            lNode1 = i;
                        }
                    } else if (nodes[i].Weight < lWeight2) {
                        lWeight2 = nodes[i].Weight;
                        lNode2 = i;
                    }
                }
            }
            nodes[nodesCount].Weight = lWeight1 + lWeight2;
            nodes[nodesCount].LeftNode = lNode1;
            nodes[nodesCount].RightNode = lNode2;
            nodes[nodesCount].ParentNode = -1;
            nodes[nodesCount].Symbol = -1;

            nodes[lNode1].ParentNode = nodesCount;
            nodes[lNode2].ParentNode = nodesCount;
            nodesCount++;
        }

        ByteArray[] symbolBitSequence = new ByteArray[256];
        for (int i = 0; i < 256; i++) {
            symbolBitSequence[i] = new ByteArray { Data = new byte[256] };
        }

        ByteArray bytes = new ByteArray { Data = new byte[256] };
        if (nodesCount > 0) {
            CreateBitSequences(nodes, nodesCount - 1, bytes, symbolBitSequence);
        }

        if (symbolCount2 == 1) {
            symbolBitSequence[inputArray[0]].Count = 1;
        }

        int hEncodedMessageLen = 0;
        for (int i = 0; i < 256; i++) {
            if (charCount[i] > 0) {
                hEncodedMessageLen += symbolBitSequence[i].Count * charCount[i];
                Debug($"char {i}: {charCount[i]}x {symbolBitSequence[i].Count} bits ({symbolBitSequence[i].Count / 8.0} bytes)");
            }
        }
        hEncodedMessageLen = (hEncodedMessageLen % 8 == 0) ? hEncodedMessageLen / 8 : hEncodedMessageLen / 8 + 1;

        if (encoding == MessageType.NoCompression || (encoding == MessageType.Auto && (hEncodedMessageLen == 0 || hEncodedMessageLen > byteLen))) {
            byte[] noCompressionResult = new byte[byteLen + 4];
            Array.Copy(inputArray, 0, noCompressionResult, 4, byteLen);
            noCompressionResult[0] = 72; // 'H'
            noCompressionResult[1] = 69; // 'E'
            noCompressionResult[2] = 48; // '0'
            noCompressionResult[3] = 13; // '\r'
            Debug($"HE0; lLength:{hEncodedMessageLen} > ByteLen:{byteLen}; Encoding:{encoding}");
            return noCompressionResult;
        }

        byte checksum = 0;
        for (int i = 0; i < byteLen; i++) {
            checksum ^= inputArray[i];
        }
        result[resultLen++] = checksum;

        Array.Copy(BitConverter.GetBytes(byteLen), 0, result, resultLen, 4);
        Debug($"Encode ByteLen:{byteLen}");
        resultLen += 4;

        byte[] bitValue = { 1, 2, 4, 8, 16, 32, 64, 128 };
        int symbolCount = 0;
        for (int i = 0; i < 256; i++) {
            if (symbolBitSequence[i].Count > 0) symbolCount++;
        }

        Array.Copy(BitConverter.GetBytes((ushort)symbolCount), 0, result, resultLen, 2);
        Debug($"Encoding count :{symbolCount}");
        Debug($"Encoding count2:{symbolCount2}");
        resultLen += 2;

        int huffmanIndexBitLenAndSome = 0;
        for (int i = 0; i < 256; i++) {
            if (symbolBitSequence[i].Count > 0) {
                result[resultLen++] = (byte)i;
                result[resultLen++] = symbolBitSequence[i].Count;
                huffmanIndexBitLenAndSome += 16 + symbolBitSequence[i].Count;
            }
        }

        Array.Resize(ref result, resultLen + huffmanIndexBitLenAndSome / 8);

        int bitPos = 0;
        byte tempByte = 0;
        for (int i = 0; i < 256; i++) {
            if (symbolBitSequence[i].Count > 0) {
                for (int j = 0; j < symbolBitSequence[i].Count; j++) {
                    if (symbolBitSequence[i].Data[j] == 1) tempByte += bitValue[bitPos];
                    bitPos++;
                    if (bitPos == 8) {
                        result[resultLen++] = tempByte;
                        tempByte = 0;
                        bitPos = 0;
                    }
                }
            }
        }
        if (bitPos > 0) {
            result[resultLen++] = tempByte;
        }

        Array.Resize(ref result, resultLen + hEncodedMessageLen);

        tempByte = 0;
        bitPos = 0;
        for (int i = 0; i < byteLen; i++) {
            for (int j = 0; j < symbolBitSequence[inputArray[i]].Count; j++) {
                if (symbolBitSequence[inputArray[i]].Data[j] == 1) tempByte += bitValue[bitPos];
                bitPos++;
                if (bitPos == 8) {
                    result[resultLen++] = tempByte;
                    bitPos = 0;
                    tempByte = 0;
                }
            }
        }
        if (bitPos > 0) {
            result[resultLen++] = tempByte;
        }

        Array.Resize(ref result, resultLen);
        Debug("success2");
        return result;
    }

    public byte[] DecodeByte(byte[] inputBytes, int byteLen = -1) {
        public byte[] DecodeByte(byte[] inputBytes, int byteLen = -1) {
            if (byteLen == -1) {
                byteLen = inputBytes.Length;
            }

            // Check for header: "HE0\r" or "HE3\r"
            if (inputBytes[0] != 72 || inputBytes[1] != 69 || inputBytes[3] != 13) {
                throw new VaraHuffException("Invalid header");
            }

            if (inputBytes[2] == 48) // '0'
            {
                byte[] result = new byte[byteLen - 4];
                Array.Copy(inputBytes, 4, result, 0, byteLen - 4);
                Debug("successful decode null string or HE0");
                return result;
            }

            if (inputBytes[2] != 51) // '3'
            {
                throw new VaraHuffException("The data either was not compressed with HE3 or is corrupt (identification string not found)");
            }

            int currPos = 5;
            byte checkSum = inputBytes[currPos - 1];
            currPos++;

            int resultLen = BitConverter.ToInt32(inputBytes, currPos - 1);
            Debug($"Decoded ResultLen:{resultLen}");
            currPos += 4;

            int lResultLen = resultLen;
            if (resultLen == 0) {
                Debug("0 length huffman message");
                return new byte[0];
            }

            byte[] result = new byte[resultLen];
            ushort count = BitConverter.ToUInt16(inputBytes, currPos - 1);
            Debug($"Decoded Count:{count}");
            currPos += 2;

            ByteArray[] symbolBitSequence = new ByteArray[256];
            for (int i = 0; i < 256; i++) {
                symbolBitSequence[i] = new ByteArray();
            }

            HuffmanTree[] nodes = new HuffmanTree[MaxNodes + 1];
            for (int i = 0; i <= MaxNodes; i++) {
                nodes[i] = new HuffmanTree();
            }

            for (int i = 1; i <= count; i++) {
                byte symbol = inputBytes[currPos - 1];
                currPos++;
                byte bitCount = inputBytes[currPos - 1];
                currPos++;
                symbolBitSequence[symbol].Count = bitCount;
                symbolBitSequence[symbol].Data = new byte[bitCount];
            }

            byte[] bitValue = { 1, 2, 4, 8, 16, 32, 64, 128 };
            Debug($"CurrPos: {currPos}; InputBytes.Length:{inputBytes.Length}");
            byte byteValue = inputBytes[currPos - 1];
            currPos++;
            int bitPos = 0;

            for (int i = 0; i < 256; i++) {
                if (symbolBitSequence[i].Count > 0) {
                    for (int j = 0; j < symbolBitSequence[i].Count; j++) {
                        if ((byteValue & bitValue[bitPos]) != 0) symbolBitSequence[i].Data[j] = 1;
                        bitPos++;
                        if (bitPos == 8) {
                            byteValue = inputBytes[currPos - 1];
                            currPos++;
                            bitPos = 0;
                        }
                    }
                }
            }

            if (bitPos == 0) currPos--;

            int nodesCount = 1;
            nodes[0].LeftNode = -1;
            nodes[0].RightNode = -1;
            nodes[0].ParentNode = -1;
            nodes[0].Symbol = -1;

            for (int i = 0; i < 256; i++) {
                nodesCount = CreateTree(nodes, nodesCount, i, symbolBitSequence[i]);
            }

            resultLen = 0;
            int nodeIndex = 0;
            for (int pos = currPos; pos < byteLen; pos++) {
                byteValue = inputBytes[pos];
                for (bitPos = 0; bitPos < 8; bitPos++) {
                    nodeIndex = (byteValue & bitValue[bitPos]) != 0 ? nodes[nodeIndex].RightNode : nodes[nodeIndex].LeftNode;
                    if (nodes[nodeIndex].Symbol > -1) {
                        result[resultLen] = (byte)nodes[nodeIndex].Symbol;
                        resultLen++;
                        if (resultLen == lResultLen) {
                            Debug("DecodeFinished");
                            goto DecodeFinished;
                        }
                        nodeIndex = 0;
                    }
                }
            }

        DecodeFinished:
            byte charC = 0;
            for (int i = 0; i < resultLen; i++) {
                charC ^= result[i];
            }
            if (charC != checkSum) {
                throw new VaraHuffException("The data might be corrupted (checksum did not match expected value)");
            }

            Array.Resize(ref result, resultLen);
            Debug("success");
            return result;
        }

        private void Debug(string text) {
        if (ShowDebug) {
            Console.WriteLine("VHuffNet debug: " + text);
        }
    }

    private void CreateBitSequences(HuffmanTree[] nodes, int nodeIndex, ByteArray bytes, ByteArray[] symbolBitSequence) {
        HuffmanTree cNode = nodes[nodeIndex];

        if (cNode.Symbol > -1) {
            symbolBitSequence[cNode.Symbol] = bytes;
            return;
        }

        if (cNode.LeftNode > -1) {
            ByteArray newBytes = bytes.Clone();
            newBytes.Data[newBytes.Count] = 0;
            newBytes.Count++;
            CreateBitSequences(nodes, cNode.LeftNode, newBytes, symbolBitSequence);
        }

        if (cNode.RightNode > -1) {
            ByteArray newBytes = bytes.Clone();
            newBytes.Data[newBytes.Count] = 1;
            newBytes.Count++;
            CreateBitSequences(nodes, cNode.RightNode, newBytes, symbolBitSequence);
        }
    }
}