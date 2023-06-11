Imports System.IO.Ports
Imports Excel = Microsoft.Office.Interop.Excel '-> It is used to save data to Excel. 
'-> If you don't need it, delete this code and all the code in ButtonExportToExcel.

Public Class Form1
    Dim DataSerIn As String '-> Variable to hold incoming serial data

    Private stopwatch As New Diagnostics.Stopwatch '-> Stopwatch declaration used as a timer
    Dim _sec, _min As String '-> Variable to hold the minutes and seconds of the stopwatch

    Dim ThresholdStat As Boolean = True '-> Variable for triggers in calculating heartbeats
    Dim HB As Integer = 0 '-> Variable to hold the heart rate signal value from the serial
    Dim cntHB As Integer = 0 '-> Variable for counting the number of heartbeats
    Dim HeartRateBPMResult As Integer '-> Variable to display the result of heart rate calculation (BPM)

    Dim ShowHB As Integer '-> Variable for indicator to display the result of heart rate calculation (BPM)
    Dim SB As Boolean = True '-> Helper variable to display the result of heart rate calculation (BPM)

    Dim SR As Boolean = False '-> Variable trigger to start the calculation of the heart rate (BPM)

    Private Sub ButtonScanPort_Click(sender As Object, e As EventArgs) Handles ButtonScanPort.Click
        '----------------------------------------To read and display COM Port
        ComboBoxPort.Items.Clear()
        Dim myPort As Array
        Dim i As Integer
        myPort = IO.Ports.SerialPort.GetPortNames()
        ComboBoxPort.Items.AddRange(myPort)
        i = ComboBoxPort.Items.Count
        i = i - i
        Try
            ComboBoxPort.SelectedIndex = i
            ButtonConnection.Enabled = True
            ComboBoxPort.DroppedDown = True
        Catch ex As Exception
            MsgBox("Failed to scan Port. Ensure that Arduino is properly connected to the computer." & vbCrLf & "Error Message : " & ex.Message, MsgBoxStyle.Critical, "Error !!!")
            ComboBoxPort.Text = ""
            ComboBoxPort.Items.Clear()
            ButtonConnection.Enabled = False
            ButtonRecord.Enabled = False
            Return
        End Try
        '----------------------------------------
    End Sub

    Private Sub ButtonConnection_Click(sender As Object, e As EventArgs) Handles ButtonConnection.Click
        ChartHeartBeatRealTime.Series("Series1").Points.Clear()
        '----------------------------------------Condition and execution when the connection button or the disconnect button is pressed
        If ButtonConnection.Text = "Connect" Then
            Try
                SerialPortArduino.PortName = ComboBoxPort.Text
                SerialPortArduino.BaudRate = ComboBoxBaudRate.Text
                SerialPortArduino.Open()
                SerialPortArduino.DiscardOutBuffer()
                SerialPortArduino.DiscardInBuffer()
                SerialPortArduino.WriteLine(TextBoxThreshold.Text)
                TextBoxThreshold.Enabled = False

                For i = 0 To 101
                    ChartHeartBeatRealTime.Series("Series2").Points.AddY(TextBoxThreshold.Text)
                    If ChartHeartBeatRealTime.Series("Series2").Points.Count = 101 Then
                        ChartHeartBeatRealTime.Series("Series2").Points.RemoveAt(0)
                    End If
                Next

                TimerSerialPort.Start()
                ButtonConnection.Text = "Disconnect"
                ButtonRecord.Enabled = True
                LabelConnection.Text = "Status : Connected"
                ButtonScanPort.Enabled = False
                ButtonRecord.Enabled = True
                ComboBoxPort.Enabled = False
                ComboBoxBaudRate.Enabled = False
            Catch ex As Exception
                MsgBox("Failed to open Port. Ensure that Arduino is connected to the computer correctly and that you have selected the correct port." & vbCrLf & "Error Message : " & ex.Message, MsgBoxStyle.Critical, "Error !!!")
            End Try
        Else
            Try
                TimerSerialPort.Stop()
                Threading.Thread.Sleep(500)
                SerialPortArduino.Close()
                Threading.Thread.Sleep(500)
                ButtonConnection.Text = "Connect"
                ButtonRecord.Enabled = False
                LabelConnection.Text = "Status : Disconnected"
                ButtonScanPort.Enabled = True
                ButtonRecord.Enabled = False
                ComboBoxPort.Enabled = True
                ComboBoxBaudRate.Enabled = True
                TextBoxThreshold.Enabled = True

                For i = 0 To 101
                    ChartHeartBeatRealTime.Series("Series1").Points.AddY(0)
                    If ChartHeartBeatRealTime.Series("Series1").Points.Count = 101 Then
                        ChartHeartBeatRealTime.Series("Series1").Points.RemoveAt(0)
                    End If
                Next
            Catch ex As Exception

            End Try
        End If
        '----------------------------------------
    End Sub

    Private Sub ButtonRecord_Click(sender As Object, e As EventArgs) Handles ButtonRecord.Click
        '----------------------------------------Condition and execution when the button starts recording or the button stops recording is clicked
        If ButtonRecord.Text = "Start Recording" Then
            ButtonRecord.Text = "Stop Recording"
            DataGridViewHRBPM.Rows.Clear()
            ChartHeartRateBPM.Series("Series1").Points.Clear()
            ButtonConnection.Enabled = False
            ButtonExportToExcel.Enabled = False
            ButtonClearRecording.Enabled = False
            cntHB = 0
            SR = True
            stopwatch.Start()
        Else
            ButtonRecord.Text = "Start Recording"
            ButtonConnection.Enabled = True
            ButtonExportToExcel.Enabled = True
            ButtonClearRecording.Enabled = True
            SR = False
            stopwatch.Reset()
            stopwatch.Stop()
            cntHB = 0
            LabelTime.Text = "Timer 00: 00"
        End If
        '----------------------------------------
    End Sub

    Private Sub ButtonExportToExcel_Click(sender As Object, e As EventArgs) Handles ButtonExportToExcel.Click
        Try
            If DataGridViewHRBPM.Rows.Count > 0 Then
                Dim filename As String = ""
                Dim SV As SaveFileDialog = New SaveFileDialog()
                SV.Filter = "EXCEL FILES|*.xlsx;*.xls"
                Dim result As DialogResult = SV.ShowDialog()

                If result = DialogResult.OK Then
                    Me.Text = "Monitoring Heart Rate (Saving to Excel. Please wait...)"
                    ProgressBarSave.Visible = True
                    ProgressBarSave.Value = 2
                    filename = SV.FileName
                    Dim multiselect As Boolean = DataGridViewHRBPM.MultiSelect
                    DataGridViewHRBPM.MultiSelect = True
                    DataGridViewHRBPM.SelectAll()
                    DataGridViewHRBPM.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText
                    Clipboard.SetDataObject(DataGridViewHRBPM.GetClipboardContent())
                    Dim results = System.Convert.ToString(Clipboard.GetData(DataFormats.Text))
                    DataGridViewHRBPM.ClearSelection()
                    DataGridViewHRBPM.MultiSelect = multiselect
                    Dim XCELAPP As Microsoft.Office.Interop.Excel.Application = Nothing
                    Dim XWORKBOOK As Microsoft.Office.Interop.Excel.Workbook = Nothing
                    Dim XSHEET As Microsoft.Office.Interop.Excel.Worksheet = Nothing
                    Dim misValue As Object = System.Reflection.Missing.Value
                    ProgressBarSave.Value = 4
                    XCELAPP = New Excel.Application()
                    XWORKBOOK = XCELAPP.Workbooks.Add(misValue)
                    XCELAPP.DisplayAlerts = False
                    XCELAPP.Visible = False
                    XSHEET = XWORKBOOK.ActiveSheet
                    ProgressBarSave.Value = 6
                    XSHEET.Paste()
                    XWORKBOOK.SaveAs(filename, Excel.XlFileFormat.xlOpenXMLWorkbook)
                    XWORKBOOK.Close(False)
                    XCELAPP.Quit()
                    ProgressBarSave.Value = 8
                    Try
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(XSHEET)
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(XWORKBOOK)
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(XCELAPP)
                    Catch
                    End Try
                    Me.Text = "Monitoring Heart Rate"
                    ProgressBarSave.Value = 10
                    ProgressBarSave.Visible = False
                    MessageBox.Show("Save Succesfully")
                End If
            End If
        Catch ex As Exception
            Me.Text = "Data Log"
            MessageBox.Show(ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ButtonClearRecording_Click(sender As Object, e As EventArgs) Handles ButtonClearRecording.Click
        DataGridViewHRBPM.Rows.Clear()
        ChartHeartRateBPM.Series("Series1").Points.Clear()
        For i = 0 To 13
            ChartHeartRateBPM.Series("Series1").Points.AddXY("00:00", 0)
        Next
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        '----------------------------------------To display a temporary graph when the application is first run
        For i = 0 To 101
            ChartHeartBeatRealTime.Series("Series1").Points.AddY(0)
            If ChartHeartBeatRealTime.Series("Series1").Points.Count = 101 Then
                ChartHeartBeatRealTime.Series("Series1").Points.RemoveAt(0)
            End If
        Next

        For i = 0 To 13
            ChartHeartRateBPM.Series("Series1").Points.AddXY("00:00", 0)
        Next
        '----------------------------------------

        DoubleBuffer.DoubleBuffered(DataGridViewHRBPM, True) '-> To speed up DataGridView performance
        ComboBoxBaudRate.Text = "115200"

    End Sub

    Private Sub TextBoxThreshold_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBoxThreshold.KeyPress
        '----------------------------------------code to only allow numeric input on the mobile phone textbox
        If Not ((e.KeyChar >= "0" And e.KeyChar <= "9") Or e.KeyChar = vbBack Or e.KeyChar = "+") Then
            MessageBox.Show("Invalid Input! Numbers Only.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
            e.Handled = True
        End If
        '----------------------------------------
    End Sub

    Private Sub TimerSerialPort_Tick(sender As Object, e As EventArgs) Handles TimerSerialPort.Tick
        Processing_and_executing_incoming_serial_data()
    End Sub

    Private Sub SerialPortArduino_DataReceived(sender As Object, e As SerialDataReceivedEventArgs) Handles SerialPortArduino.DataReceived
        Try
            DataSerIn = SerialPortArduino.ReadLine '-> Receive and store data from serial
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    '----------------------------------------Subroutines for processing and displaying heart beat / heart rate data from serial
    Sub Processing_and_executing_incoming_serial_data()
        If ChartHeartBeatRealTime.Series("Series1").Points.Count = 101 Then
            ChartHeartBeatRealTime.Series("Series1").Points.RemoveAt(0)
        End If
        ChartHeartBeatRealTime.Series("Series1").Points.AddXY(0, DataSerIn)
        ChartHeartBeatRealTime.Series("Series1").LegendText = "Real Time HeartBeat Signal (" & DataSerIn & ")"

        If SR = True Then
            HB = DataSerIn
            If HB > TextBoxThreshold.Text And ThresholdStat = True Then
                cntHB += 1
                ThresholdStat = False
            End If

            If HB < TextBoxThreshold.Text Then
                ThresholdStat = True
            End If

            Dim elapsed As TimeSpan = stopwatch.Elapsed
            _sec = elapsed.Seconds.ToString("00")
            _min = elapsed.Minutes.ToString("00")
            LabelTime.Text = "Timer " & _min & ":" & _sec

            ShowHB = _sec Mod 10

            If ShowHB = 0 And SB = True Then
                HeartRateBPMResult = cntHB * 6
                DataGridViewHRBPM.Rows.Add(_min & ":" & _sec, HeartRateBPMResult)
                DataGridViewHRBPM.FirstDisplayedScrollingRowIndex = DataGridViewHRBPM.RowCount - 1
                DataGridViewHRBPM.ClearSelection()
                DataGridViewHRBPM.Rows(DataGridViewHRBPM.RowCount - 1).Selected = True

                If ChartHeartRateBPM.Series("Series1").Points.Count = 13 Then
                    ChartHeartRateBPM.Series("Series1").Points.RemoveAt(0)
                End If
                ChartHeartRateBPM.Series("Series1").Points.AddXY(_min & ":" & _sec, HeartRateBPMResult)

                cntHB = 0
                SB = False
            End If

            If ShowHB <> 0 Then
                SB = True
            End If
        End If
    End Sub
    '----------------------------------------
End Class
