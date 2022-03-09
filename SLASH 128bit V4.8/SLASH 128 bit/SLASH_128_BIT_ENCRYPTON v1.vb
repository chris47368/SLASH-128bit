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
    Dim entropy_pool(31) As Byte

    Public Function Initiate_Encryption(key As Byte(), data_l As Long, entropy As Byte())
        Dim enc As New SLASH_Key_Schedule
        enc.Intitate_Encryption(key)
        entropy_pool = entropy
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
    Public Function Gen_New_IV()

        'entropy gen and capture
        Dim iv(31) As Byte
        Dim ivhash As New ARX_hash_function
        For i As Integer = 0 To 31
            iv(i) = main_key(i) Xor entropy_pool(i)
        Next
        ivhash.Generate_New_SLASH_HASH(iv)
        iv = ivhash.Resize_Output(16)
        lastblocksize = lastblocksize Mod 16 'calculate last block size
        iv = Gen_salted_IV(iv, lastblocksize)


        iv = encrypt_function(iv)
        'prev_block = iv
        Array.Copy(iv, prev_block, 16)
        Return True
    End Function
    Function Gen_salted_IV(iv As Byte(), lastblocksize As Integer)

        'salt generation
        Dim salt(43) As Byte
        For i As Integer = 0 To 43
            If i < 12 Then
                salt(i) = iv(i)
            Else
                salt(i) = main_key(i - 12)
            End If
        Next
        Dim salthash As New ARX_hash_function
        salthash.Generate_New_SLASH_HASH(salt)
        'salt addition
        For i As Integer = 0 To 3
            iv(12 + i) = salthash.hash(i)
        Next
        'last block size location calc
        Dim lbs_loc As Byte = salthash.hash(4) Mod 8
        '##last block size insertion into salt
        If lbs_loc Mod 2 = 0 Then
            Dim salt_loc As Integer = lbs_loc / 2 'finds correct salt byte
            iv(12 + salt_loc) = iv(12 + salt_loc) Xor lastblocksize 'insert lbs into salt
        Else
            Dim salt_loc As Integer = (lbs_loc - 1) / 2 'finds correct salt byte
            lastblocksize = lastblocksize * 16 ' shifts lbs by 4 places into byte
            iv(12 + salt_loc) = iv(12 + salt_loc) Xor lastblocksize 'insert lbs into salt
        End If

        Return iv
    End Function
    Function verify_key(iv As Byte())
        iv = decrypt_function(iv)
        'salt generation
        Dim salt(43) As Byte
        For i As Integer = 0 To 43
            If i < 12 Then
                salt(i) = iv(i)
            Else
                salt(i) = main_key(i - 12)
            End If
        Next
        Dim salthash As New ARX_hash_function
        salthash.Generate_New_SLASH_HASH(salt)
        'salt subtraction
        Dim salt_count As Integer = 0
        For i As Integer = 0 To 3
            iv(12 + i) = iv(12 + i) Xor salthash.hash(i)
            If iv(12 + i) > 0 Then
                salt_count = salt_count + 1
                If salt_count > 1 Then
                    Return False
                    Exit Function
                End If
            End If
        Next
        'last block size location calc
        Dim lbs_loc As Byte = salthash.hash(4) Mod 8
        Try
            '##last block size insertion into salt
            If lbs_loc Mod 2 = 0 Then
                Dim salt_loc As Integer = lbs_loc / 2 'finds correct salt byte
                iv(12 + salt_loc) = lastblocksize 'reads lbs
                If lastblocksize > 15 Then
                    Return False
                    Exit Function
                End If
            Else
                Dim salt_loc As Integer = (lbs_loc - 1) / 2 'finds correct salt byte
                iv(12 + salt_loc) = iv(12 + salt_loc) Xor lastblocksize 'insert lbs into salt
                iv(12 + salt_loc) = lastblocksize 'reads raw lbs
                If lastblocksize < 16 And lastblocksize > 0 Then
                    Return False
                    Exit Function
                End If
                lastblocksize = lastblocksize / 16
            End If
        Catch ex As Exception
            Return False
            Exit Function
        End Try


        Return True
    End Function
End Class
