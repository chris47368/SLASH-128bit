Public Class SLASH_Key_Schedule
    Dim roundno As Integer = 10
    Public SBOX(roundno - 1, 255) As Byte
    Public PBOX(roundno - 1, 15) As Byte
    Public R_KEY(roundno - 1, 15) As Byte
    Public M_KEY(31) As Byte



    Public Function Intitate_Encryption(input_key As Byte())
        Inject_Key(input_key)
        Gen_SBOX()
        Gen_PBOX()
        Gen_R_KEY()
        Return True
    End Function
    Public Function Initiate_Decryption(input_key As Byte())
        Inject_Key(input_key)
        Gen_SBOX()
        Gen_PBOX()
        Gen_R_KEY()
        SBOX = invertsbox(SBOX)
        PBOX = invertpbox(PBOX)
        Return True
    End Function
    Function Inject_Key(data As Byte())
        Dim genNewkey As New ARX_hash_function
        genNewkey.Generate_New_SLASH_HASH(data)
        M_KEY = genNewkey.hash
        Return True
    End Function
    Function Gen_SBOX()
        Dim SBOX_KEY As Byte() = M_KEY
        Dim sbox_Generator As New ARX_hash_function
        SBOX_KEY = idkey(SBOX_KEY, "SBOX_KEY")
        sbox_Generator.Generate_New_SLASH_HASH(SBOX_KEY)
        SBOX_KEY = sbox_Generator.hash
        For y As Integer = 0 To roundno - 1
            Dim count As Integer = 0
            Dim check(255) As Boolean
            While count < 256

                For i As Integer = 0 To 31
                    If check(SBOX_KEY(i)) = False Then
                        SBOX(y, count) = SBOX_KEY(i)
                        check(SBOX_KEY(i)) = True
                        count += 1
                    End If
                Next
                sbox_Generator.Generate_New_SLASH_HASH(SBOX_KEY)
                SBOX_KEY = sbox_Generator.hash
            End While
            count = 0
        Next
        Return True
    End Function
    Function Gen_PBOX()
        Dim PBOX_KEY As Byte() = M_KEY
        Dim pbox_Generator As New ARX_hash_function
        PBOX_KEY = idkey(PBOX_KEY, "PBOX_KEY")
        pbox_Generator.Generate_New_SLASH_HASH(PBOX_KEY)
        PBOX_KEY = pbox_Generator.hash
        For y As Integer = 0 To roundno - 1
            Dim count As Integer = 0
            Dim check(15) As Boolean
            While count < 16

                For i As Integer = 0 To 31
                    If check(PBOX_KEY(i) Mod 16) = False Then
                        PBOX(y, count) = PBOX_KEY(i) Mod 16
                        check(PBOX_KEY(i) Mod 16) = True
                        count += 1
                    End If
                Next
                pbox_Generator.Generate_New_SLASH_HASH(PBOX_KEY)
                PBOX_KEY = pbox_Generator.hash
            End While
            count = 0
        Next
        Return True
    End Function
    Function Gen_R_KEY()
        Dim N_R_KEY As Byte() = M_KEY
        Dim rkey_Generator As New ARX_hash_function
        N_R_KEY = idkey(N_R_KEY, "ROUND_KEY")
        rkey_Generator.Generate_New_SLASH_HASH(N_R_KEY)
        N_R_KEY = rkey_Generator.Resize_Output(16 * roundno)
        For x As Integer = 0 To roundno - 1
            For i As Integer = 0 To 15
                R_KEY(x, i) = N_R_KEY((16 * x) + i)
            Next
        Next
        Return True
    End Function
    Function invertsbox(ByVal input As Byte(,))
        Dim insbox(roundno - 1, 255) As Byte
        For y As Integer = 0 To roundno - 1
            For x As Integer = 0 To 255
                'pointer=location
                'location=pointer
                Dim inp = input(y, x)
                insbox(y, inp) = x
            Next
        Next
        Return insbox
    End Function
    Function invertpbox(ByVal input As Byte(,))
        Dim inpbox(roundno - 1, 15) As Byte
        For y As Integer = 0 To roundno - 1
            For x As Integer = 0 To 15
                'pointer=location
                'location=pointer
                Dim inp = input(y, x)
                inpbox(y, inp) = x
            Next
        Next
        Return inpbox
    End Function
End Class
