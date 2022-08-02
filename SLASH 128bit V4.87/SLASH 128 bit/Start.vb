Imports System.Threading
Imports System.Text
Public Class Start


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

        Dim en As New Threading.Thread(AddressOf Entropy.Entropy_generator)
        en.Priority = Threading.ThreadPriority.Highest
        en.Start()

        Dim mouse_pos As New Threading.Thread(AddressOf mouse_position)
        mouse_pos.Priority = Threading.ThreadPriority.Highest
        mouse_pos.Start()
    End Sub
    Sub mouse_position()
        While True
            Entropy.mouse_cord = MousePosition.ToString
            Thread.Sleep(1)
        End While
    End Sub
    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        If My.Settings.choice = True Then
            Me.Hide()
            Timer1.Stop()
        End If
    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        mouse_cord = MousePosition.ToString
    End Sub

    Private Sub Start_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        End
    End Sub

End Class