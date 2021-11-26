Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading
Imports System.IO
Module Crypto_Func
    Function SHA256_gen(ByVal inputbytes As Byte()) As Byte()
        Dim sha256 As SHA256 = SHA256Managed.Create()
        'Dim bytes As Byte() = Encoding.UTF8.GetBytes(inputString)
        Dim hash As Byte() = sha256.ComputeHash(inputbytes)
        Dim stringBuilder As New StringBuilder()
        Return hash
    End Function
    Function hextobyte(ByVal input As String)
        Dim minp As String = input.Replace(" ", "")
        minp = minp.Replace(vbCr, "").Replace(vbLf, "")
        input.Trim()
        Dim output((minp.Length / 2) - 1) As Byte
        Dim a
        Dim x As Integer = 1
        While x < output.Length * 2
            a = Mid(minp, x, 2)
            output((x - 1) / 2) = Convert.ToByte(a, 16)
            x += 2
        End While
        Return output
    End Function
    Function bytetohex(ByVal hex As Byte())
        Dim stringBuilder As New StringBuilder()
        For i As Integer = 0 To hex.Length - 1
            stringBuilder.Append(hex(i).ToString("X2"))
        Next
        Return stringBuilder.ToString
    End Function
    Function contoint(ByVal input As String)
        Dim output As Integer
        If Integer.TryParse(input, output) Then
            Return CInt(input)
        Else
            Dim inp As String = input
            For i As Integer = 1 To input.Length
                If Mid(inp, i, 1) = "." Then
                    inp = Mid(inp, 1, i - 1)
                    Return CInt(inp) + 1
                End If
            Next
        End If
        Return 0
    End Function
    Function idkey(ByVal key As Byte(), ByVal ident As String)
        Dim pkey = key
        Dim keyid As Byte() = Encoding.UTF8.GetBytes(ident)
        For i As Integer = 0 To keyid.Length - 1
            pkey(i) = pkey(i) Xor keyid(i)
        Next
        Return pkey
    End Function
    Function permutate_block(inputblock As Byte(), blocksize As Integer, pbox As Byte())
        'Dim pbox As Byte() = hextobyte(perm_pbox)
        'Dim inputblock As Byte() = hextobyte(TextBox1.Text)
        Dim outputblock(inputblock.Length - 1) As Byte
        'Dim blocksize As Integer = 64
        Dim byte_count As Integer = blocksize / 8
        'For i As Integer = 0 To 7 'byte level
        '    For x As Integer = 0 To 7 'bit level

        '    Next
        'Next
        Dim newbitloc = 0
        Dim nx As Integer = 0
        Dim ny As Integer = 0
        Dim oldbitloc As Integer = 0
        Dim ox As Integer = 0
        Dim oy As Integer = 0
        For i As Integer = 0 To blocksize - 1
            newbitloc = pbox(i)
            nx = newbitloc Mod 8 'new bit loc
            ny = (newbitloc - nx) / byte_count ' new byte loc
            oldbitloc = i
            ox = oldbitloc Mod 8 'old byte loc
            oy = (oldbitloc - ox) / byte_count 'old bit loc
            outputblock(ny) = outputblock(ny) Or (((inputblock(ox) >> oy) And 1) << nx)
        Next
        Return outputblock
    End Function
End Module
