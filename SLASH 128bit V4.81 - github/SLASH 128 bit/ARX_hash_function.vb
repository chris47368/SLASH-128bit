Public Class ARX_hash_function
    Public hash(31) As Byte
    Dim hash_mix_table(,) As Integer = {{0, 1, 2, 3}, {0, 3, 2, 1}, {0, 2, 3, 1}, {0, 1, 3, 2}, {0, 2, 1, 3}, {0, 3, 1, 2}, {1, 2, 3, 0}, {1, 0, 3, 2}, {1, 3, 0, 2}, {1, 2, 0, 3}, {1, 3, 2, 0}, {1, 0, 2, 3}, {2, 3, 0, 1}, {2, 1, 0, 3}, {2, 0, 1, 3}, {2, 3, 1, 0}, {2, 0, 3, 1}, {2, 1, 3, 0}, {3, 0, 1, 2}, {3, 2, 1, 0}, {3, 1, 2, 0}, {3, 0, 2, 1}, {3, 1, 0, 2}, {3, 2, 0, 1}}
    Dim internal_state(3, 31) As Byte
    Dim mix_entropy(15) As Byte
    Dim bitrate As Integer = 32
    Dim worked = False
    Dim mix_value As Integer = 0

    '//256 bitrate sponge like ARX hash function with 768 bit capacity
    '//total internal state: 1024 bits
    Function ARX_HASH_FUNCTION()
        Dim preintstate(3, 31) As Byte
        For round As Integer = 0 To 31

            'copy internal state
            For i As Integer = 0 To 3
                For x As Integer = 0 To 31
                    preintstate(i, x) = internal_state(i, x)
                Next
            Next

            'xor mix entropy when aplicable
            If round = 7 Then
                For i As Integer = 0 To 3
                    For x As Integer = 15 To 19
                        mix_entropy((4 * i) + (x Mod 4)) = mix_entropy((4 * i) + (x Mod 4)) Xor internal_state(i, x)
                    Next
                Next
            End If
            'compute new mix entropy
            mix_entropy = Diffuse_hash(mix_entropy)
            mix_value = mix_entropy(0) Mod 24
            'mix row
            For i As Integer = 0 To 7
                Diff_8_Bit_Enc(internal_state(hash_mix_table(mix_value, 0), (((4 * i) + 0) Mod bitrate)), internal_state(hash_mix_table(mix_value, 0), (((4 * i) + 1) Mod bitrate)), internal_state(hash_mix_table(mix_value, 0), (((4 * i) + 2) Mod bitrate)), internal_state(hash_mix_table(mix_value, 0), (((4 * i) + 3) Mod bitrate)))
                Diff_8_Bit_Enc(internal_state(hash_mix_table(mix_value, 1), (((4 * i) + 1) Mod bitrate)), internal_state(hash_mix_table(mix_value, 1), (((4 * i) + 2) Mod bitrate)), internal_state(hash_mix_table(mix_value, 1), (((4 * i) + 3) Mod bitrate)), internal_state(hash_mix_table(mix_value, 1), (((4 * i) + 4) Mod bitrate)))
                Diff_8_Bit_Enc(internal_state(hash_mix_table(mix_value, 2), (((4 * i) + 2) Mod bitrate)), internal_state(hash_mix_table(mix_value, 2), (((4 * i) + 3) Mod bitrate)), internal_state(hash_mix_table(mix_value, 2), (((4 * i) + 4) Mod bitrate)), internal_state(hash_mix_table(mix_value, 2), (((4 * i) + 5) Mod bitrate)))
                Diff_8_Bit_Enc(internal_state(hash_mix_table(mix_value, 3), (((4 * i) + 3) Mod bitrate)), internal_state(hash_mix_table(mix_value, 3), (((4 * i) + 4) Mod bitrate)), internal_state(hash_mix_table(mix_value, 3), (((4 * i) + 5) Mod bitrate)), internal_state(hash_mix_table(mix_value, 3), (((4 * i) + 6) Mod bitrate)))
            Next
            'mix column
            For i As Integer = 0 To bitrate - 1
                Diff_8_Bit_Enc(internal_state(hash_mix_table(mix_value, 0), i), internal_state(hash_mix_table(mix_value, 1), i), internal_state(hash_mix_table(mix_value, 2), i), internal_state(hash_mix_table(mix_value, 3), i))
            Next
        Next

        'xor
        For i As Integer = 0 To 3
            For x As Integer = 0 To 31
                internal_state(i, x) = preintstate(i, x) Xor internal_state(i, x)
            Next
        Next
        Return True
    End Function

    Public Function Generate_New_SLASH_HASH(input_data As Byte())
        Dim w_hash(bitrate - 1) As Byte
        Blank_internal_state()
        worked = True
        If input_data.Length Mod bitrate > 0 Then
            Array.Resize(input_data, input_data.Length + (bitrate - (input_data.Length Mod bitrate)))
        End If
        If input_data.Length = 0 Then
            Array.Resize(input_data, bitrate)
        End If
        For i As Integer = 0 To (input_data.Length / bitrate) - 1
            For x As Integer = 0 To 31
                internal_state(0, x) = internal_state(0, x) Xor input_data((bitrate * i) + x)
            Next
            ARX_HASH_FUNCTION()
        Next
        For i As Integer = 0 To 31
            w_hash(i) = internal_state(0, i)
        Next
        w_hash.CopyTo(hash, 0)

        Return True
    End Function

    Public Function Blank_internal_state()
        worked = False
        bitrate = 32
        For i As Integer = 0 To 3
            For x As Integer = 0 To 31
                internal_state(i, x) = (((32 * i) + x) * 2)
                If x Mod 2 = 1 Then
                    internal_state(i, x) = internal_state(i, x) + 1
                End If
            Next
        Next
        Return True
    End Function
    Public Function Inject_internal_state(input As Byte(,))
        internal_state = input
        Return True
    End Function
    Public Function Release_internal_state()

        Return internal_state
    End Function
    Public Function Resize_Output(size As Integer)
        If worked = False Then
            Throw New System.Exception("Hash Not initiated.")
            Exit Function
        End If
        Dim output(size - 1) As Byte
        Dim blockcount As Integer = contoint(size / bitrate)


        '####resize to compatable internal size
        If output.Length Mod bitrate > 0 Then
            Array.Resize(output, output.Length + (bitrate - (output.Length Mod bitrate)))
        End If
        '##########################

        If size = bitrate Then

            For i As Integer = 0 To (bitrate - 1)
                output(i) = internal_state(0, i)
            Next

        ElseIf size > bitrate Then
            For x As Integer = 0 To blockcount - 1
                For i As Integer = 0 To (bitrate - 1)
                    output((x * bitrate) + i) = internal_state(0, i)
                Next
                ARX_HASH_FUNCTION()
            Next
        ElseIf size < bitrate Then
            For i As Integer = 0 To (bitrate - 1)
                output(i) = internal_state(0, i)
            Next
        End If
        Array.Resize(output, size)
        Return output
    End Function

End Class
