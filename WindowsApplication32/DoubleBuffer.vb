﻿'========================================================Module to speed up DataGridView performance
Imports System
Imports System.Reflection
Imports System.Windows.Forms

Module DoubleBuffer
    Public Sub DoubleBuffered(ByVal dgv As DataGridView, ByVal setting As Boolean)
        Dim dgvType As Type = dgv.[GetType]()
        Dim pi As PropertyInfo = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance Or BindingFlags.NonPublic)
        pi.SetValue(dgv, setting, Nothing)
    End Sub
End Module
'========================================================