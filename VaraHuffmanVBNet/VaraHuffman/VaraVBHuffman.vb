Option Explicit On
' based on "VARA HUFFMAN COMPRESSION.pdf"

' Imports System.Runtime.InteropServices

Public Class VaraHuffmanVB

    Public Enum MessageType
        Auto
        NoCompression
        Huffman
    End Enum
    Public ShowDebug As Boolean = False

    Private Class HuffmanTree
        Public ParentNode As Integer
        Public RightNode As Integer
        Public LeftNode As Integer
        Public Symbol As Integer ' was "Value as Integer" (0-255 or -1 for empty)
        Public Weight As Long
    End Class

    Private Class ByteArray
        Public Count As Byte
        Public Data() As Byte

        Function Clone() As ByteArray
            Dim newByteArray As ByteArray
            newByteArray = New ByteArray()
            newByteArray.Count = Count
            newByteArray.Data = Data.Clone()
            Return newByteArray
        End Function
    End Class

    'original:
    'Private Declare Sub CopyMem Lib "kernel32" Alias "RtlMoveMemory" (Destination As Any, Source As Any, ByVal Length As Long)
    'example fix:
    '<System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.AsAny)> ByVal o As Object)
    'Private Declare Sub CopyMem Lib "kernel32" Alias "RtlMoveMemory" (<MarshalAsAttribute(UnmanagedType.AsAny)> Destination As Object, <MarshalAsAttribute(UnmanagedType.AsAny)> Source As Object, ByVal Length As Long)

    Private Const MaxNodes As Int16 = 511 ' 0..MaxNodes-1; (Max 511 nodes for huffman tree are needed. 2N-1; original code was 512)

    'Create Huffman tree 
    Private Function CreateTree(Nodes() As HuffmanTree, NodesCount As Long, CharC As Long, SymbolBitSequence As ByteArray)
        Dim a As Integer, NodeIndex As Long

        NodeIndex = 0
        For a = 0 To (SymbolBitSequence.Count - 1)
            If (SymbolBitSequence.Data(a) = 0) Then
                If (Nodes(NodeIndex).LeftNode = -1) Then
                    Nodes(NodeIndex).LeftNode = NodesCount
                    Nodes(NodesCount).ParentNode = NodeIndex
                    Nodes(NodesCount).LeftNode = -1
                    Nodes(NodesCount).RightNode = -1
                    Nodes(NodesCount).Symbol = -1
                    NodesCount = NodesCount + 1
                End If
                NodeIndex = Nodes(NodeIndex).LeftNode
            ElseIf (SymbolBitSequence.Data(a) = 1) Then
                If (Nodes(NodeIndex).RightNode = -1) Then
                    Nodes(NodeIndex).RightNode = NodesCount
                    Nodes(NodesCount).ParentNode = NodeIndex
                    Nodes(NodesCount).LeftNode = -1
                    Nodes(NodesCount).RightNode = -1
                    Nodes(NodesCount).Symbol = -1
                    NodesCount = NodesCount + 1
                End If
                NodeIndex = Nodes(NodeIndex).RightNode
            Else
                Stop
            End If
        Next
        Nodes(NodeIndex).Symbol = CharC
        Return NodesCount
    End Function

    Public Function EncodeBytes(InputArray() As Byte, Optional ByteLen As Long = -1, Optional Encoding As MessageType = MessageType.Auto) As Byte()
        Dim i As Long, j As Long, Checksum As Byte, BitPos As Byte, lNode1 As Long
        Dim lNode2 As Long, lNodes As Long, HEncodedMessageLen As Long, HuffmanIndexBitLenAndSome As Integer
        Dim lWeight1 As Long, lWeight2 As Long, Result() As Byte, TempByte As Byte
        Dim ResultLen As Long, NodesCount As Integer,
        BitValue(0 To 7) As Byte, CharCount(0 To 255) As Long
        Dim Nodes(0 To MaxNodes - 1) As HuffmanTree
        Dim Bytes As ByteArray
        Dim SymbolBitSequence(0 To 255) As ByteArray ' .Count = number of bits (stored as bytes); .Data(): 0's and 1's
        Dim SymbolCount As Integer
        Dim SymbolCount2 As Integer

        If (ByteLen = -1) Then
            ByteLen = InputArray.Length
        End If

        If (ByteLen = 0 And Not Encoding = MessageType.Huffman) Then ' causes an error, so don't force to compress 0 len inputs for now
            'If (ByteLen = 0) Then 'note: error if we don't do this for ForceHuffman (...And Not ForceHuffman)
            ReDim Preserve InputArray(0 To ByteLen + 3)
            If (ByteLen > 0) Then
                'Call CopyMem(ByteArray(4), ByteArray(0), ByteLen)
                Array.Copy(InputArray, 0, InputArray, 4, ByteLen)
            End If
            ' "HE0\r"
            InputArray(0) = 72
            InputArray(1) = 69
            InputArray(2) = 48
            InputArray(3) = 13
            Debug("0 ByteLen")
            Return InputArray
            'Exit Function
        End If

        'Result[0 to 3]: header/identifier "HE3\r"
        ReDim Result(0 To 522)
        Result(0) = 72
        Result(1) = 69
        Result(2) = 51
        Result(3) = 13
        ResultLen = 4

        For i = 0 To (ByteLen - 1)
            CharCount(InputArray(i)) = CharCount(InputArray(i)) + 1
        Next

        For i = 0 To MaxNodes - 1
            Nodes(i) = New HuffmanTree()
        Next

        For i = 0 To 255
            If (CharCount(i) > 0) Then
                With Nodes(NodesCount)
                    .Weight = CharCount(i)
                    .Symbol = i
                    .LeftNode = -1
                    .RightNode = -1
                    .ParentNode = -1
                End With
                NodesCount = NodesCount + 1
                SymbolCount2 = SymbolCount2 + 1
            End If
        Next

        For lNodes = NodesCount To 2 Step -1
            lNode1 = -1 : lNode2 = -1
            For i = 0 To (NodesCount - 1)
                If (Nodes(i).ParentNode = -1) Then
                    If (lNode1 = -1) Then
                        lWeight1 = Nodes(i).Weight
                        lNode1 = i
                    ElseIf (lNode2 = -1) Then
                        lWeight2 = Nodes(i).Weight
                        lNode2 = i
                    ElseIf (Nodes(i).Weight < lWeight1) Then
                        If (Nodes(i).Weight < lWeight2) Then
                            If (lWeight1 < lWeight2) Then
                                lWeight2 = Nodes(i).Weight
                                lNode2 = i
                            Else
                                lWeight1 = Nodes(i).Weight
                                lNode1 = i
                            End If
                        Else
                            lWeight1 = Nodes(i).Weight
                            lNode1 = i
                        End If
                    ElseIf (Nodes(i).Weight < lWeight2) Then
                        lWeight2 = Nodes(i).Weight
                        lNode2 = i
                    End If
                End If
            Next
            With Nodes(NodesCount)
                .Weight = lWeight1 + lWeight2
                .LeftNode = lNode1
                .RightNode = lNode2
                .ParentNode = -1
                .Symbol = -1
            End With

            Nodes(lNode1).ParentNode = NodesCount
            Nodes(lNode2).ParentNode = NodesCount
            NodesCount = NodesCount + 1
        Next

        For i = 0 To 255
            SymbolBitSequence(i) = New ByteArray()
        Next

        Bytes = New ByteArray() ' (needed because bytes is a class instead of a structure/type)

        ReDim Bytes.Data(0 To 255)
        If (NodesCount > 0) Then 'Minor Bugfix: check NodesCount > 0 to allow huffman encoding zero-length messages
            Call CreateBitSequences(Nodes, NodesCount - 1, Bytes, SymbolBitSequence)
        End If

        'one-symbol bugfix:
        If (SymbolCount2 = 1) Then
            ' only one symbol (e.g. compressing "aaaaa")
            ' Without this, its node will have an empty bitsequence for some reason.
            ' Make it 1 bit long (a single 0)
            SymbolBitSequence(InputArray(0)).Count = 1
        End If

        ' Calculate Huffman Encoded Message Length, initially in bits, later changed to bytes(excludes huffman index or table)
        For i = 0 To 255
            If (CharCount(i) > 0) Then
                HEncodedMessageLen = HEncodedMessageLen + SymbolBitSequence(i).Count * CharCount(i)
                Debug($"char {i}: {CharCount(i)}x {SymbolBitSequence(i).Count} bits ({SymbolBitSequence(i).Count / 8} bytes)")
            End If
        Next
        HEncodedMessageLen = IIf(HEncodedMessageLen Mod 8 = 0, HEncodedMessageLen \ 8, HEncodedMessageLen \ 8 + 1)

        If Encoding = MessageType.NoCompression Or (Encoding = MessageType.Auto And ((HEncodedMessageLen = 0) Or (HEncodedMessageLen > ByteLen))) Then
            ' Huffman compression doesn't improve size or no compression requested.
            ' Send as original bytes with "HE0\r" header.

            ' TODO: (optimization) Do this sooner for MessageType.NoCompression 

            ReDim Result(0 To ByteLen + 3)
            Array.Copy(InputArray, 0, Result, 4, ByteLen)

            Result(0) = 72
            Result(1) = 69
            Result(2) = 48
            Result(3) = 13
            Debug($"HE0; lLength:{HEncodedMessageLen} > ByteLen:{ByteLen}; Encoding:{Encoding}")
            Return Result
        End If

        Checksum = 0
        For i = 0 To (ByteLen - 1)
            Checksum = Checksum Xor InputArray(i)
        Next
        'Result[5]: all input bytes of the uncompressed message without header XOR'd togethered as a "CRC" check (kind of like parity bits per bit position)
        Result(ResultLen) = Checksum
        ResultLen = ResultLen + 1

        'Result[6-9]: length of decoded message in bytes
        'Call CopyMem(Result(ResultLen), ByteLen, 4)
        Array.Copy(BitConverter.GetBytes(Convert.ToUInt32(ByteLen)), 0, Result, ResultLen, 4)
        Debug($"Encode ByteLen:{ByteLen}")
        ResultLen = ResultLen + 4

        BitValue(0) = 2 ^ 0
        BitValue(1) = 2 ^ 1
        BitValue(2) = 2 ^ 2
        BitValue(3) = 2 ^ 3
        BitValue(4) = 2 ^ 4
        BitValue(5) = 2 ^ 5
        BitValue(6) = 2 ^ 6
        BitValue(7) = 2 ^ 7
        SymbolCount = 0
        For i = 0 To 255
            'This count is redundant now. Already have SymbolCount2. 
            'if we allow nulls then need to use:
            'If (CharValue(i) IsNot Nothing AndAlso CharValue(i).Count > 0) Then SymbolCount = SymbolCount + 1
            If (SymbolBitSequence(i).Count > 0) Then SymbolCount = SymbolCount + 1
            'If (CharCount(i) > 0) Then SymbolCount = SymbolCount + 1
        Next

        'Call CopyMem(Result(ResultLen), Count, 2)
        Array.Copy(BitConverter.GetBytes(Convert.ToUInt16(SymbolCount)), 0, Result, ResultLen, 2)
        Debug($"Encoding count :{SymbolCount}")
        Debug($"Encoding count2:{SymbolCount2}")
        ResultLen = ResultLen + 2
        'Result[10-11]: Number of nodes in Huffman tree (= how many unique bytes appear in message)

        HuffmanIndexBitLenAndSome = 0 ' to for length (in bits) of huffman symbol table that goes at the end of message
        For i = 0 To 255
            If (SymbolBitSequence(i).Count > 0) Then
                'Result[12]: First character (0-255) 
                'Result[13]: Number of bits in its Huffman encoding
                'Continue up to Result[524] (theoretical max if all characters are used)
                Result(ResultLen) = i
                ResultLen = ResultLen + 1
                Result(ResultLen) = SymbolBitSequence(i).Count
                ResultLen = ResultLen + 1

                ' 16 bits for this index, 
                ' then a count of how often the latter appears? adding that many bits? what?
                ' Not sure if this is meant to measure the size of the message again (HEncodedMessageLen)
                ' or if it's meant to be for the huffman table at the end? But I think it's the wrong value? Not sure
                ' Also lacks the fancy rounding of elsewhere:
                ' HEncodedMessageLen = IIf(HEncodedMessageLen Mod 8 = 0, HEncodedMessageLen \ 8, HEncodedMessageLen \ 8 + 1)
                HuffmanIndexBitLenAndSome = HuffmanIndexBitLenAndSome + 16 + SymbolBitSequence(i).Count
            End If
        Next


        ReDim Preserve Result(0 To ResultLen + HuffmanIndexBitLenAndSome \ 8)

        ' Huffman encoded message
        BitPos = 0
        TempByte = 0
        For i = 0 To 255
            With SymbolBitSequence(i)
                If (.Count > 0) Then
                    For j = 0 To (.Count - 1)
                        If (.Data(j)) Then TempByte = TempByte + BitValue(BitPos)
                        BitPos = BitPos + 1
                        If (BitPos = 8) Then
                            Result(ResultLen) = TempByte
                            ResultLen = ResultLen + 1
                            TempByte = 0
                            BitPos = 0
                        End If
                    Next
                End If
            End With
        Next
        If (BitPos > 0) Then
            Result(ResultLen) = TempByte ' write final byte of message (0 padded)
            ResultLen = ResultLen + 1
        End If

        ' resize again!?
        ReDim Preserve Result(0 To ResultLen - 1 + HEncodedMessageLen)

        ' Create Huffman table: each valid character one after another.
        TempByte = 0
        ' or is it the huffman codes listed out?
        BitPos = 0
        For i = 0 To (ByteLen - 1)
            With SymbolBitSequence(InputArray(i))
                For j = 0 To (.Count - 1)
                    If (.Data(j) = 1) Then TempByte = TempByte + BitValue(BitPos)
                    BitPos = BitPos + 1
                    If (BitPos = 8) Then
                        Result(ResultLen) = TempByte
                        ResultLen = ResultLen + 1
                        BitPos = 0
                        TempByte = 0
                    End If
                Next
            End With
        Next
        If (BitPos > 0) Then
            Result(ResultLen) = TempByte
            ResultLen = ResultLen + 1
        End If
        ReDim InputArray(0 To ResultLen - 1)
        'Call CopyMem(ByteArray(0), Result(0), ResultLen)
        Array.Copy(Result, 0, InputArray, 0, ResultLen) ' TODO: just return Result?

        'EncodeByte = Result

        'Originally copied result back over InputArray:
        'ReDim ByteArray(0 To ResultLen - 1) ' originally
        'Call CopyMem(InputArray(0), Result(0), ResultLen)

        ReDim Preserve Result(0 To ResultLen - 1)
        Debug("success2")
        'Return Result
        Return InputArray
    End Function

    Private Function DecodeString(Text As String) As String
        ' this function doesn't work

        Dim ByteArray() As Byte
        'ByteArray() = StrConv(Text, vbFromUnicode)
        ByteArray = System.Text.Encoding.GetEncoding(1252).GetBytes(Text)
        Call DecodeByte(ByteArray, Len(Text))
        'DecodeString = StrConv(ByteArray(), vbUnicode)
        DecodeString = System.Text.Encoding.GetEncoding(1252).GetString(ByteArray)

    End Function
    Private Function EncodeString(Text As String) As String
        ' this function doesn't work

        Dim ByteArray() As Byte
        'ByteArray() = StrConv(Text, vbFromUnicode)
        ByteArray = System.Text.Encoding.GetEncoding(1252).GetBytes(Text)
        Call EncodeBytes(ByteArray, Len(Text))
        'EncodeString = StrConv(ByteArray(), vbUnicode)
        EncodeString = System.Text.Encoding.GetEncoding(1252).GetString(ByteArray)
    End Function

    'Decode routine 
    Public Function DecodeByte(InputBytes() As Byte, Optional ByteLen As Long = -1) As Byte()
        Dim i As Long, j As Long, Pos As Long, CharC As Byte, CurrPos As Long
        Dim Count As Integer, CheckSum As Byte, Result() As Byte, BitPos As Integer
        Dim NodeIndex As Long, ByteValue As Byte, ResultLen As Long, NodesCount As Long
        Dim lResultLen As Long, BitValue(0 To 7) As Byte
        Dim Nodes(0 To MaxNodes) As HuffmanTree ' 
        Dim SymbolBitSequence(0 To 255) As ByteArray
        If (ByteLen = -1) Then
            ByteLen = InputBytes.Length
        End If

        ' check for header: "HE0\r" or "HE3\r"
        If (InputBytes(0) <> 72) Or (InputBytes(1) <> 69) Or (InputBytes(3) <> 13) Then

        ElseIf (InputBytes(2) = 48) Then

            'Call CopyMem(ByteArray(0), ByteArray(4), ByteLen - 4)
            'Array.Copy(InputBytes, 4, InputBytes, 0, ByteLen - 4)
            'ReDim Preserve InputBytes(0 To ByteLen - 5)
            ReDim Result(0 To ByteLen - 5)
            Array.Copy(InputBytes, 4, Result, 0, ByteLen - 4)
            'Exit Sub
            Debug("successful decode null string or HE0")
            Return Result

        ElseIf (InputBytes(2) <> 51) Then
            Const ErrorDescription As String = "The data either was not compressed with HE3 or is corrupt (identification string not found)"
            Err.Raise(vbObjectError, "HuffmanDecode()", ErrorDescription)
            'Exit Sub
            Debug("decode fail 5 no header")
            Return New Byte() {}

        End If

        CurrPos = 5
        CheckSum = InputBytes(CurrPos - 1)
        CurrPos = CurrPos + 1

        'Call CopyMem(ResultLen, ByteArray(CurrPos - 1), 4)
        'Array.Copy(ByteArray, CurrPos - 1, resultLenBytes, 0, 4) ' buggy
        ResultLen = BitConverter.ToInt32(InputBytes, CurrPos - 1)
        Debug($"Decoded ResultLen:{ResultLen}")


        CurrPos = CurrPos + 4
        lResultLen = ResultLen
        'If (ResultLen = 0) Then Exit Sub
        If (ResultLen = 0) Then
            Debug("0 length huffman message")
            Return New Byte() {} ' minor bugfix: allow 0 length huffman message
            'Return InputBytes ' original: why?
        End If
        ReDim Result(0 To ResultLen - 1)
        'Call CopyMem(Count, ByteArray(CurrPos - 1), 2)
        'Array.Copy(InputBytes, CurrPos - 1, BitConverter.GetBytes(Count), 0, 2) ' buggy
        Count = BitConverter.ToUInt16(InputBytes, CurrPos - 1)
        Debug($"Decoded Count:{Count}")

        CurrPos = CurrPos + 2

        'TODO: optimization: don't declare all when don't need them all
        For i = 0 To 255
            SymbolBitSequence(i) = New ByteArray()
        Next

        For i = 0 To MaxNodes
            Nodes(i) = New HuffmanTree()
        Next

        For i = 1 To Count
            With SymbolBitSequence(InputBytes(CurrPos - 1))
                CurrPos = CurrPos + 1
                .Count = InputBytes(CurrPos - 1)
                CurrPos = CurrPos + 1
                ReDim .Data(0 To .Count - 1)
            End With
        Next

        BitValue(0) = 2 ^ 0
        BitValue(1) = 2 ^ 1
        BitValue(2) = 2 ^ 2
        BitValue(3) = 2 ^ 3
        BitValue(4) = 2 ^ 4
        BitValue(5) = 2 ^ 5
        BitValue(6) = 2 ^ 6
        BitValue(7) = 2 ^ 7
        Debug($"CurrPos: {CurrPos}; InputBytes.Len:{InputBytes.Length}")
        ByteValue = InputBytes(CurrPos - 1)
        CurrPos = CurrPos + 1
        BitPos = 0

        For i = 0 To 255
            With SymbolBitSequence(i)
                If (.Count > 0) Then
                    For j = 0 To (.Count - 1)
                        If (ByteValue And BitValue(BitPos)) Then .Data(j) = 1
                        BitPos = BitPos + 1
                        If (BitPos = 8) Then
                            ByteValue = InputBytes(CurrPos - 1)
                            CurrPos = CurrPos + 1
                            BitPos = 0
                        End If
                    Next
                End If
            End With
        Next

        If (BitPos = 0) Then CurrPos = CurrPos - 1

        NodesCount = 1
        Nodes(0).LeftNode = -1
        Nodes(0).RightNode = -1
        Nodes(0).ParentNode = -1
        Nodes(0).Symbol = -1

        For i = 0 To 255
            NodesCount = CreateTree(Nodes, NodesCount, i, SymbolBitSequence(i))
        Next

        ResultLen = 0
        For CurrPos = CurrPos To ByteLen
            ByteValue = InputBytes(CurrPos - 1)
            For BitPos = 0 To 7
                If (ByteValue And BitValue(BitPos)) Then NodeIndex = Nodes(NodeIndex).RightNode Else NodeIndex = Nodes(NodeIndex).LeftNode
                If (Nodes(NodeIndex).Symbol > -1) Then
                    Result(ResultLen) = Nodes(NodeIndex).Symbol
                    ResultLen = ResultLen + 1
                    If (ResultLen = lResultLen) Then
                        Debug("DecodeFinished")
                        GoTo DecodeFinished
                    End If
                    NodeIndex = 0
                End If
            Next
        Next
DecodeFinished:
        CharC = 0
        For i = 0 To (ResultLen - 1)
            CharC = CharC Xor Result(i)
        Next
        If (CharC <> CheckSum) Then Err.Raise(vbObjectError, "clsHuffman.Decode()", "The data might be corrupted (checksum did not match expected value)")
        ReDim InputBytes(0 To ResultLen - 1)
        'Call CopyMem(ByteArray(0), Result(0), ResultLen)
        Debug($"Final ResultLen:{ResultLen}")
        Array.Copy(Result, 0, InputBytes, 0, ResultLen)

        Debug("success")
        Return InputBytes
    End Function

    Private Sub Debug(text As String)
        If ShowDebug Then
            Console.WriteLine("VHuffmanVBNet debug: " + text)
        End If
    End Sub

    Private Sub CreateBitSequences(Nodes() As HuffmanTree, ByVal NodeIndex As Integer, Bytes As ByteArray, SymbolBitSequence() As ByteArray)
        Dim NewBytes As ByteArray
        Dim CNode As HuffmanTree = Nodes(NodeIndex) ' Current Node

        If (CNode.Symbol > -1) Then
            SymbolBitSequence(CNode.Symbol) = Bytes
            Exit Sub
        End If
        If (CNode.LeftNode > -1) Then
            NewBytes = Bytes.Clone()
            NewBytes.Data(NewBytes.Count) = 0
            NewBytes.Count = NewBytes.Count + 1
            Call CreateBitSequences(Nodes, CNode.LeftNode, NewBytes, SymbolBitSequence)
        End If
        If (CNode.RightNode > -1) Then
            NewBytes = Bytes.Clone()
            NewBytes.Data(NewBytes.Count) = 1
            NewBytes.Count = NewBytes.Count + 1
            Call CreateBitSequences(Nodes, CNode.RightNode, NewBytes, SymbolBitSequence)
        End If
    End Sub

End Class
