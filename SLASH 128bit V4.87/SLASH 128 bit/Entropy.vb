Imports System.Text
Imports System.Threading
Module Entropy
    Dim milli_ticks As UInt64 = 0
    Public entropy_pool(31) As Byte
    Public mouse_cord As String = ""
    Sub Entropy_generator()
        Dim True_entropy As New ARX_hash_function
        Dim looped As Boolean = False
        While True
            Dim temp_entropy As New ARX_hash_function
            Dim entropy_string_byte As Byte() = Encoding.UTF8.GetBytes(mouse_cord & milli_ticks.ToString & (DateTime.UtcNow - New DateTime(1970, 1, 1)).TotalMilliseconds)
            Dim entropy_bytes(31 + entropy_string_byte.Length) As Byte
            entropy_pool.CopyTo(entropy_bytes, 0)
            entropy_string_byte.CopyTo(entropy_bytes, 32)
            temp_entropy.Generate_New_SLASH_HASH(entropy_bytes)
            If looped = False Then
                True_entropy.Generate_New_SLASH_HASH(temp_entropy.hash)
                looped = True
            Else
                True_entropy.feed_hash(temp_entropy.hash)
            End If

            entropy_pool = True_entropy.hash
            Try
                milli_ticks += 1
            Catch ex As Exception
                milli_ticks = 0
            End Try
            Thread.Sleep(1)
        End While
    End Sub
End Module
