Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading
Imports System.IO
Public Class CFile
    Dim progress As Long = 0
    Dim working As Boolean = False
    Dim blocks As Long = 0
    Dim pblocks As Long = 0
    Dim status As String = "Status: None"
    Dim closeprogram = True
    Dim Ticks As UInt64 = 0
    Dim entropy_pool(31) As Byte
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        OpenFileDialog1.Title = "Please Select a File"
        OpenFileDialog1.Filter = "All files|*.*|Encrypted files (*.slsh)|*.slsh"
        If RadioButton1.Checked = True Then
            OpenFileDialog1.FilterIndex = 1
        Else
            OpenFileDialog1.FilterIndex = 2
        End If
        OpenFileDialog1.RestoreDirectory = True

        OpenFileDialog1.InitialDirectory = "C:temp"


        OpenFileDialog1.ShowDialog()
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        If CheckBox2.Checked = True Then
            If Mid(TextBox2.Text, TextBox2.TextLength - 4, 5).ToLower = ".slsh" Then
                RadioButton2.Checked = True
                TextBox8.Text = Mid(TextBox2.Text, 1, TextBox2.TextLength - 5)
                SaveFileDialog1.FileName = Mid(OpenFileDialog1.FileName, 1, OpenFileDialog1.FileName.Length - 5)
                SaveFileDialog1.FilterIndex = 1
            Else
                TextBox8.Text = TextBox2.Text & ".slsh"
                SaveFileDialog1.FileName = OpenFileDialog1.FileName & ".slsh"
                SaveFileDialog1.FilterIndex = 2
                RadioButton1.Checked = True
            End If
        End If

        SaveFileDialog1.Title = "Please Select a File"
        SaveFileDialog1.Filter = "All files|*.*|Encrypted files (*.slsh)|*.slsh"
        If RadioButton1.Checked = True Then
            SaveFileDialog1.FilterIndex = 2
        Else
            SaveFileDialog1.FilterIndex = 1
        End If

        SaveFileDialog1.RestoreDirectory = True

        SaveFileDialog1.InitialDirectory = "C:temp"


        SaveFileDialog1.ShowDialog()
    End Sub

    Private Sub OpenFileDialog1_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles OpenFileDialog1.FileOk
        Dim strm As System.IO.Stream
        strm = OpenFileDialog1.OpenFile()
        TextBox2.Text = OpenFileDialog1.FileName.ToString()
        If Not (strm Is Nothing) Then
            strm.Close()

        End If
        Try
            If CheckBox2.Checked = True Then
                If Mid(TextBox2.Text, TextBox2.TextLength - 4, 5).ToLower = ".slsh" Then
                    RadioButton2.Checked = True
                    TextBox8.Text = Mid(TextBox2.Text, 1, TextBox2.TextLength - 5)
                    SaveFileDialog1.FileName = Mid(OpenFileDialog1.FileName, 1, OpenFileDialog1.FileName.Length - 5)
                    SaveFileDialog1.FilterIndex = 1
                Else
                    TextBox8.Text = TextBox2.Text & ".slsh"
                    SaveFileDialog1.FileName = OpenFileDialog1.FileName & ".slsh"
                    SaveFileDialog1.FilterIndex = 2
                    RadioButton1.Checked = True
                End If
            End If
        Catch ex As Exception

        End Try


    End Sub

    Private Sub SaveFileDialog1_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles SaveFileDialog1.FileOk
        TextBox8.Text = SaveFileDialog1.FileName.ToString()
    End Sub

    Private Sub CFile_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' CheckForIllegalCrossThreadCalls = False
        Dim en As New Threading.Thread(AddressOf milliTick)
        en.Priority = Threading.ThreadPriority.Highest
        en.Start()
        Dim ent As New Threading.Thread(AddressOf Entropy_generator)
        ent.Priority = Threading.ThreadPriority.Highest
        ' ent.Start()
    End Sub
    Sub Entropy_generator()
        While True
            Dim tick_String As String = Ticks.ToString
            Dim entropy_string_byte As Byte() = Encoding.UTF8.GetBytes(MousePosition.ToString & tick_String & (DateTime.Now - New DateTime(1970, 1, 1)).TotalMilliseconds)
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
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If TextBox3.TextLength < 1 Or TextBox2.TextLength < 1 Or TextBox8.TextLength < 1 Then
            MsgBox("Please fill in all required forms.")
            GoTo t
        End If
        If My.Computer.FileSystem.FileExists(TextBox2.Text) Then
        Else
            MsgBox("Input file not found.")
            GoTo t
        End If

        Dim en As New Threading.Thread(AddressOf crypt)
        en.Priority = Threading.ThreadPriority.Highest
        en.Start()
        ProgressBar1.Value = 0
        GroupBox3.Enabled = False

t:
    End Sub
    Sub crypt()

        Dim Slash_crypt As New SLASH_128_BIT_ENCRYPTON
        Dim encrypt As Boolean = True
        Dim inputfile As String = ""
        Dim outputfile As String = ""
        Dim password As String = ""
        TextBox2.Invoke(Sub() inputfile = TextBox2.Text)
        TextBox8.Invoke(Sub() outputfile = TextBox8.Text)
        TextBox3.Invoke(Sub() password = TextBox3.Text)
        Dim inputfilesize As Long = getsize(inputfile)
        Dim lastblocksize As Long = inputfilesize Mod 16
        Lock_GUI()
        Dim blocknumber As Long = contoint(inputfilesize / 16)
        Dim inputdata As System.IO.FileStream
        Dim outputdata As System.IO.FileStream
        Dim inputblock(15) As Byte
        Dim outputblock(15) As Byte
        progress = 0
        Dim count As Integer = 0
        Dim entropy As Byte() = entropy_pool
        Dim sw As Stopwatch = Stopwatch.StartNew
        Try
            inputdata = New FileStream(inputfile, FileMode.Open)
            outputdata = New FileStream(outputfile, FileMode.Create, FileAccess.Write)
            If RadioButton1.Checked = False Then
                encrypt = False
            End If

            If encrypt = False Then
                If lastblocksize > 0 Then
                    inputdata.Close()
                    outputdata.Close()
                    Return_GUI("No encrypted data.")
                    Exit Sub
                End If
            End If

            'Dim iv As Byte() = {82, 134, 254, 90, 37, 112, 140, 192}


            If encrypt = True Then
                'initiate encryption
                status = "Status: Encrypting Data"
                If lastblocksize > 0 Then
                    Slash_crypt.lastblocksize = lastblocksize
                End If
                Slash_crypt.Initiate_Encryption(Encoding.UTF8.GetBytes(password), inputfilesize, entropy)
            Else
                status = "Status: Decrypting Data"
                'initiate decryption
                inputdata.Read(Slash_crypt.prev_block, 0, 16)
                Try
                    Slash_crypt.Initiate_Decryption(Encoding.UTF8.GetBytes(password))
                Catch ex As Exception
                    inputdata.Close()
                    outputdata.Close()
                    Return_GUI("Invalid Decryption Key.")
                    Exit Sub
                End Try


                lastblocksize = Slash_crypt.lastblocksize
            End If

            '######cryption

            If encrypt = True Then
                'write IV before writing encrypted data
                outputdata.Write(Slash_crypt.prev_block, 0, 16)
                '#######main encryption loop
                For i As Long = 0 To blocknumber - 1
                    blocks = i
                    If i > 0 Then
                        progress = (i / blocknumber) * 100
                    End If
                    Dim block(15) As Byte
                    '#####take 1 block data for encryption, xor prev block
                    If i = blocknumber - 1 And lastblocksize > 0 Then
                        inputdata.Read(inputblock, 0, lastblocksize)
                    Else
                        inputdata.Read(inputblock, 0, 16)
                    End If

                    'encrypt block
                    outputblock = Slash_crypt.Encrypt_Block(inputblock)
                    'write outputblock to output data
                    outputdata.Write(outputblock, 0, 16)

                Next
                '#######main encryption loop
            Else

                For i As Long = 0 To blocknumber - 2
                    blocks = i
                    If i > 0 Then
                        progress = (i / blocknumber) * 100
                    End If
                    '#######main decryption loop
                    Dim tempblock(15) As Byte
                    Dim block(15) As Byte
                    '#####take 1 block data for decryption
                    inputdata.Read(inputblock, 0, 16)
                    'decrypt block
                    outputblock = Slash_crypt.Decrypt_Block(inputblock)
                    '##### xor output with prev block
                    'Write outputblock to output data

                    If i = blocknumber - 2 And lastblocksize > 0 Then
                        outputdata.Write(outputblock, 0, lastblocksize)
                    Else
                        outputdata.Write(outputblock, 0, 16)
                    End If


                Next
                '#######main decryption loop
            End If
            progress = 100
            inputdata.Close()
            outputdata.Close()
            sw.Stop()

        Catch ex As Exception
            MsgBox(ex.Message, , "Error")
            MsgBox(count)
            inputdata.Close()
            outputdata.Close()
            Return_GUI("")
            Exit Sub
        End Try
        If CheckBox1.Checked = True Then
            Try
                My.Computer.FileSystem.DeleteFile(inputfile)

            Catch ex As Exception
                MsgBox("Cannot delete input file.", MsgBoxStyle.Exclamation, "Error")
            End Try
        End If
        Return_GUI("The Output has to been saved to: " & outputfile)
        'If CheckBox1.Checked = True Then
        '    Return_GUI("Time taken: " & sw.ElapsedMilliseconds & "ms")
        'Else
        'Return_GUI("")
        'End If
        Exit Sub
    End Sub
    Function Lock_GUI()
        GroupBox3.Invoke(Sub() GroupBox3.Enabled = False)
        working = True
        Return True
    End Function
    Function Return_GUI(message As String)

        If message = "" Or message = Nothing Then
            working = False
        Else
            MsgBox(message)
            working = False
        End If
        While working = True
            Thread.Sleep(1)
        End While
        blocks = 0
        status = "Status: None"
        GroupBox3.Invoke(Sub() GroupBox3.Enabled = True)
        'Button1.Invoke(Sub() Button1.Focus())
        Return True
    End Function
    Function getsize(ByRef filename)
        Dim info As New FileInfo(filename)
        Dim fsize As Long = info.Length
        Return fsize
    End Function

    Private Sub TextBox3_TextChanged(sender As Object, e As EventArgs) Handles TextBox3.TextChanged
        If TextBox3.TextLength < 8 Then
            Label8.Text = "Password is Not secure."
            Label8.ForeColor = Color.Red
        End If
        If TextBox3.TextLength <= 11 And TextBox3.TextLength > 7 Then
            Label8.Text = "Password security is poor."
            Label8.ForeColor = Color.Orange
        End If
        If TextBox3.TextLength <= 15 And TextBox3.TextLength >= 12 Then
            Label8.Text = "Password security is OK."
            Label8.ForeColor = Color.YellowGreen
        End If
        If TextBox3.TextLength > 15 Then
            Label8.Text = "Password security is Great."
            Label8.ForeColor = Color.ForestGreen
        End If
        Label7.Text = TextBox3.TextLength
        If TextBox3.TextLength > 0 Then
            Button1.Enabled = True
        Else
            Button1.Enabled = False
        End If
    End Sub

    Private Sub Panel2_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Panel2.MouseDown
        TextBox3.UseSystemPasswordChar = False
    End Sub

    Private Sub Panel2_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Panel2.MouseUp
        TextBox3.UseSystemPasswordChar = True
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Label3.Text = status
        ProgressBar1.Value = progress
        Label2.Text = ProgressBar1.Value & "%"
    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        Dim data_speed = blocks - pblocks
        Try
            Label4.Text = "Speed: " & ((data_speed * 16) / 1000000) & " MB/s"
        Catch ex As Exception
        End Try
        pblocks = blocks
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Start.Show()
        closeprogram = False
        CheckBox1.Checked = False
        CheckBox2.Checked = True
        Me.Close()
    End Sub

    Private Sub CFile_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If closeprogram = True Then
            Process.GetCurrentProcess().Kill()
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        TextBox3.Text = ""
        TextBox2.Text = ""
        TextBox8.Text = ""
        CheckBox1.Checked = False
        CheckBox2.Checked = True
        RadioButton1.Checked = True
        ProgressBar1.Value = 0
    End Sub

    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles TextBox2.TextChanged

    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged
        Try
            If CheckBox2.Checked = True Then
                Dim ext = Mid(TextBox2.Text, TextBox2.TextLength - 4, 5)
                If Mid(TextBox2.Text, TextBox2.TextLength - 4, 5).ToLower = ".slsh" Then
                    RadioButton2.Checked = True
                    TextBox8.Text = Mid(TextBox2.Text, 1, TextBox2.TextLength - 5)
                Else
                    TextBox8.Text = TextBox2.Text & ".slsh"
                    RadioButton1.Checked = True
                End If
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub TextBox3_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox3.KeyDown
        If e.Control And (e.KeyCode = Keys.C Or e.KeyCode = Keys.X) Then
            e.Handled = True
        End If

        If TextBox3.UseSystemPasswordChar = False Then

            If e.Control And e.KeyCode = Keys.C Then
                Clipboard.SetDataObject(TextBox3.SelectedText, True)


            ElseIf e.Control And e.KeyCode = Keys.X Then
                Dim tcut As String = TextBox3.SelectedText
                TextBox3.SelectedText = ""
                Clipboard.SetDataObject(tcut, True)
            End If

        End If

        If e.KeyCode = Keys.Enter Then
            If TextBox3.TextLength < 1 Or TextBox2.TextLength < 1 Or TextBox8.TextLength < 1 Then
                MsgBox("Please fill in all required forms.")
                GoTo t
            End If
            If My.Computer.FileSystem.FileExists(TextBox2.Text) Then
            Else
                MsgBox("Input file not found.")
                GoTo t
            End If

            Dim en As New Threading.Thread(AddressOf crypt)
            en.Priority = Threading.ThreadPriority.Highest
            en.Start()
            ProgressBar1.Value = 0
            GroupBox3.Enabled = False

t:
        End If
    End Sub

    Private Sub TextBox2_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox2.KeyDown
        If e.Control And (e.KeyCode = Keys.C Or e.KeyCode = Keys.X) Then
            e.Handled = True
        End If



        If e.Control And e.KeyCode = Keys.C Then
            Clipboard.SetDataObject(TextBox2.SelectedText, True)


        ElseIf e.Control And e.KeyCode = Keys.X Then
            Dim tcut As String = TextBox2.SelectedText
            TextBox2.SelectedText = ""
            Clipboard.SetDataObject(tcut, True)
            End If


    End Sub


    Private Sub TextBox8_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox8.KeyDown
        If e.Control And (e.KeyCode = Keys.C Or e.KeyCode = Keys.X) Then
            e.Handled = True
        End If

        If TextBox3.UseSystemPasswordChar = False Then

            If e.Control And e.KeyCode = Keys.C Then
                Clipboard.SetDataObject(TextBox8.SelectedText, True)


            ElseIf e.Control And e.KeyCode = Keys.X Then
                Dim tcut As String = TextBox8.SelectedText
                TextBox8.SelectedText = ""
                Clipboard.SetDataObject(tcut, True)
            End If

        End If
    End Sub


    Private Sub Timer3_Tick(sender As Object, e As EventArgs) Handles Timer3.Tick
        entropy_pool = Start.entropy_pool

        'Label5.Text = Ticks.ToString & MousePosition.ToString
    End Sub
End Class