Public Class slash_hash_function
    Public hash(31) As Byte
    Dim hash_rotbox(79) As Integer
    Dim internal_state(79) As Byte
    Dim bitrate As Integer = 32
    Dim worked = False

    '//256 bit sponge function with 384 bit capacity
    '//total internal state: 640 bits
    Function SLASH_HASH_FUNCTION()
        Dim ti_state(79) As Byte
        Dim hash_sbox As Byte() = {115, 126, 229, 198, 22, 174, 114, 131, 1, 34, 94, 223, 215, 163, 101, 166, 89, 44, 252, 217, 185, 222, 16, 59,
            112, 53, 93, 45, 63, 97, 20, 156, 241, 61, 250, 108, 147, 145, 207, 64, 193, 148, 240, 69, 104, 25, 74, 70, 230, 205, 138, 246, 133,
            170, 107, 254, 226, 2, 76, 162, 206, 52, 218, 51, 150, 82, 49, 188, 37, 216, 203, 204, 85, 187, 33, 40, 201, 117, 237, 60, 214, 77,
            18, 65, 244, 110, 13, 55, 236, 199, 15, 116, 122, 29, 161, 167, 17, 155, 168, 132, 135, 27, 41, 253, 96, 87, 176, 120, 36, 105, 211,
            57, 178, 179, 180, 139, 165, 191, 224, 30, 56, 225, 62, 130, 11, 219, 79, 255, 38, 71, 183, 182, 142, 19, 189, 184, 35, 157, 181, 249,
            67, 152, 186, 149, 144, 221, 245, 95, 111, 81, 75, 154, 175, 232, 21, 141, 125, 169, 146, 137, 248, 23, 4, 43, 239, 8, 109, 84, 14, 78,
            160, 227, 243, 50, 172, 195, 31, 28, 194, 0, 212, 173, 213, 3, 143, 238, 68, 46, 140, 247, 151, 42, 127, 72, 136, 202, 10, 90, 92, 118,
            24, 233, 113, 124, 54, 231, 121, 164, 66, 159, 153, 190, 47, 123, 200, 86, 129, 251, 48, 83, 91, 158, 192, 12, 106, 5, 134, 208, 209, 26,
            32, 171, 73, 128, 235, 100, 177, 103, 210, 39, 196, 102, 7, 242, 80, 234, 228, 58, 99, 119, 98, 6, 9, 88, 197, 220}
        For roundno As Integer = 0 To 31
            Dim fl_stream(239) As Byte
            'ti_state = hextobyte(bytetohex(internal_state))
            internal_state.CopyTo(ti_state, 0)
            '##lfsr
            Array.Clear(fl_stream, 0, fl_stream.Length)
            internal_state.CopyTo(fl_stream, 160)
            For i As Integer = 0 To 159
                Dim t As Integer = 159 - i
                '1= 80,79,43,42
                fl_stream(t) = fl_stream(t + 80) Xor fl_stream((t + 79)) Xor fl_stream((t + 43)) Xor fl_stream((t + 42))
                internal_state(t Mod 80) = fl_stream(t)
            Next

            For i As Integer = 0 To 79
                '##byte rotate
                internal_state(i) = (internal_state(i) << hash_rotbox(i)) Or (internal_state(i) >> (8 - hash_rotbox(i)))
                '##sbox
                internal_state(i) = hash_sbox(internal_state(i))
                '##add previous state
                internal_state(i) = internal_state(i) Xor ti_state(i)
                '##get byte rotations
                hash_rotbox(i) = internal_state(i) Mod 8
            Next

        Next


        Return True
    End Function

    Public Function Generate_New_SLASH_HASH(input_data As Byte())
        Dim w_hash(bitrate - 1) As Byte
        worked = True
        If input_data.Length Mod bitrate > 0 Then
            Array.Resize(input_data, input_data.Length + (bitrate - (input_data.Length Mod bitrate)))
        End If
        If input_data.Length = 0 Then
            Array.Resize(input_data, bitrate)
        End If
        For i As Integer = 0 To (input_data.Length / bitrate) - 1
            For x As Integer = 0 To (bitrate - 1)
                internal_state(x) = internal_state(x) Xor input_data((bitrate * i) + x)
            Next
            SLASH_HASH_FUNCTION()
        Next
        Array.Copy(internal_state, w_hash, bitrate)
        w_hash.CopyTo(hash, 0)

        Return True
    End Function
    'Public Function Inject_Capacity(input As Byte())
    '    For i As Integer = 64 To 79
    '        internal_state(i) = input(i - 64)
    '    Next
    '    Return True
    'End Function
    'Public Function Release_Capacity()
    '    Dim capacity(15) As Byte
    '    For i As Integer = 64 To 79
    '        capacity(i - 64) = internal_state(i)
    '    Next
    '    Return capacity
    'End Function
    Public Function Blank_internal_state()
        worked = False
        bitrate = 32
        For i As Integer = 0 To 79
            internal_state(i) = 0
            hash_rotbox(i) = 0
        Next
        Return True
    End Function
    Public Function Inject_internal_state(input As Byte())
        For i As Integer = 0 To 79
            internal_state(i) = input(i)
            hash_rotbox(i) = internal_state(i) Mod 8
        Next
        Return True
    End Function
    Public Function Release_internal_state()
        Dim state(79) As Byte
        For i As Integer = 0 To 79
            state(i) = internal_state(i)
        Next
        Return state
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
                output(i) = internal_state(i)
            Next

        ElseIf size > bitrate Then
            For x As Integer = 0 To blockcount - 1
                For i As Integer = 0 To (bitrate - 1)
                    output((x * bitrate) + i) = internal_state(i)
                Next
                SLASH_HASH_FUNCTION()
            Next
        ElseIf size < bitrate Then
            For i As Integer = 0 To (bitrate - 1)
                output(i) = internal_state(i)
            Next
        End If
        Array.Resize(output, size)
        Return output
    End Function
    Public Function Set_Bitrate(size As Integer)
        If size > 80 Or size < 1 Then
            Throw New System.Exception("Invalid Bitrate.")
            Exit Function
        End If

        Blank_internal_state()
        bitrate = size
        Array.Resize(hash, size)
        Return True
    End Function
End Class
