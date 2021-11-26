Imports System.Text
Public Class SLASH_128_BIT_ENCRYPTON
    Dim r_count = 10
    Dim Sbox(r_count - 1, 255) As Byte
    Dim P_Box(r_count - 1, 15) As Byte
    Dim Round_Key(r_count - 1, 15) As Byte
    Public prev_block(15) As Byte
    Dim main_key(31) As Byte
    Dim ver_const As Byte = 170
    Public lastblocksize As Long = 0
    Public t_pb(15) As Byte
    Dim t_stream(15) As Byte

    Public Function Initiate_Encryption(key As Byte(), data_l As Long)
        Dim enc As New SLASH_Key_Schedule
        enc.Intitate_Encryption(key)
        Sbox = enc.SBOX
        P_Box = enc.PBOX
        Round_Key = enc.R_KEY
        main_key = enc.M_KEY
        Gen_New_IV()
        If data_l Mod 16 > 0 Then
            lastblocksize = data_l Mod 16
        End If
        Return True
    End Function
    Public Function Initiate_Decryption(key As Byte())
        Dim dec As New SLASH_Key_Schedule
        dec.Initiate_Decryption(key)
        Sbox = dec.SBOX
        P_Box = dec.PBOX
        Round_Key = dec.R_KEY
        main_key = dec.M_KEY
        'prev_block = IV
        Dim verify As Boolean = verify_key(hextobyte(bytetohex(prev_block)))
        If verify = False Then
            Throw New System.Exception("Incorrect Key.")
            Exit Function
        End If
        Return True
    End Function
    Function encrypt_function(ByVal input As Byte())
        ' Dim input As Byte() = BitConverter.GetBytes(sinput)

        For roundno As Integer = 0 To r_count - 1
            '##diffusion

            input = Diffuse_Enc(input)
            'permute
            input.CopyTo(t_stream, 0)
            For i As Integer = 0 To 15
                input(i) = t_stream(P_Box(roundno, i))
            Next
            For i As Integer = 0 To 15
                '##add round key
                input(i) = input(i) Xor Round_Key(roundno, i)
                '##sbox
                input(i) = Sbox(roundno, input(i))
            Next
        Next
        ' Dim output As UInt64 = BitConverter.ToUInt64(input, 0)
        Return input
    End Function
    Function decrypt_function(input As Byte())
        'Dim input As Byte() = BitConverter.GetBytes(sinput)
        For x As Integer = 0 To r_count - 1
            Dim roundno As Integer = (r_count - 1) - x

            For i As Integer = 0 To 15
                '##sbox
                input(i) = Sbox(roundno, input(i))
                '##add round key
                input(i) = input(i) Xor Round_Key(roundno, i)


            Next
            'permute
            input.CopyTo(t_stream, 0)
            For i As Integer = 0 To 15
                input(i) = t_stream(P_Box(roundno, i))
            Next

            '##diffusion
            input = Diffuse_Dec(input)
        Next
        Return input
    End Function
    Public Function Encrypt_Block(input As Byte())
        Array.Resize(input, 16)
        For i As Integer = 0 To 15
            input(i) = prev_block(i) Xor input(i)
        Next
        input = encrypt_function(input)
        Array.Copy(input, prev_block, 16)
        Return input
    End Function
    Public Function Decrypt_Block(input As Byte())

        Array.Copy(input, t_pb, 16)
        input = decrypt_function(input)
        For i As Integer = 0 To 15
            input(i) = prev_block(i) Xor input(i)
        Next
        Array.Copy(t_pb, prev_block, 16)
        Return input
    End Function
    Function verify_key(ByVal input As Byte())
        input = decrypt_function(input)
        For i As Integer = 0 To 1
            If input(i) <> ver_const Then
                Return False
                Exit Function
            End If
        Next
        lastblocksize = input(2)
        If lastblocksize > 15 Then
            Return False
            Exit Function
        End If
        Return True
    End Function
    Public Function Gen_New_IV()
        Dim iv As Byte() = Encoding.UTF8.GetBytes((DateTime.Now - New DateTime(1970, 1, 1)).TotalMilliseconds)
        Dim ivhash As New slash_hash_function
        ivhash.Generate_New_SLASH_HASH(iv)
        iv = ivhash.hash
        For i As Integer = 0 To 15
            iv(i) = main_key(i) Xor iv(i)
        Next
        ivhash.Generate_New_SLASH_HASH(iv)
        iv = ivhash.Resize_Output(16)
        iv(0) = ver_const
        iv(1) = ver_const
        lastblocksize = lastblocksize Mod 16
        iv(2) = lastblocksize
        iv = encrypt_function(iv)
        'prev_block = iv
        Array.Copy(iv, prev_block, 16)
        Return True
    End Function
End Class
