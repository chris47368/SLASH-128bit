Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading
Imports System.IO
Imports System.ComponentModel

Public Class CText
    Dim progress As Integer = 0
    Dim working As Boolean = False
    Dim s_file As String = ""
    Dim closeprogram = True
    Dim clicks As Integer = 0
    Dim Mouse_cords As String = "00"
    Dim Ticks As UInt64 = 0
    Dim entropy_pool(31) As Byte

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        RichTextBox2.Text = ""
        Dim en As New Threading.Thread(AddressOf crypt)
        en.Priority = Threading.ThreadPriority.Highest
        en.Start()
        Timer2.Start()
    End Sub

    Sub crypt()

        Dim Slash_crypt As New SLASH_128_BIT_ENCRYPTON
        Dim encrypt As Boolean = True
        Dim outputdata As Byte()
        Dim lastblocksize As Integer = 0
        Dim password As String = ""
        TextBox1.Invoke(Sub() password = TextBox1.Text)
        Lock_GUI()
        progress = 0
        Dim sw As Stopwatch = Stopwatch.StartNew
        RadioButton1.Invoke(Sub() encrypt = RadioButton1.Checked)
        Dim inputdata As Byte()
        Dim raw_text As String = ""
        RichTextBox1.Invoke(Sub() raw_text = RichTextBox1.Text)
        Dim r_input_length As Integer = raw_text.Length
        If encrypt = True Then
            Try
                inputdata = Encoding.UTF8.GetBytes(raw_text)
            Catch ex As Exception
                Return_GUI("No valid input.")
                Exit Sub
            End Try

        Else
            Try
                inputdata = Convert.FromBase64String(raw_text)
            Catch ex As Exception
                Return_GUI("No encrypted data.")
                Exit Sub
            End Try
            If inputdata.Length Mod 16 > 0 Then
                Return_GUI("No encrypted data.")
                Exit Sub
            End If
        End If

        'Dim iv As Byte() = {82, 134, 254, 90, 37, 112, 140, 192}


        If encrypt = True Then
            'initiate encryption

            If inputdata.Length Mod 16 > 0 Then
                lastblocksize = inputdata.Length Mod 16
                Slash_crypt.lastblocksize = lastblocksize
                Array.Resize(inputdata, inputdata.Length + (16 - lastblocksize))
            End If
            Array.Resize(outputdata, inputdata.Length + 16)
            Slash_crypt.Initiate_Encryption(Encoding.UTF8.GetBytes(password), r_input_length, entropy_pool)
        Else
            'initiate decryption
            For i As Integer = 0 To 15
                Slash_crypt.prev_block(i) = inputdata(i)
            Next
            Try
                Slash_crypt.Initiate_Decryption(Encoding.UTF8.GetBytes(password))
            Catch ex As Exception
                Return_GUI("Invalid Decryption Key.")
                Exit Sub
            End Try

            Array.Resize(outputdata, inputdata.Length - 16)
            lastblocksize = Slash_crypt.lastblocksize
        End If

        '######cryption

        If encrypt = True Then
            'write IV before writing encrypted data
            For i As Integer = 0 To 15

                outputdata(i) = Slash_crypt.prev_block(i)
            Next
            '#######main encryption loop
            For i As Integer = 0 To (inputdata.Length / 16) - 1
                If i > 0 Then
                    progress = (i / (inputdata.Length / 16)) * 100
                End If
                Dim block(15) As Byte
                '#####take 1 block data for encryption, xor prev block
                For x As Integer = 0 To 15
                    block(x) = inputdata((i * 16) + x)

                Next
                'encrypt block
                block = Slash_crypt.Encrypt_Block(block)
                ''apply current block for prev block
                'Slash_crypt.prev_block = block
                'write outputblock to output data
                For x As Integer = 0 To 15
                    outputdata(((i + 1) * 16) + x) = block(x)
                Next

            Next
            '#######main encryption loop
        Else
            For i As Integer = 0 To (outputdata.Length / 16) - 1
                If i > 0 Then
                    progress = (i / (inputdata.Length / 16)) * 100
                End If
                '#######main decryption loop
                Dim tempblock(15) As Byte
                Dim block(15) As Byte
                '#####take 1 block data for decryption
                For x As Integer = 0 To 15
                    block(x) = inputdata(((i + 1) * 16) + x)
                    'tempblock(x) = block(x)
                Next
                'apply current block for prev block
                '            tempblock = block

                'decrypt block
                block = Slash_crypt.Decrypt_Block(block)
                '##### xor output with prev block
                'For x As Integer = 0 To 7
                '    block(x) = block(x) Xor Slash_crypt.prev_block(x)
                'Next
                'Write outputblock to output data
                'Slash_crypt.prev_block = tempblock
                For x As Integer = 0 To 15
                    outputdata((i * 16) + x) = block(x)
                Next

            Next
            '#######main decryption loop
        End If
        progress = 100
        If encrypt = True Then
            RichTextBox2.Invoke(Sub() RichTextBox2.Text = Convert.ToBase64String(outputdata))
            ' RichTextBox2.Text = Convert.ToBase64String(outputdata)
        Else
            'RichTextBox2.Text = Encoding.UTF8.GetString(outputdata)
            RichTextBox2.Invoke(Sub() RichTextBox2.Text = Encoding.UTF8.GetString(outputdata))
        End If
        sw.Stop()
        If CheckBox1.Checked = True Then
            Return_GUI("Time taken: " & sw.ElapsedMilliseconds & "ms")
        Else
            Return_GUI("")
        End If
    End Sub
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'CheckForIllegalCrossThreadCalls = False
        Dim en As New Threading.Thread(AddressOf milliTick)
        en.Priority = Threading.ThreadPriority.Highest
        en.Start()
        Dim ent As New Threading.Thread(AddressOf Entropy_generator)
        ent.Priority = Threading.ThreadPriority.Highest
        ent.Start()
    End Sub
    Sub Entropy_generator()
        While True
            Dim tick_String As String = Ticks.ToString
            Dim entropy_string_byte As Byte() = Encoding.UTF8.GetBytes(Mouse_cords & tick_String & (DateTime.Now - New DateTime(1970, 1, 1)).TotalMilliseconds)
            Dim entropy_bytes(31 + entropy_string_byte.Length) As Byte
            entropy_pool.CopyTo(entropy_bytes, 0)
            entropy_string_byte.CopyTo(entropy_bytes, 32)
            Dim new_hash As New ARX_hash_function
            new_hash.Generate_New_SLASH_HASH(entropy_bytes)
            entropy_pool = new_hash.hash
            Thread.Sleep(1)

        End While
    End Sub
    Sub milliTick()
        While True
            Ticks += 1
            Thread.Sleep(1)
        End While

    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Label1.Text = "Input: " & RichTextBox1.TextLength
        Label2.Text = "Output: " & RichTextBox2.TextLength
        Try
            ProgressBar1.Value = progress
        Catch ex As Exception

        End Try

        If TextBox1.TextLength > 0 And RichTextBox1.TextLength > 0 And working = False Then
            Button1.Enabled = True
        Else
            Button1.Enabled = False
        End If
    End Sub
    Function Lock_GUI()
        RichTextBox1.Invoke(Sub() RichTextBox1.Enabled = False)
        Button1.Invoke(Sub() Button1.Enabled = False)
        Button2.Invoke(Sub() Button2.Enabled = False)
        Button6.Invoke(Sub() Button6.Enabled = False)
        TextBox1.Invoke(Sub() TextBox1.Enabled = False)
        CheckBox1.Invoke(Sub() CheckBox1.Enabled = False)
        RadioButton1.Invoke(Sub() RadioButton1.Enabled = False)
        RadioButton2.Invoke(Sub() RadioButton2.Enabled = False)
        working = True
        Return True
    End Function
    Function Return_GUI(message As String)
        RichTextBox1.Invoke(Sub() RichTextBox1.Enabled = True)
        Button1.Invoke(Sub() Button1.Enabled = True)
        Button2.Invoke(Sub() Button2.Enabled = True)
        Button6.Invoke(Sub() Button6.Enabled = True)
        TextBox1.Invoke(Sub() TextBox1.Enabled = True)
        CheckBox1.Invoke(Sub() CheckBox1.Enabled = True)
        RadioButton1.Invoke(Sub() RadioButton1.Enabled = True)
        RadioButton2.Invoke(Sub() RadioButton2.Enabled = True)
        working = False
        If message = "" Or message = Nothing Then

        Else
            MsgBox(message)
        End If

        Return True
    End Function
    Private Sub Panel2_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Panel2.MouseDown
        TextBox1.UseSystemPasswordChar = False
    End Sub

    Private Sub Panel2_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Panel2.MouseUp
        TextBox1.UseSystemPasswordChar = True
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        Label7.Text = TextBox1.TextLength

    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing

    End Sub

    Private Sub CopyToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CopyToolStripMenuItem.Click
        Clipboard.SetDataObject(RichTextBox1.SelectedText, True)
    End Sub

    Private Sub PasteToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PasteToolStripMenuItem.Click
        RichTextBox1.Paste()
    End Sub
    Private Sub SelectAllToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SelectAllToolStripMenuItem.Click
        RichTextBox1.SelectAll()
    End Sub

    Private Sub CutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CutToolStripMenuItem.Click
        Dim tcut As String = RichTextBox1.SelectedText
        RichTextBox1.SelectedText = ""
        Clipboard.SetDataObject(tcut, True)

    End Sub

    Private Sub CopyToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles CopyToolStripMenuItem1.Click
        'RichTextBox2.Copy()
        Try
            Clipboard.SetDataObject(RichTextBox2.SelectedText, True)
        Catch ex As Exception

        End Try

    End Sub

    Private Sub SelectAndCopyAllToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SelectAndCopyAllToolStripMenuItem.Click
        RichTextBox2.SelectAll()
    End Sub


    Private Sub OpenFileDialog1_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles OpenFileDialog1.FileOk
        Dim strm As System.IO.Stream
        strm = OpenFileDialog1.OpenFile()
        s_file = OpenFileDialog1.FileName.ToString()
        If Not (strm Is Nothing) Then
            strm.Close()
        End If
    End Sub



    Private Sub SaveFileDialog1_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles SaveFileDialog1.FileOk
        s_file = SaveFileDialog1.FileName.ToString()
    End Sub

    Private Sub SaveOutputToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaveOutputToolStripMenuItem.Click
        If RichTextBox2.Text = "" Then
            MsgBox("No output text to save")
            Exit Sub
        End If

        s_file = ""
        SaveFileDialog1.Title = "Please Select a File"
        SaveFileDialog1.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
        SaveFileDialog1.FilterIndex = 1
        SaveFileDialog1.RestoreDirectory = True

        SaveFileDialog1.InitialDirectory = "C:temp"


        SaveFileDialog1.ShowDialog()
        Dim errorsave = "0"
        If s_file.Length > 0 Then
            Try
                My.Computer.FileSystem.WriteAllText(s_file, RichTextBox2.Text, False)
            Catch ex As System.IO.DirectoryNotFoundException
                errorsave = "1"
                ' Let the user know that the directory did not exist.
                MsgBox("Output not saved,Directory not found.", MsgBoxStyle.Critical, "Output not saved.")
            Catch ex As UnauthorizedAccessException
                errorsave = "1"
                MsgBox("Output not saved,Access is denied.", MsgBoxStyle.Critical, "Output not saved.")
            Catch ex As ArgumentNullException
                errorsave = "1"
                MsgBox("Output not saved,Please enter a file save path.", MsgBoxStyle.Critical, "Output not saved.")
            Catch ex As ArgumentException
                errorsave = "1"
                MsgBox("Output not saved,Invalid Path.", MsgBoxStyle.Critical, "Output not saved.")
            End Try

            If errorsave = "0" Then
                MsgBox("The Output has to been saved to: " & s_file, MsgBoxStyle.Information, "Output saved.")
            End If
        End If

        s_file = ""
    End Sub

    Private Sub OpenTextFileToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenTextFileToolStripMenuItem.Click
        s_file = ""
        OpenFileDialog1.Title = "Please Select a File"
        OpenFileDialog1.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
        OpenFileDialog1.FilterIndex = 1
        OpenFileDialog1.RestoreDirectory = True

        OpenFileDialog1.InitialDirectory = "C:temp"


        OpenFileDialog1.ShowDialog()
        If s_file.Length > 0 Then
            If System.IO.File.Exists(s_file) Then
                RichTextBox1.Text = My.Computer.FileSystem.ReadAllText(s_file)

            Else
                MsgBox("Please Ensure you have entered the correct file location", MsgBoxStyle.Exclamation)
            End If
        End If
        s_file = ""
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Start.Show()
        closeprogram = False
        Debug.Close()
        CheckBox1.Checked = False
        Me.Close()
    End Sub

    Private Sub CText_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If closeprogram = True Then
            Process.GetCurrentProcess().Kill()
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        RichTextBox1.Clear()
        RichTextBox2.Clear()
        CheckBox1.Checked = False
        TextBox1.Clear()
        RadioButton1.Checked = True
        s_file = ""
    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        If working = False And TextBox1.TextLength > 0 And RichTextBox2.TextLength > 0 Then
            clicks = (clicks + 1) Mod 26
            If clicks = 20 Then
                Button1.Enabled = False
            ElseIf clicks = 25 Then
                Button1.Enabled = True
                Timer2.Stop()
            End If
        End If
    End Sub

    Private Sub Button1_MouseClick(sender As Object, e As MouseEventArgs) Handles Button1.MouseClick

    End Sub

    Private Sub RichTextBox1_TextChanged(sender As Object, e As EventArgs) Handles RichTextBox1.TextChanged

    End Sub

    Private Sub RichTextBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles RichTextBox1.KeyDown
        If e.Control And (e.KeyCode = Keys.C Or e.KeyCode = Keys.X) Then
            e.Handled = True
        End If

        If e.Control And e.KeyCode = Keys.C Then
            Clipboard.SetDataObject(RichTextBox1.SelectedText, True)
        End If

        If e.Control And e.KeyCode = Keys.X Then
            Dim tcut As String = RichTextBox1.SelectedText
            RichTextBox1.SelectedText = ""
            Clipboard.SetDataObject(tcut, True)

        End If

    End Sub


    Private Sub RichTextBox2_KeyDown(sender As Object, e As KeyEventArgs) Handles RichTextBox2.KeyDown
        If e.Control And (e.KeyCode = Keys.C Or e.KeyCode = Keys.X) Then
            e.Handled = True
        End If

        If e.Control And e.KeyCode = Keys.C Then
            Clipboard.SetDataObject(RichTextBox2.SelectedText, True)
        End If

        If e.Control And e.KeyCode = Keys.X Then
            Dim tcut As String = RichTextBox2.SelectedText
            RichTextBox2.SelectedText = ""
            Clipboard.SetDataObject(tcut, True)
        End If
    End Sub

    Private Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyDown
        If e.Control And (e.KeyCode = Keys.C Or e.KeyCode = Keys.X) Then
            e.Handled = True
        End If

        If TextBox1.UseSystemPasswordChar = False Then
            If e.Control And e.KeyCode = Keys.C Then
                Clipboard.SetDataObject(TextBox1.SelectedText, True)


            ElseIf e.Control And e.KeyCode = Keys.X Then
                Dim tcut As String = TextBox1.SelectedText
                TextBox1.SelectedText = ""
                Clipboard.SetDataObject(tcut, True)
            End If
        End If

    End Sub

    Private Sub RichTextBox2_TextChanged(sender As Object, e As EventArgs) Handles RichTextBox2.TextChanged

    End Sub

    Private Sub CText_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        Mouse_cords = e.X & e.Y
    End Sub
End Class
