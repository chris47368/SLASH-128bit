Imports System.Threading
Imports System.Text
Public Class Start

    Dim milli_ticks As UInt64 = 0
    Public entropy_pool(31) As Byte
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        CText.Show()
        Me.Hide()
        If CheckBox1.Checked = True Then
            My.Settings.choice = True
            My.Settings.opt = False
            My.Settings.Save()
        End If
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        CFile.Show()
        Me.Hide()
        If CheckBox1.Checked = True Then
            My.Settings.choice = True
            My.Settings.opt = True
            My.Settings.Save()
        End If
    End Sub

    Private Sub CheckBox1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked = False Then
            My.Settings.choice = False
            My.Settings.Save()
        End If
    End Sub

    Private Sub Start_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        If My.Settings.choice = True Then
            CheckBox1.Checked = True
            If My.Settings.opt = True Then
                CFile.Show()
                Me.Hide()
            Else
                CText.Show()
                Me.Hide()
            End If
            Timer1.Start()
        End If

        Dim en As New Threading.Thread(AddressOf Entropy_generator)
        en.Priority = Threading.ThreadPriority.Highest
        en.Start()
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        If My.Settings.choice = True Then
            Me.Hide()
            Timer1.Stop()
        End If
    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick

    End Sub

    Private Sub Start_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        End
    End Sub
    Sub Entropy_generator()
        Dim new_hash As New ARX_hash_function
        Dim looped As Boolean = False
        While True
            Dim tick_String As String = milli_ticks.ToString
            Dim entropy_string_byte As Byte() = Encoding.UTF8.GetBytes(MousePosition.ToString & tick_String & (DateTime.Now - New DateTime(1970, 1, 1)).TotalMilliseconds)
            Dim entropy_bytes(31 + entropy_string_byte.Length) As Byte
            entropy_pool.CopyTo(entropy_bytes, 0)
            entropy_string_byte.CopyTo(entropy_bytes, 32)
            If looped = False Then
                new_hash.Generate_New_SLASH_HASH(entropy_bytes)
                looped = True
            Else
                new_hash.feed_hash(entropy_bytes)
            End If

            entropy_pool = new_hash.hash
            Try
                milli_ticks += 1
            Catch ex As Exception
                milli_ticks = 0
            End Try

            Thread.Sleep(1)
        End While
    End Sub
End Class