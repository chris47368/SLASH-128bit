Module Diffusion
    Function Diff_8_Bit_Enc(ByRef a As Byte, ByRef b As Byte, ByRef c As Byte, ByRef d As Byte)
        Dim t As Byte = 0
        'b = a,d
        t = ((a + d) << 1) Or ((a + d) >> 8 - 1)
        b = b Xor t
        'c= a,b
        t = ((a + b) << 3) Or ((a + b) >> 8 - 3)
        c = c Xor t
        'd= b,c
        t = ((b + c) << 5) Or ((b + c) >> 8 - 5)
        d = d Xor t
        'a= c,d
        t = ((c + d) << 7) Or ((c + d) >> 8 - 7)
        a = a Xor t
        Return True
    End Function
    Function Diffuse_Enc(ByVal input As Byte())
        'even round
        Diff_8_Bit_Enc(input(0), input(1), input(2), input(3))
        Diff_8_Bit_Enc(input(4), input(5), input(6), input(7))
        Diff_8_Bit_Enc(input(8), input(9), input(10), input(11))
        Diff_8_Bit_Enc(input(12), input(13), input(14), input(15))
        'odd round
        Diff_8_Bit_Enc(input(0), input(4), input(8), input(12))
        Diff_8_Bit_Enc(input(1), input(5), input(9), input(13))
        Diff_8_Bit_Enc(input(2), input(6), input(10), input(14))
        Diff_8_Bit_Enc(input(3), input(7), input(11), input(15))
        Return input
    End Function
    Function Diff_8_Bit_Dec(ByRef a As Byte, ByRef b As Byte, ByRef c As Byte, ByRef d As Byte)
        Dim t As Byte = 0
        'a= c,d
        t = ((c + d) << 7) Or ((c + d) >> 8 - 7)
        a = a Xor t
        'd= b,c
        t = ((b + c) << 5) Or ((b + c) >> 8 - 5)
        d = d Xor t
        'c= a,b
        t = ((a + b) << 3) Or ((a + b) >> 8 - 3)
        c = c Xor t
        'b = a,d
        t = ((a + d) << 1) Or ((a + d) >> 8 - 1)
        b = b Xor t
        Return True
    End Function
    Function Diffuse_Dec(ByVal input As Byte())

        'odd round
        Diff_8_Bit_Dec(input(3), input(7), input(11), input(15))
        Diff_8_Bit_Dec(input(2), input(6), input(10), input(14))
        Diff_8_Bit_Dec(input(1), input(5), input(9), input(13))
        Diff_8_Bit_Dec(input(0), input(4), input(8), input(12))
        'even round
        Diff_8_Bit_Dec(input(12), input(13), input(14), input(15))
        Diff_8_Bit_Dec(input(8), input(9), input(10), input(11))
        Diff_8_Bit_Dec(input(4), input(5), input(6), input(7))
        Diff_8_Bit_Dec(input(0), input(1), input(2), input(3))
        Return input
    End Function
    Function Diffuse_hash(ByVal input As Byte())
        Dim t_input(15) As Byte
        'copy input
        For i As Integer = 0 To 15
            t_input(i) = input(i)
        Next
        'even round
        Diff_8_Bit_Enc(input(0), input(1), input(2), input(3))
        Diff_8_Bit_Enc(input(4), input(5), input(6), input(7))
        Diff_8_Bit_Enc(input(8), input(9), input(10), input(11))
        Diff_8_Bit_Enc(input(12), input(13), input(14), input(15))
        'odd round
        Diff_8_Bit_Enc(input(0), input(4), input(8), input(12))
        Diff_8_Bit_Enc(input(1), input(5), input(9), input(13))
        Diff_8_Bit_Enc(input(2), input(6), input(10), input(14))
        Diff_8_Bit_Enc(input(3), input(7), input(11), input(15))
        'xor input
        For i As Integer = 0 To 15
            input(i) = input(i) Xor t_input(i)
        Next
        Return input
    End Function
End Module
