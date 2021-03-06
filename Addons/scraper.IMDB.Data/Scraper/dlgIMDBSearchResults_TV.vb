﻿' ################################################################################
' #                             EMBER MEDIA MANAGER                              #
' ################################################################################
' ################################################################################
' # This file is part of Ember Media Manager.                                    #
' #                                                                              #
' # Ember Media Manager is free software: you can redistribute it and/or modify  #
' # it under the terms of the GNU General Public License as published by         #
' # the Free Software Foundation, either version 3 of the License, or            #
' # (at your option) any later version.                                          #
' #                                                                              #
' # Ember Media Manager is distributed in the hope that it will be useful,       #
' # but WITHOUT ANY WARRANTY; without even the implied warranty of               #
' # MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the                #
' # GNU General Public License for more details.                                 #
' #                                                                              #
' # You should have received a copy of the GNU General Public License            #
' # along with Ember Media Manager.  If not, see <http://www.gnu.org/licenses/>. #
' ################################################################################

Imports System.Text.RegularExpressions
Imports System.IO
Imports EmberAPI
Imports NLog

Public Class dlgIMDBSearchResults_TV

#Region "Fields"
    Shared logger As Logger = NLog.LogManager.GetCurrentClassLogger()
    Friend WithEvents bwDownloadPic As New System.ComponentModel.BackgroundWorker
    Friend WithEvents tmrLoad As New System.Windows.Forms.Timer
    Friend WithEvents tmrWait As New System.Windows.Forms.Timer

    Private _IMDB As IMDB.Scraper
    Private sHTTP As New HTTP
    Private _currnode As Integer = -1
    Private _prevnode As Integer = -2
    Private _SpecialSettings As IMDB_Data.SpecialSettings

    Private _InfoCache As New Dictionary(Of String, MediaContainers.TVShow)
    Private _PosterCache As New Dictionary(Of String, System.Drawing.Image)
    Private _filterOptions As Structures.ScrapeOptions

    Private _nShow As MediaContainers.TVShow

#End Region 'Fields

#Region "Methods"

    Public Sub New(ByVal SpecialSettings As IMDB_Data.SpecialSettings, ByRef IMDB As IMDB.Scraper)
        ' This call is required by the designer.
        InitializeComponent()
        Me.Left = Master.AppPos.Left + (Master.AppPos.Width - Me.Width) \ 2
        Me.Top = Master.AppPos.Top + (Master.AppPos.Height - Me.Height) \ 2
        Me.StartPosition = FormStartPosition.Manual
        _SpecialSettings = SpecialSettings
        _IMDB = IMDB
    End Sub

    Public Overloads Function ShowDialog(ByRef nShow As MediaContainers.TVShow, ByVal sShowTitle As String, ByVal sShowPath As String, ByVal filterOptions As Structures.ScrapeOptions) As Windows.Forms.DialogResult
        Me.tmrWait.Enabled = False
        Me.tmrWait.Interval = 250
        Me.tmrLoad.Enabled = False

        Me.tmrLoad.Interval = 100

        _filterOptions = filterOptions
        _nShow = nShow

        Me.Text = String.Concat(Master.eLang.GetString(794, "Search Results"), " - ", sShowTitle)
        Me.txtSearch.Text = sShowTitle
        Me.txtFileName.Text = sShowPath

        ' fix for Enhancement #91
        'chkManual.Enabled = False
        chkManual.Enabled = True

        _IMDB.SearchTVShowAsync(sShowTitle, _filterOptions)

        Return MyBase.ShowDialog()
    End Function

    Public Overloads Function ShowDialog(ByRef nShow As MediaContainers.TVShow, ByVal Res As IMDB.SearchResults_TVShow, ByVal sShowTitle As String, ByVal sShowPath As String) As Windows.Forms.DialogResult
        Me.tmrWait.Enabled = False
        Me.tmrWait.Interval = 250
        Me.tmrLoad.Enabled = False
        Me.tmrLoad.Interval = 100

        _nShow = nShow

        Me.Text = String.Concat(Master.eLang.GetString(794, "Search Results"), " - ", sShowTitle)
        Me.txtSearch.Text = sShowTitle
        Me.txtFileName.Text = sShowPath
        SearchResultsDownloaded(Res)

        Return MyBase.ShowDialog()
    End Function

    Private Sub btnSearch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSearch.Click
        If Not String.IsNullOrEmpty(Me.txtSearch.Text) Then
            Me.OK_Button.Enabled = False
            pnlPicStatus.Visible = False
            _InfoCache.Clear()
            _PosterCache.Clear()
            Me.ClearInfo()
            Me.Label3.Text = Master.eLang.GetString(798, "Searching IMDB...")
            Me.pnlLoading.Visible = True

            chkManual.Enabled = False

            _IMDB.CancelAsync()
            _IMDB.SearchTVShowAsync(Me.txtSearch.Text, _filterOptions)
        End If
    End Sub

    Private Sub btnVerify_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnVerify.Click
        Dim pOpt As New Structures.ScrapeOptions
        pOpt = SetPreviewOptions()
        If Regex.IsMatch(Me.txtIMDBID.Text.Replace("tt", String.Empty), "\d\d\d\d\d\d\d") Then
            Me.pnlLoading.Visible = True
            _IMDB.CancelAsync()
            _IMDB.GetSearchTVShowInfoAsync(Me.txtIMDBID.Text.Replace("tt", String.Empty), _nShow, pOpt)
        Else
            MessageBox.Show(Master.eLang.GetString(799, "The ID you entered is not a valid IMDB ID."), Master.eLang.GetString(292, "Invalid Entry"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End If
    End Sub

    Private Sub bwDownloadPic_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwDownloadPic.DoWork
        Dim Args As Arguments = DirectCast(e.Argument, Arguments)

        sHTTP.StartDownloadImage(Args.pURL)

        While sHTTP.IsDownloading
            Application.DoEvents()
            Threading.Thread.Sleep(50)
        End While

        e.Result = New Results With {.Result = sHTTP.Image, .IMDBId = Args.IMDBId}
    End Sub

    Private Sub bwDownloadPic_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwDownloadPic.RunWorkerCompleted
        '//
        ' Thread finished: display pic if it was able to get one
        '\\

        Dim Res As Results = DirectCast(e.Result, Results)

        Try
            Me.pbPoster.Image = Res.Result
            If Not _PosterCache.ContainsKey(Res.IMDBId) Then
                _PosterCache.Add(Res.IMDBId, CType(Res.Result.Clone, Image))
            End If
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        Finally
            pnlPicStatus.Visible = False
        End Try
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        If _IMDB.bwIMDB.IsBusy Then
            _IMDB.CancelAsync()
        End If

        _nShow = New MediaContainers.TVShow

        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub btnOpenFolder_Click(sender As Object, e As EventArgs) Handles btnOpenFolder.Click
        Dim fPath As String = Directory.GetParent(Me.txtFileName.Text).FullName

        If Not String.IsNullOrEmpty(fPath) Then
            Process.Start("Explorer.exe", fPath)
        End If
    End Sub

    Private Sub chkManual_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkManual.CheckedChanged
        Me.ClearInfo()
        If Me.chkManual.Enabled Then
            Me.pnlLoading.Visible = False
            _IMDB.CancelAsync()
        End If
        Me.OK_Button.Enabled = False
        Me.txtIMDBID.Enabled = Me.chkManual.Checked
        Me.btnVerify.Enabled = Me.chkManual.Checked
        Me.tvResults.Enabled = Not Me.chkManual.Checked

        If Not Me.chkManual.Checked Then
            txtIMDBID.Text = String.Empty
        End If
    End Sub

    Private Sub ClearInfo()
        Me.ControlsVisible(False)
        Me.lblTitle.Text = String.Empty
        Me.lblTagline.Text = String.Empty
        Me.lblYear.Text = String.Empty
        Me.lblDirectors.Text = String.Empty
        Me.lblGenre.Text = String.Empty
        Me.txtPlot.Text = String.Empty
        Me.lblIMDBID.Text = String.Empty
        Me.pbPoster.Image = Nothing

        _nShow.Clear()

        _IMDB.CancelAsync()
    End Sub

    Private Sub ControlsVisible(ByVal areVisible As Boolean)
        Me.lblPremieredHeader.Visible = areVisible
        Me.lblDirectorsHeader.Visible = areVisible
        Me.lblGenreHeader.Visible = areVisible
        Me.lblPlotHeader.Visible = areVisible
        Me.lblIMDBHeader.Visible = areVisible
        Me.txtPlot.Visible = areVisible
        Me.lblYear.Visible = areVisible
        Me.lblTagline.Visible = areVisible
        Me.lblTitle.Visible = areVisible
        Me.lblDirectors.Visible = areVisible
        Me.lblGenre.Visible = areVisible
        Me.lblIMDBID.Visible = areVisible
        Me.pbPoster.Visible = areVisible
    End Sub

    Private Sub dlgIMDBSearchResults_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.GotFocus
        Me.AcceptButton = Me.OK_Button
    End Sub

    Private Sub dlgIMDBSearchResults_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.SetUp()
        pnlPicStatus.Visible = False
        AddHandler _IMDB.SearchInfoDownloaded_TV, AddressOf SearchInfoDownloaded
        AddHandler _IMDB.SearchResultsDownloaded_TV, AddressOf SearchResultsDownloaded

        Try
            Dim iBackground As New Bitmap(Me.pnlTop.Width, Me.pnlTop.Height)
            Using g As Graphics = Graphics.FromImage(iBackground)
                g.FillRectangle(New Drawing2D.LinearGradientBrush(Me.pnlTop.ClientRectangle, Color.SteelBlue, Color.LightSteelBlue, Drawing2D.LinearGradientMode.Horizontal), pnlTop.ClientRectangle)
                Me.pnlTop.BackgroundImage = iBackground
            End Using
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
    End Sub

    Private Sub dlgIMDBSearchResults_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Me.Activate()
        Me.tvResults.Focus()
    End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        If Me.chkManual.Checked AndAlso Me.btnVerify.Enabled Then
            If Not Regex.IsMatch(Me.txtIMDBID.Text.Replace("tt", String.Empty), "\d\d\d\d\d\d\d") Then
                MessageBox.Show(Master.eLang.GetString(799, "The ID you entered is not a valid IMDB ID."), Master.eLang.GetString(292, "Invalid Entry"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Exit Sub
            Else
                If MessageBox.Show(String.Concat(Master.eLang.GetString(821, "You have manually entered an IMDB ID but have not verified it is correct."), Environment.NewLine, Environment.NewLine, Master.eLang.GetString(101, "Are you sure you want to continue?")), Master.eLang.GetString(823, "Continue without verification?"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.No Then
                    Exit Sub
                Else
                    _nShow.IMDB = Me.txtIMDBID.Text.Replace("tt", String.Empty)
                End If
            End If
        End If

        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub SearchInfoDownloaded(ByVal sPoster As String, ByVal bSuccess As Boolean)
        Me.pnlLoading.Visible = False
        Me.OK_Button.Enabled = True

        If bSuccess Then
            Me.ControlsVisible(True)
            Me.lblTitle.Text = _nShow.Title
            Me.lblTagline.Text = String.Empty
            Me.lblYear.Text = _nShow.Premiered
            'Me.lblDirector.Text = _nShow.Directors
            Me.lblGenre.Text = _nShow.Genre
            Me.txtPlot.Text = _nShow.Plot
            Me.lblIMDBID.Text = _nShow.IMDB

            If _PosterCache.ContainsKey(_nShow.IMDB) Then
                'just set it
                Me.pbPoster.Image = _PosterCache(_nShow.IMDB)
            Else
                'go download it, if available
                If Not String.IsNullOrEmpty(sPoster) Then
                    If Me.bwDownloadPic.IsBusy Then
                        Me.bwDownloadPic.CancelAsync()
                    End If
                    pnlPicStatus.Visible = True
                    Me.bwDownloadPic = New System.ComponentModel.BackgroundWorker
                    Me.bwDownloadPic.WorkerSupportsCancellation = True
                    Me.bwDownloadPic.RunWorkerAsync(New Arguments With {.pURL = sPoster, .IMDBId = _nShow.IMDB})
                End If

            End If

            'store clone of tmpmovie
            If Not _InfoCache.ContainsKey(_nShow.IMDB) Then
                _InfoCache.Add(_nShow.IMDB, GetTVShowClone(_nShow))
            End If


            Me.btnVerify.Enabled = False
        Else
            If Me.chkManual.Checked Then
                MessageBox.Show(Master.eLang.GetString(825, "Unable to retrieve movie details for the entered IMDB ID. Please check your entry and try again."), Master.eLang.GetString(826, "Verification Failed"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Me.btnVerify.Enabled = True
            End If
        End If
    End Sub

    Private Sub SearchResultsDownloaded(ByVal M As IMDB.SearchResults_TVShow)
        Me.tvResults.Nodes.Clear()
        Me.ClearInfo()
        If M IsNot Nothing AndAlso M.Matches.Count > 0 Then
            For Each Show As MediaContainers.TVShow In M.Matches
                Me.tvResults.Nodes.Add(New TreeNode() With {.Text = String.Concat(Show.Title), .Tag = Show.IMDB})
            Next
            Me.tvResults.SelectedNode = Me.tvResults.Nodes(0)

            Me._prevnode = -2

            Me.tvResults.Focus()
        Else
            Me.tvResults.Nodes.Add(New TreeNode With {.Text = Master.eLang.GetString(833, "No Matches Found")})
        End If
        Me.pnlLoading.Visible = False
        chkManual.Enabled = True
    End Sub

    Private Function SetPreviewOptions() As Structures.ScrapeOptions
        Dim aOpt As New Structures.ScrapeOptions
        aOpt.bMainGenres = True
        aOpt.bMainPlot = True
        aOpt.bMainPremiered = True
        aOpt.bMainTitle = True

        Return aOpt
    End Function

    Private Sub SetUp()
        Me.OK_Button.Text = Master.eLang.GetString(179, "OK")
        Me.Cancel_Button.Text = Master.eLang.GetString(167, "Cancel")
        Me.Label2.Text = Master.eLang.GetString(836, "View details of each result to find the proper movie.")
        Me.Label1.Text = Master.eLang.GetString(948, "TV Search Results")
        Me.chkManual.Text = Master.eLang.GetString(847, "Manual IMDB Entry:")
        Me.btnVerify.Text = Master.eLang.GetString(848, "Verify")
        Me.lblPremieredHeader.Text = String.Concat(Master.eLang.GetString(724, "Premiered"), ":")
        Me.lblDirectorsHeader.Text = String.Concat(Master.eLang.GetString(940, "Directors"), ":")
        Me.lblGenreHeader.Text = Master.eLang.GetString(51, "Genre(s):")
        Me.lblIMDBHeader.Text = Master.eLang.GetString(873, "IMDB ID:")
        Me.lblPlotHeader.Text = Master.eLang.GetString(242, "Plot Outline:")
        Me.Label3.Text = Master.eLang.GetString(798, "Searching IMDB...")
    End Sub

    Private Sub tmrLoad_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrLoad.Tick
        Dim pOpt As New Structures.ScrapeOptions
        pOpt = SetPreviewOptions()

        Me.tmrWait.Stop()
        Me.tmrLoad.Stop()
        Me.pnlLoading.Visible = True
        Me.Label3.Text = Master.eLang.GetString(875, "Downloading details...")

        _IMDB.GetSearchTVShowInfoAsync(Me.tvResults.SelectedNode.Tag.ToString, _nShow, pOpt)
    End Sub

    Private Sub tmrWait_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrWait.Tick
        If Not Me._prevnode = Me._currnode Then
            Me._prevnode = Me._currnode
            Me.tmrWait.Stop()
            Me.tmrLoad.Start()
        Else
            Me.tmrLoad.Stop()
            Me.tmrWait.Stop()
        End If
    End Sub
    Private Sub tvResults_AfterSelect(ByVal sender As System.Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) Handles tvResults.AfterSelect
        Try
            Me.tmrWait.Stop()
            Me.tmrLoad.Stop()

            Me.ClearInfo()
            Me.OK_Button.Enabled = False

            If Me.tvResults.SelectedNode.Tag IsNot Nothing AndAlso Not String.IsNullOrEmpty(Me.tvResults.SelectedNode.Tag.ToString) Then
                Me._currnode = Me.tvResults.SelectedNode.Index

                'check if this movie is in the cache already
                If _InfoCache.ContainsKey(Me.tvResults.SelectedNode.Tag.ToString) Then
                    _nShow = GetTVShowClone(_InfoCache(Me.tvResults.SelectedNode.Tag.ToString))
                    SearchInfoDownloaded(String.Empty, True)
                    Return
                End If

                Me.pnlLoading.Visible = True
                Me.tmrWait.Start()
            Else
                Me.pnlLoading.Visible = False
            End If

        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
    End Sub

    Private Sub tvResults_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles tvResults.GotFocus
        Me.AcceptButton = Me.OK_Button
    End Sub

    Private Sub txtIMDBID_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtIMDBID.GotFocus
        Me.AcceptButton = Me.btnVerify
    End Sub

    Private Sub txtIMDBID_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtIMDBID.TextChanged
        If Me.chkManual.Checked Then
            Me.btnVerify.Enabled = True
            Me.OK_Button.Enabled = False
        End If
    End Sub

    Private Sub txtSearch_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtSearch.GotFocus
        Me.AcceptButton = Me.btnSearch
    End Sub

    Private Function GetTVShowClone(ByVal original As MediaContainers.TVShow) As MediaContainers.TVShow
        Try
            Using mem As New IO.MemoryStream()
                Dim bin As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter(Nothing, New System.Runtime.Serialization.StreamingContext(Runtime.Serialization.StreamingContextStates.Clone))
                bin.Serialize(mem, original)
                mem.Seek(0, IO.SeekOrigin.Begin)
                Return DirectCast(bin.Deserialize(mem), MediaContainers.TVShow)
            End Using
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try

        Return Nothing
    End Function


#End Region 'Methods

#Region "Nested Types"

    Private Structure Arguments

#Region "Fields"

        Dim pURL As String
        Dim IMDBId As String

#End Region 'Fields

    End Structure

    Private Structure Results

#Region "Fields"

        Dim Result As Image
        Dim IMDBId As String

#End Region 'Fields

    End Structure

#End Region 'Nested Types

End Class