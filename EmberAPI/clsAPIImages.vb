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

Imports System.Drawing.Imaging
Imports System.IO
Imports System.Drawing
Imports System.Windows.Forms
Imports NLog


<Serializable()> _
Public Class Images

#Region "Fields"

    Shared logger As Logger = NLog.LogManager.GetCurrentClassLogger()

    Private _image As Image
    Private _ms As MemoryStream

#End Region 'Fields

#Region "Constructors"

    Public Sub New()
        Me.Clear()
    End Sub

#End Region 'Constructors

#Region "Properties"

    Public ReadOnly Property HasMemoryStream() As Boolean
        Get
            Return _ms.Length > 0
        End Get
    End Property

    Public ReadOnly Property [Image]() As Image
        Get
            Return _image
        End Get
    End Property

#End Region 'Properties

#Region "Methods"
    ''' <summary>
    ''' Replaces the internally-held image with the supplied image
    ''' </summary>
    ''' <param name="nImage">The new <c>Image</c> to retain</param>
    ''' <remarks>
    ''' 2013/11/25 Dekker500 - Disposed old image before replacing
    ''' </remarks>
    Public Sub UpdateMSfromImg(nImage As Image)

        Try
            Dim ICI As ImageCodecInfo = GetEncoderInfo(ImageFormat.Jpeg)

            Dim EncPars As EncoderParameters = New EncoderParameters(2)
            EncPars.Param(0) = New EncoderParameter(Encoder.RenderMethod, EncoderValue.RenderNonProgressive)
            EncPars.Param(1) = New EncoderParameter(Encoder.Quality, 100)

            'Write the supplied image into the MemoryStream
            If Not _ms Is Nothing Then _ms.Dispose()
            _ms = New MemoryStream()
            nImage.Save(_ms, ICI, EncPars)
            _ms.Flush()

            'Replace the existing image with the new image
            If Not _image Is Nothing Then _image.Dispose()
            _image = New Bitmap(_ms)

        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name,ex)
        End Try
    End Sub
    ''' <summary>
    ''' Reset this instance to a pristine condition
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Clear()
        _image = Nothing
        _ms = New MemoryStream()
    End Sub
    ''' <summary>
    ''' Delete the given arbitrary file
    ''' </summary>
    ''' <param name="sPath"></param>
    ''' <remarks>This version of Delete is wrapped in a try-catch block which 
    ''' will log errors before safely returning.</remarks>
    Private Shared Sub Delete(ByVal sPath As String)
        If Not String.IsNullOrEmpty(sPath) Then
            Try
                File.Delete(sPath)
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name & Convert.ToChar(Windows.Forms.Keys.Tab) & "Param: <" & sPath & ">", ex)
            End Try
        End If
    End Sub
    ''' <summary>
    ''' Delete all movie images with specified image type
    ''' </summary>
    ''' <param name="DBMovie"></param>
    ''' <param name="ImageType"></param>
    ''' <remarks></remarks>
    Public Shared Sub Delete_Movie(ByVal DBMovie As Database.DBElement, ByVal ImageType As Enums.ModifierType)
        If String.IsNullOrEmpty(DBMovie.Filename) Then Return

        Try
            For Each a In FileUtils.GetFilenameList.Movie(DBMovie, ImageType)
                Select Case ImageType
                    Case Enums.ModifierType.MainActorThumbs
                        Dim tmpPath As String = Directory.GetParent(a.Replace("<placeholder>", "dummy")).FullName
                        If Directory.Exists(tmpPath) Then
                            FileUtils.Delete.DeleteDirectory(tmpPath)
                        End If
                    Case Enums.ModifierType.MainExtrafanarts, Enums.ModifierType.MainExtrathumbs
                        If Directory.Exists(a) Then
                            FileUtils.Delete.DeleteDirectory(a)
                        End If
                    Case Else
                        If File.Exists(a) Then
                            Delete(a)
                        End If
                End Select
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name & Convert.ToChar(Windows.Forms.Keys.Tab) & "<" & DBMovie.Filename & ">", ex)
        End Try
    End Sub
    ''' <summary>
    ''' Delete all movieset images with specified image type
    ''' </summary>
    ''' <param name="DBMovieSet"></param>
    ''' <param name="ImageType"></param>
    ''' <remarks></remarks>
    Public Shared Sub Delete_MovieSet(ByVal DBMovieSet As Database.DBElement, ByVal ImageType As Enums.ModifierType)
        If String.IsNullOrEmpty(DBMovieSet.MovieSet.Title) Then Return

        Try
            For Each a In FileUtils.GetFilenameList.MovieSet(DBMovieSet, ImageType)
                If File.Exists(a) Then
                    Delete(a)
                End If
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name & Convert.ToChar(Windows.Forms.Keys.Tab) & "<" & DBMovieSet.MovieSet.Title & ">", ex)
        End Try
    End Sub
    ''' <summary>
    ''' Delete all tv show AllSeasons images with specified image type
    ''' </summary>
    ''' <param name="DBTVShow"></param>
    ''' <param name="ImageType"></param>
    ''' <remarks></remarks>
    Public Shared Sub Delete_TVAllSeasons(ByVal DBTVShow As Database.DBElement, ByVal ImageType As Enums.ModifierType)
        If String.IsNullOrEmpty(DBTVShow.ShowPath) Then Return

        Try
            For Each a In FileUtils.GetFilenameList.TVShow(DBTVShow, ImageType)
                If File.Exists(a) Then
                    Delete(a)
                End If
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name & Convert.ToChar(Windows.Forms.Keys.Tab) & "<" & DBTVShow.ShowPath & ">", ex)
        End Try
    End Sub
    ''' <summary>
    ''' Delete all tv episode images with specified image type
    ''' </summary>
    ''' <param name="DBTVEpisode"></param>
    ''' <param name="ImageType"></param>
    ''' <remarks></remarks>
    Public Shared Sub Delete_TVEpisode(ByVal DBTVEpisode As Database.DBElement, ByVal ImageType As Enums.ModifierType)
        If String.IsNullOrEmpty(DBTVEpisode.Filename) Then Return

        Try
            For Each a In FileUtils.GetFilenameList.TVEpisode(DBTVEpisode, ImageType)
                Select Case ImageType
                    Case Enums.ModifierType.EpisodeActorThumbs
                        Dim tmpPath As String = Directory.GetParent(a.Replace("<placeholder>", "dummy")).FullName
                        If Directory.Exists(tmpPath) Then
                            FileUtils.Delete.DeleteDirectory(tmpPath)
                        End If
                    Case Else
                        If File.Exists(a) Then
                            Delete(a)
                        End If
                End Select
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name & Convert.ToChar(Windows.Forms.Keys.Tab) & "<" & DBTVEpisode.ShowPath & ">", ex)
        End Try
    End Sub
    ''' <summary>
    ''' Delete all tv season images with specified image type
    ''' </summary>
    ''' <param name="DBTVSeason"></param>
    ''' <param name="ImageType"></param>
    ''' <remarks></remarks>
    Public Shared Sub Delete_TVSeason(ByVal DBTVSeason As Database.DBElement, ByVal ImageType As Enums.ModifierType)
        If String.IsNullOrEmpty(DBTVSeason.ShowPath) Then Return

        Try
            For Each a In FileUtils.GetFilenameList.TVSeason(DBTVSeason, ImageType)
                If File.Exists(a) Then
                    Delete(a)
                End If
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name & Convert.ToChar(Windows.Forms.Keys.Tab) & "<" & DBTVSeason.ShowPath & ">", ex)
        End Try
    End Sub
    ''' <summary>
    ''' Delete all tv show images with specified image type
    ''' </summary>
    ''' <param name="DBTVShow"></param>
    ''' <remarks></remarks>
    Public Shared Sub Delete_TVShow(ByVal DBTVShow As Database.DBElement, ByVal ImageType As Enums.ModifierType)
        If String.IsNullOrEmpty(DBTVShow.ShowPath) Then Return

        Try
            For Each a In FileUtils.GetFilenameList.TVShow(DBTVShow, ImageType)
                Select Case ImageType
                    Case Enums.ModifierType.MainActorThumbs
                        Dim tmpPath As String = Directory.GetParent(a.Replace("<placeholder>", "dummy")).FullName
                        If Directory.Exists(tmpPath) Then
                            FileUtils.Delete.DeleteDirectory(tmpPath)
                        End If
                    Case Enums.ModifierType.MainExtrafanarts
                        If Directory.Exists(a) Then
                            FileUtils.Delete.DeleteDirectory(a)
                        End If
                    Case Else
                        If File.Exists(a) Then
                            Delete(a)
                        End If
                End Select
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name & Convert.ToChar(Windows.Forms.Keys.Tab) & "<" & DBTVShow.ShowPath & ">", ex)
        End Try
    End Sub
    ''' <summary>
    ''' Loads this Image from the contents of the supplied file
    ''' </summary>
    ''' <param name="sPath">Path to the image file</param>
    ''' <param name="LoadBitmap">Create bitmap from memorystream</param>
    ''' <remarks></remarks>
    Public Sub FromFile(ByVal sPath As String, Optional LoadBitmap As Boolean = False)
        If Not String.IsNullOrEmpty(sPath) AndAlso File.Exists(sPath) Then
            'Try
            Me._ms = New MemoryStream()
            Using fsImage As FileStream = File.OpenRead(sPath)
                Dim memStream As New MemoryStream
                memStream.SetLength(fsImage.Length)
                fsImage.Read(memStream.GetBuffer, 0, CInt(Fix(fsImage.Length)))
                Me._ms.Write(memStream.GetBuffer, 0, CInt(Fix(fsImage.Length)))
                Me._ms.Flush()
                If LoadBitmap Then
                    _image = New Bitmap(Me._ms)
                End If
            End Using
        Else
            Me._ms = New MemoryStream
            Me._image = Nothing
        End If
    End Sub

    Public Function FromMemoryStream() As Boolean
        If Me.HasMemoryStream Then
            Me._image = New Bitmap(Me._ms)
            Return True
        Else
            Return False
        End If
    End Function
    ''' <summary>
    ''' Loads this Image from the supplied URL
    ''' </summary>
    ''' <param name="sURL">URL to the image file</param>
    ''' <param name="LoadBitmap">Create bitmap from memorystream</param>
    ''' <remarks></remarks>
    Public Sub FromWeb(ByVal sURL As String, Optional LoadBitmap As Boolean = False)
        If String.IsNullOrEmpty(sURL) Then Return

        Try
            Dim sHTTP As New HTTP
            sHTTP.StartDownloadImage(sURL)
            While sHTTP.IsDownloading
                Application.DoEvents()
                Threading.Thread.Sleep(50)
            End While

            If sHTTP.Image IsNot Nothing Then
                If Me._ms IsNot Nothing Then
                    Me._ms.Dispose()
                End If
                Me._ms = New MemoryStream()

                Dim retSave() As Byte
                retSave = sHTTP.ms.ToArray
                Me._ms.Write(retSave, 0, retSave.Length)

                'I do not copy from the _ms as it could not be a JPG
                '_image = New Bitmap(sHTTP.Image)
                If LoadBitmap Then
                    _image = New Bitmap(sHTTP.Image) '(Me._ms)
                End If

                ' if is not a JPG or PNG we have to convert the memory stream to JPG format
                If Not (sHTTP.isJPG OrElse sHTTP.isPNG) Then
                    UpdateMSfromImg(New Bitmap(_image))
                End If
            End If

        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name & Convert.ToChar(Windows.Forms.Keys.Tab) & "<" & sURL & ">", ex)
        End Try
    End Sub

    Public Sub ResizeExtraFanart(ByVal fromPath As String, ByVal toPath As String)
        Me.FromFile(fromPath)
        Me.Save(toPath)
    End Sub

    Public Sub ResizeExtraThumb(ByVal fromPath As String, ByVal toPath As String)
        Me.FromFile(fromPath)
        Me.Save(toPath)
    End Sub
    ''' <summary>
    ''' Stores the Image to the supplied <paramref name="sPath"/>
    ''' </summary>
    ''' <param name="sPath">Location to store the image</param>
    ''' <remarks></remarks>
    Public Sub Save(ByVal sPath As String)
        If Me._ms.Length > 0 Then
            Dim retSave() As Byte
            Try
                retSave = _ms.ToArray

                'make sure directory exists
                Directory.CreateDirectory(Directory.GetParent(sPath).FullName)
                If sPath.Length <= 260 Then
                    Using fs As New FileStream(sPath, FileMode.Create, FileAccess.Write)
                        fs.Write(retSave, 0, retSave.Length)
                        fs.Flush()
                        fs.Close()
                    End Using
                End If
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
            End Try
        Else
            Throw New ArgumentOutOfRangeException("Looks like MemoryStream is empty")
        End If
    End Sub

    Public Shared Sub SaveMovieActorThumbs(ByVal mMovie As Database.DBElement)
        'First, (Down)Load all actor thumbs from LocalFilePath or URL
        For Each tActor As MediaContainers.Person In mMovie.Movie.Actors
            tActor.Thumb.LoadAndCache(Enums.ContentType.Movie, True)
        Next

        'Secound, remove the old ones
        Images.Delete_Movie(mMovie, Enums.ModifierType.MainActorThumbs)

        'Thirdly, save all actor thumbs
        For Each tActor As MediaContainers.Person In mMovie.Movie.Actors
            If tActor.Thumb.LoadAndCache(Enums.ContentType.Movie, True) Then
                tActor.Thumb.LocalFilePath = tActor.Thumb.ImageOriginal.SaveAsMovieActorThumb(mMovie, tActor)
            End If
        Next
    End Sub
    ''' <summary>
    ''' Save the image as an actor thumbnail
    ''' </summary>
    ''' <param name="aMovie"><c>Database.DBElement</c> representing the movie being referred to</param>
    ''' <param name="actor"><c>MediaContainers.Person</c> representing the actor</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsMovieActorThumb(ByVal aMovie As Database.DBElement, ByVal actor As MediaContainers.Person) As String
        Dim tPath As String = String.Empty

        For Each a In FileUtils.GetFilenameList.Movie(aMovie, Enums.ModifierType.MainActorThumbs)
            tPath = a.Replace("<placeholder>", actor.Name.Replace(" ", "_"))
            Save(tPath)
        Next

        Return tPath
    End Function
    ''' <summary>
    ''' Save the image as a movie banner
    ''' </summary>
    ''' <param name="mMovie"><c>Database.DBElement</c> representing the movie being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsMovieBanner(ByVal mMovie As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Dim doResize As Boolean = False
        If Master.eSettings.MovieBannerResize Then
            If _image Is Nothing Then FromMemoryStream()
            doResize = _image.Width > Master.eSettings.MovieBannerWidth OrElse _image.Height > Master.eSettings.MovieBannerHeight
        End If

        Try
            If doResize Then
                ImageUtils.ResizeImage(_image, Master.eSettings.MovieBannerWidth, Master.eSettings.MovieBannerHeight)
                'need to align _immage and _ms
                UpdateMSfromImg(_image)
            End If

            For Each a In FileUtils.GetFilenameList.Movie(mMovie, Enums.ModifierType.MainBanner)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as a movie ClearArt
    ''' </summary>
    ''' <param name="mMovie"><c>Database.DBElement</c> representing the movie being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsMovieClearArt(ByVal mMovie As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Try
            For Each a In FileUtils.GetFilenameList.Movie(mMovie, Enums.ModifierType.MainClearArt)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as a movie ClearLogo
    ''' </summary>
    ''' <param name="mMovie"><c>Database.DBElement</c> representing the movie being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsMovieClearLogo(ByVal mMovie As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Try
            For Each a In FileUtils.GetFilenameList.Movie(mMovie, Enums.ModifierType.MainClearLogo)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as a movie landscape
    ''' </summary>
    ''' <param name="mMovie"><c>Database.DBElement</c> representing the movie being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsMovieDiscArt(ByVal mMovie As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Try
            For Each a In FileUtils.GetFilenameList.Movie(mMovie, Enums.ModifierType.MainDiscArt)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save all movie Extrafanarts
    ''' </summary>
    ''' <param name="mMovie"><c>Database.DBElement</c> representing the movie being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Shared Function SaveMovieExtrafanarts(ByVal mMovie As Database.DBElement) As String
        Dim efPath As String = String.Empty

        Images.Delete_Movie(mMovie, Enums.ModifierType.MainExtrafanarts)

        For Each eImg As MediaContainers.Image In mMovie.ImagesContainer.Extrafanarts
            If eImg.LoadAndCache(Enums.ContentType.Movie, True) Then
                efPath = eImg.ImageOriginal.SaveAsMovieExtrafanart(mMovie, If(Not String.IsNullOrEmpty(eImg.URLOriginal), Path.GetFileName(eImg.URLOriginal), Path.GetFileName(eImg.LocalFilePath)))
            End If
        Next

        'If efPath is empty (i.e. expert setting enabled but expert extrafanart scraping disabled) it will cause Ember to crash, therefore do check first
        If Not String.IsNullOrEmpty(efPath) Then
            Return Directory.GetParent(efPath).FullName
        Else
            Return String.Empty
        End If
    End Function
    ''' <summary>
    ''' Save the image as a movie's extrafanart
    ''' </summary>
    ''' <param name="mMovie"><c>Structures.DBMovie</c> representing the movie being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsMovieExtrafanart(ByVal mMovie As Database.DBElement, ByVal sName As String) As String
        Dim efPath As String = String.Empty
        Dim iMod As Integer = 0
        Dim iVal As Integer = 1

        If String.IsNullOrEmpty(mMovie.Filename) Then Return efPath

        Dim doResize As Boolean = False
        If Master.eSettings.MovieExtrafanartsResize Then
            If _image Is Nothing Then FromMemoryStream()
            doResize = _image.Width > Master.eSettings.MovieExtrafanartsWidth OrElse _image.Height > Master.eSettings.MovieExtrafanartsHeight
        End If

        Try
            If doResize Then
                ImageUtils.ResizeImage(_image, Master.eSettings.MovieExtrafanartsWidth, Master.eSettings.MovieExtrafanartsHeight)
                'need to align _immage and _ms
                UpdateMSfromImg(_image)
            End If

            For Each a In FileUtils.GetFilenameList.Movie(mMovie, Enums.ModifierType.MainExtrafanarts)
                If Not String.IsNullOrEmpty(a) Then
                    If Not Directory.Exists(a) Then
                        Directory.CreateDirectory(a)
                    End If
                    If String.IsNullOrEmpty(sName) Then
                        iMod = Functions.GetExtrafanartsModifier(a)
                        iVal = iMod + 1
                        sName = Path.Combine(a, String.Concat("extrafanart", iVal, ".jpg"))
                    End If
                    efPath = Path.Combine(a, sName)
                    Save(efPath)
                End If
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try

        Return efPath
    End Function
    ''' <summary>
    ''' Save all movie Extrathumbs
    ''' </summary>
    ''' <param name="mMovie"><c>Database.DBElement</c> representing the movie being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Shared Function SaveMovieExtrathumbs(ByVal mMovie As Database.DBElement) As String
        Dim etPath As String = String.Empty

        Images.Delete_Movie(mMovie, Enums.ModifierType.MainExtrathumbs)

        For Each eImg As MediaContainers.Image In mMovie.ImagesContainer.Extrathumbs.OrderBy(Function(f) f.Index)
            If eImg.LoadAndCache(Enums.ContentType.Movie, True) Then
                etPath = eImg.ImageOriginal.SaveAsMovieExtrathumb(mMovie)
            End If
        Next

        'If etPath is empty (i.e. expert setting enabled but expert extrathumb scraping disabled) it will cause Ember to crash, therefore do check first
        If Not String.IsNullOrEmpty(etPath) Then
            Return Directory.GetParent(etPath).FullName
        Else
            Return String.Empty
        End If
    End Function
    ''' <summary>
    ''' Save the image as a movie's extrathumb
    ''' </summary>
    ''' <param name="mMovie"><c>Database.DBElement</c> representing the movie being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsMovieExtrathumb(ByVal mMovie As Database.DBElement) As String
        Dim etPath As String = String.Empty
        Dim iMod As Integer = 0
        Dim iVal As Integer = 1

        If String.IsNullOrEmpty(mMovie.Filename) Then Return etPath

        Dim doResize As Boolean = False
        If Master.eSettings.MovieExtrathumbsResize Then
            If _image Is Nothing Then FromMemoryStream()
            doResize = _image.Width > Master.eSettings.MovieExtrathumbsWidth OrElse _image.Height > Master.eSettings.MovieExtrathumbsHeight
        End If

        Try
            If doResize Then
                ImageUtils.ResizeImage(_image, Master.eSettings.MovieExtrathumbsWidth, Master.eSettings.MovieExtrathumbsHeight)
                'need to align _immage and _ms
                UpdateMSfromImg(_image)
            End If

            For Each a In FileUtils.GetFilenameList.Movie(mMovie, Enums.ModifierType.MainExtrathumbs)
                If Not String.IsNullOrEmpty(a) Then
                    If Not Directory.Exists(a) Then
                        Directory.CreateDirectory(a)
                    End If
                    iMod = Functions.GetExtrathumbsModifier(a)
                    iVal = iMod + 1
                    etPath = Path.Combine(a, String.Concat("thumb", iVal, ".jpg"))
                    Save(etPath)
                End If
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try

        Return etPath
    End Function
    ''' <summary>
    ''' Save the image as a movie fanart
    ''' </summary>
    ''' <param name="mMovie"><c></c> representing the movie being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsMovieFanart(ByVal mMovie As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Dim doResize As Boolean = False
        If Master.eSettings.MovieFanartResize Then
            If _image Is Nothing Then FromMemoryStream()
            doResize = _image.Width > Master.eSettings.MovieFanartWidth OrElse _image.Height > Master.eSettings.MovieFanartHeight
        End If

        Try
            If doResize Then
                ImageUtils.ResizeImage(_image, Master.eSettings.MovieFanartWidth, Master.eSettings.MovieFanartHeight)
                'need to align _immage and _ms
                UpdateMSfromImg(_image)
            End If

            For Each a In FileUtils.GetFilenameList.Movie(mMovie, Enums.ModifierType.MainFanart)
                Save(a)
                strReturn = a
            Next

            If Master.eSettings.MovieBackdropsAuto AndAlso Directory.Exists(Master.eSettings.MovieBackdropsPath) Then
                Save(String.Concat(Master.eSettings.MovieBackdropsPath, Path.DirectorySeparatorChar, StringUtils.CleanFileName(mMovie.Movie.OriginalTitle), "_tt", mMovie.Movie.IMDBID, ".jpg"))
            End If

        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try

        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as a movie landscape
    ''' </summary>
    ''' <param name="mMovie"><c>Database.DBElement</c> representing the movie being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsMovieLandscape(ByVal mMovie As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Try
            For Each a In FileUtils.GetFilenameList.Movie(mMovie, Enums.ModifierType.MainLandscape)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as a movie poster
    ''' </summary>
    ''' <param name="mMovie"><c>Database.DBElement</c> representing the movie being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsMoviePoster(ByVal mMovie As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Dim doResize As Boolean = False
        If Master.eSettings.MoviePosterResize Then
            If _image Is Nothing Then FromMemoryStream()
            doResize = _image.Width > Master.eSettings.MoviePosterWidth OrElse _image.Height > Master.eSettings.MoviePosterHeight
        End If

        Try
            If doResize Then
                ImageUtils.ResizeImage(_image, Master.eSettings.MoviePosterWidth, Master.eSettings.MoviePosterHeight)
                'need to align _immage and _ms
                UpdateMSfromImg(_image)
            End If

            For Each a In FileUtils.GetFilenameList.Movie(mMovie, Enums.ModifierType.MainPoster)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as a movieset banner
    ''' </summary>
    ''' <param name="mMovieSet"><c>Database.DBElement</c> representing the movieset being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsMovieSetBanner(ByVal mMovieSet As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Dim doResize As Boolean = False
        If Master.eSettings.MovieSetBannerResize Then
            If _image Is Nothing Then FromMemoryStream()
            doResize = _image.Width > Master.eSettings.MovieSetBannerWidth OrElse _image.Height > Master.eSettings.MovieSetBannerHeight
        End If

        Try
            If doResize Then
                ImageUtils.ResizeImage(_image, Master.eSettings.MovieSetBannerWidth, Master.eSettings.MovieSetBannerHeight)
                'need to align _immage and _ms
                UpdateMSfromImg(_image)
            End If

            For Each a In FileUtils.GetFilenameList.MovieSet(mMovieSet, Enums.ModifierType.MainBanner)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as a movieset ClearArt
    ''' </summary>
    ''' <param name="mMovieSet"><c>Database.DBElement</c> representing the movieset being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsMovieSetClearArt(ByVal mMovieSet As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Try
            For Each a In FileUtils.GetFilenameList.MovieSet(mMovieSet, Enums.ModifierType.MainClearArt)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as a movieset ClearLogo
    ''' </summary>
    ''' <param name="mMovieSet"><c>Database.DBElement</c> representing the movieset being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsMovieSetClearLogo(ByVal mMovieSet As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Try
            For Each a In FileUtils.GetFilenameList.MovieSet(mMovieSet, Enums.ModifierType.MainClearLogo)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as a movieset DiscArt
    ''' </summary>
    ''' <param name="mMovieSet"><c>Database.DBElement</c> representing the movieset being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsMovieSetDiscArt(ByVal mMovieSet As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Try
            For Each a In FileUtils.GetFilenameList.MovieSet(mMovieSet, Enums.ModifierType.MainDiscArt)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as a movieset Fanart
    ''' </summary>
    ''' <param name="mMovieSet"><c>Database.DBElement</c> representing the movieset being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsMovieSetFanart(ByVal mMovieSet As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Dim doResize As Boolean = False
        If Master.eSettings.MovieSetFanartResize Then
            If _image Is Nothing Then FromMemoryStream()
            doResize = _image.Width > Master.eSettings.MovieSetFanartWidth OrElse _image.Height > Master.eSettings.MovieSetFanartHeight
        End If

        Try
            If doResize Then
                ImageUtils.ResizeImage(_image, Master.eSettings.MovieSetFanartWidth, Master.eSettings.MovieSetFanartHeight)
                'need to align _immage and _ms
                UpdateMSfromImg(_image)
            End If

            For Each a In FileUtils.GetFilenameList.MovieSet(mMovieSet, Enums.ModifierType.MainFanart)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as a movieset Landscape
    ''' </summary>
    ''' <param name="mMovieSet"><c>Database.DBElement</c> representing the movieset being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsMovieSetLandscape(ByVal mMovieSet As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Try
            For Each a In FileUtils.GetFilenameList.MovieSet(mMovieSet, Enums.ModifierType.MainLandscape)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as a movieset Poster
    ''' </summary>
    ''' <param name="mMovieSet"><c>Database.DBElement</c> representing the movieset being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsMovieSetPoster(ByVal mMovieSet As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Dim doResize As Boolean = False
        If Master.eSettings.MovieSetPosterResize Then
            If _image Is Nothing Then FromMemoryStream()
            doResize = _image.Width > Master.eSettings.MovieSetPosterWidth OrElse _image.Height > Master.eSettings.MovieSetPosterHeight
        End If

        Try
            If doResize Then
                ImageUtils.ResizeImage(_image, Master.eSettings.MovieSetPosterWidth, Master.eSettings.MovieSetPosterHeight)
                'need to align _immage and _ms
                UpdateMSfromImg(_image)
            End If

            For Each a In FileUtils.GetFilenameList.MovieSet(mMovieSet, Enums.ModifierType.MainPoster)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Saves the image as the AllSeason banner
    ''' </summary>
    ''' <param name="mShow">The <c>Database.DBElement</c> representing the show being referenced</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsTVAllSeasonsBanner(ByVal mShow As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Dim doResize As Boolean = False
        If Master.eSettings.TVAllSeasonsBannerResize Then
            If _image Is Nothing Then FromMemoryStream()
            doResize = _image.Width > Master.eSettings.TVAllSeasonsBannerWidth OrElse _image.Height > Master.eSettings.TVAllSeasonsBannerHeight
        End If

        Try
            If doResize Then
                ImageUtils.ResizeImage(_image, Master.eSettings.TVAllSeasonsBannerWidth, Master.eSettings.TVAllSeasonsBannerHeight)
                'need to align _immage and _ms
                UpdateMSfromImg(_image)
            End If

            For Each a In FileUtils.GetFilenameList.TVShow(mShow, Enums.ModifierType.AllSeasonsBanner)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try

        Return strReturn
    End Function
    ''' <summary>
    ''' Saves the image as the AllSeason fanart
    ''' </summary>
    ''' <param name="mShow">The <c>Database.DBElement</c> representing the show being referenced</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsTVAllSeasonsFanart(ByVal mShow As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Dim doResize As Boolean = False
        If Master.eSettings.TVAllSeasonsFanartResize Then
            If _image Is Nothing Then FromMemoryStream()
            doResize = _image.Width > Master.eSettings.TVAllSeasonsFanartWidth OrElse _image.Height > Master.eSettings.TVAllSeasonsFanartHeight
        End If

        Try
            If doResize Then
                ImageUtils.ResizeImage(_image, Master.eSettings.TVAllSeasonsFanartWidth, Master.eSettings.TVAllSeasonsFanartHeight)
                'need to align _immage and _ms
                UpdateMSfromImg(_image)
            End If

            For Each a In FileUtils.GetFilenameList.TVShow(mShow, Enums.ModifierType.AllSeasonsFanart)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try

        Return strReturn
    End Function
    ''' <summary>
    ''' Saves the image as the AllSeason landscape
    ''' </summary>
    ''' <param name="mShow">The <c>Database.DBElement</c> representing the show being referenced</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsTVAllSeasonsLandscape(ByVal mShow As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Try
            For Each a In FileUtils.GetFilenameList.TVShow(mShow, Enums.ModifierType.AllSeasonsLandscape)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try

        Return strReturn
    End Function
    ''' <summary>
    ''' Saves the image as the AllSeason poster
    ''' </summary>
    ''' <param name="mShow">The <c>Database.DBElement</c> representing the show being referenced</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsTVAllSeasonsPoster(ByVal mShow As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Dim doResize As Boolean = False
        If Master.eSettings.TVAllSeasonsPosterResize Then
            If _image Is Nothing Then FromMemoryStream()
            doResize = _image.Width > Master.eSettings.TVAllSeasonsPosterWidth OrElse _image.Height > Master.eSettings.TVAllSeasonsPosterHeight
        End If

        Try
            If doResize Then
                ImageUtils.ResizeImage(_image, Master.eSettings.TVAllSeasonsPosterWidth, Master.eSettings.TVAllSeasonsPosterHeight)
                'need to align _immage and _ms
                UpdateMSfromImg(_image)
            End If

            For Each a In FileUtils.GetFilenameList.TVShow(mShow, Enums.ModifierType.AllSeasonsPoster)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try

        Return strReturn
    End Function

    Public Shared Sub SaveTVEpisodeActorThumbs(ByVal mEpisode As Database.DBElement)
        'First, (Down)Load all actor thumbs from LocalFilePath or URL
        For Each tActor As MediaContainers.Person In mEpisode.TVEpisode.Actors
            tActor.Thumb.LoadAndCache(Enums.ContentType.TV, True)
        Next

        'Secound, remove the old ones
        'Images.DeleteTVEpisodeActorThumbs(mEpisode) 'TODO: find a way to only remove actor thumbs that not needed in other episodes with same actor thumbs path

        'Thirdly, save all actor thumbs
        For Each tActor As MediaContainers.Person In mEpisode.TVEpisode.Actors
            If tActor.Thumb.ImageOriginal.Image IsNot Nothing Then
                tActor.Thumb.LocalFilePath = tActor.Thumb.ImageOriginal.SaveAsTVEpisodeActorThumb(mEpisode, tActor)
            End If
        Next
    End Sub
    ''' <summary>
    ''' Save the image as an actor thumbnail
    ''' </summary>
    ''' <param name="mEpisode"><c>Database.DBElement</c> representing the episode being referred to</param>
    ''' <param name="actor"><c>MediaContainers.Person</c> representing the actor</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsTVEpisodeActorThumb(ByVal mEpisode As Database.DBElement, ByVal actor As MediaContainers.Person) As String
        Dim tPath As String = String.Empty

        For Each a In FileUtils.GetFilenameList.TVEpisode(mEpisode, Enums.ModifierType.EpisodeActorThumbs)
            tPath = a.Replace("<placeholder>", actor.Name.Replace(" ", "_"))
            Save(tPath)
        Next

        Return tPath
    End Function
    ''' <summary>
    ''' Saves the image as the episode fanart
    ''' </summary>
    ''' <param name="mEpisode">The <c>Database.DBElement</c> representing the show being referenced</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsTVEpisodeFanart(ByVal mEpisode As Database.DBElement) As String
        If String.IsNullOrEmpty(mEpisode.Filename) Then Return String.Empty

        Dim strReturn As String = String.Empty

        Dim doResize As Boolean = False
        If Master.eSettings.TVEpisodeFanartResize Then
            If _image Is Nothing Then FromMemoryStream()
            doResize = _image.Width > Master.eSettings.TVEpisodeFanartWidth OrElse _image.Height > Master.eSettings.TVEpisodeFanartHeight
        End If

        Try
            If doResize Then
                ImageUtils.ResizeImage(_image, Master.eSettings.TVEpisodeFanartWidth, Master.eSettings.TVEpisodeFanartHeight)
                'need to align _immage and _ms
                UpdateMSfromImg(_image)
            End If

            For Each a In FileUtils.GetFilenameList.TVEpisode(mEpisode, Enums.ModifierType.EpisodeFanart)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as an episode poster
    ''' </summary>
    ''' <param name="mEpisode">The <c>Database.DBElement</c> representing the show being referenced</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsTVEpisodePoster(ByVal mEpisode As Database.DBElement) As String
        If String.IsNullOrEmpty(mEpisode.Filename) Then Return String.Empty

        Dim strReturn As String = String.Empty

        Dim doResize As Boolean = False
        If Master.eSettings.TVEpisodePosterResize Then
            If _image Is Nothing Then FromMemoryStream()
            doResize = _image.Width > Master.eSettings.TVEpisodePosterWidth OrElse _image.Height > Master.eSettings.TVEpisodePosterHeight
        End If

        Try
            If doResize Then
                ImageUtils.ResizeImage(_image, Master.eSettings.TVEpisodePosterWidth, Master.eSettings.TVEpisodePosterHeight)
                'need to align _immage and _ms
                UpdateMSfromImg(_image)
            End If

            For Each a In FileUtils.GetFilenameList.TVEpisode(mEpisode, Enums.ModifierType.EpisodePoster)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as a TV Show's season banner
    ''' </summary>
    ''' <param name="mSeason"><c>Database.DBElement</c> representing the TV Season being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsTVSeasonBanner(ByVal mSeason As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Dim doResize As Boolean = False
        If Master.eSettings.TVSeasonBannerResize Then
            If _image Is Nothing Then FromMemoryStream()
            doResize = _image.Width > Master.eSettings.TVSeasonBannerWidth OrElse _image.Height > Master.eSettings.TVSeasonBannerHeight
        End If

        Try
            If doResize Then
                ImageUtils.ResizeImage(_image, Master.eSettings.TVSeasonBannerWidth, Master.eSettings.TVSeasonBannerHeight)
                'need to align _immage and _ms
                UpdateMSfromImg(_image)
            End If

            For Each a In FileUtils.GetFilenameList.TVSeason(mSeason, Enums.ModifierType.SeasonBanner)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as the TV Show's season fanart
    ''' </summary>
    ''' <param name="mSeason"><c>Database.DBElement</c> representing the TV Season being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsTVSeasonFanart(ByVal mSeason As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Dim doResize As Boolean = False
        If Master.eSettings.TVSeasonFanartResize Then
            If _image Is Nothing Then FromMemoryStream()
            doResize = _image.Width > Master.eSettings.TVSeasonFanartWidth OrElse _image.Height > Master.eSettings.TVSeasonFanartHeight
        End If

        Try
            If doResize Then
                ImageUtils.ResizeImage(_image, Master.eSettings.TVSeasonFanartWidth, Master.eSettings.TVSeasonFanartHeight)
                'need to align _immage and _ms
                UpdateMSfromImg(_image)
            End If

            For Each a In FileUtils.GetFilenameList.TVSeason(mSeason, Enums.ModifierType.SeasonFanart)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as a TV Show's season landscape
    ''' </summary>
    ''' <param name="mSeason"><c>Database.DBElement</c> representing the TV Season being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsTVSeasonLandscape(ByVal mSeason As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Try
            For Each a In FileUtils.GetFilenameList.TVSeason(mSeason, Enums.ModifierType.SeasonLandscape)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as a TV Show's season poster
    ''' </summary>
    ''' <param name="mSeason"><c>Database.DBElement</c> representing the TV Season being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsTVSeasonPoster(ByVal mSeason As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        Dim doResize As Boolean = False
        If Master.eSettings.TVSeasonPosterResize Then
            If _image Is Nothing Then FromMemoryStream()
            doResize = _image.Width > Master.eSettings.TVSeasonPosterWidth OrElse _image.Height > Master.eSettings.TVSeasonPosterHeight
        End If

        Try
            If doResize Then
                ImageUtils.ResizeImage(_image, Master.eSettings.TVSeasonPosterWidth, Master.eSettings.TVSeasonPosterHeight)
                'need to align _immage and _ms
                UpdateMSfromImg(_image)
            End If

            For Each a In FileUtils.GetFilenameList.TVSeason(mSeason, Enums.ModifierType.SeasonPoster)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function

    Public Shared Sub SaveTVShowActorThumbs(ByVal mShow As Database.DBElement)
        'First, (Down)Load all actor thumbs from LocalFilePath or URL
        For Each tActor As MediaContainers.Person In mShow.TVShow.Actors
            tActor.Thumb.LoadAndCache(Enums.ContentType.TV, True)
        Next

        'Secound, remove the old ones
        Images.Delete_TVShow(mShow, Enums.ModifierType.MainActorThumbs)

        'Thirdly, save all actor thumbs
        For Each tActor As MediaContainers.Person In mShow.TVShow.Actors
            If tActor.Thumb.ImageOriginal.Image IsNot Nothing Then
                tActor.Thumb.LocalFilePath = tActor.Thumb.ImageOriginal.SaveAsTVShowActorThumb(mShow, tActor)
            End If
        Next
    End Sub
    ''' <summary>
    ''' Save the image as an actor thumbnail
    ''' </summary>
    ''' <param name="mShow"><c>Database.DBElement</c> representing the show being referred to</param>
    ''' <param name="actor"><c>MediaContainers.Person</c> representing the actor</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsTVShowActorThumb(ByVal mShow As Database.DBElement, ByVal actor As MediaContainers.Person) As String
        Dim tPath As String = String.Empty

        For Each a In FileUtils.GetFilenameList.TVShow(mShow, Enums.ModifierType.MainActorThumbs)
            tPath = a.Replace("<placeholder>", actor.Name.Replace(" ", "_"))
            Save(tPath)
        Next

        Return tPath
    End Function
    ''' <summary>
    ''' Save the image as a TV Show's banner
    ''' </summary>
    ''' <param name="mShow"><c>Database.DBElement</c> representing the TV Show being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsTVShowBanner(ByVal mShow As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        If String.IsNullOrEmpty(mShow.ShowPath) Then Return strReturn

        Dim doResize As Boolean = False
        If Master.eSettings.TVShowBannerResize Then
            If _image Is Nothing Then FromMemoryStream()
            doResize = _image.Width > Master.eSettings.TVShowBannerWidth OrElse _image.Height > Master.eSettings.TVShowBannerHeight
        End If

        Try
            If doResize Then
                ImageUtils.ResizeImage(_image, Master.eSettings.TVShowBannerWidth, Master.eSettings.TVShowBannerHeight)
                'need to align _immage and _ms
                UpdateMSfromImg(_image)
            End If

            For Each a In FileUtils.GetFilenameList.TVShow(mShow, Enums.ModifierType.MainBanner)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as a TV Show's CharacterArt
    ''' </summary>
    ''' <param name="mShow"><c>Database.DBElement</c> representing the TV Show being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsTVShowCharacterArt(ByVal mShow As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        If String.IsNullOrEmpty(mShow.ShowPath) Then Return strReturn

        Try
            For Each a In FileUtils.GetFilenameList.TVShow(mShow, Enums.ModifierType.MainCharacterArt)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as a TV Show's ClearArt
    ''' </summary>
    ''' <param name="mShow"><c>Database.DBElement</c> representing the TV Show being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsTVShowClearArt(ByVal mShow As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        If String.IsNullOrEmpty(mShow.ShowPath) Then Return strReturn

        Try
            For Each a In FileUtils.GetFilenameList.TVShow(mShow, Enums.ModifierType.MainClearArt)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as a TV Show's ClearLogo
    ''' </summary>
    ''' <param name="mShow"><c>Database.DBElement</c> representing the TV Show being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsTVShowClearLogo(ByVal mShow As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        If String.IsNullOrEmpty(mShow.ShowPath) Then Return strReturn

        Try
            For Each a In FileUtils.GetFilenameList.TVShow(mShow, Enums.ModifierType.MainClearLogo)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function

    Public Shared Function SaveTVShowExtrafanarts(ByVal mShow As Database.DBElement) As String
        Dim efPath As String = String.Empty
        Dim iMod As Integer = 0
        Dim iVal As Integer = 1

        Images.Delete_TVShow(mShow, Enums.ModifierType.MainExtrafanarts)

        For Each eImg As MediaContainers.Image In mShow.ImagesContainer.Extrafanarts
            If eImg.LoadAndCache(Enums.ContentType.TV, True) Then
                efPath = eImg.ImageOriginal.SaveAsTVShowExtrafanart(mShow, If(Not String.IsNullOrEmpty(eImg.URLOriginal), Path.GetFileName(eImg.URLOriginal), Path.GetFileName(eImg.LocalFilePath)))
            End If
        Next

        'If efPath is empty (i.e. expert setting enabled but expert extrafanart scraping disabled) it will cause Ember to crash, therefore do check first
        If Not String.IsNullOrEmpty(efPath) Then
            Return Directory.GetParent(efPath).FullName
        Else
            Return String.Empty
        End If
    End Function
    ''' <summary>
    ''' Save the image as a tv show's extrafanart
    ''' </summary>
    ''' <param name="mShow"><c>Database.DBElement</c> representing the TV Show being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsTVShowExtrafanart(ByVal mShow As Database.DBElement, ByVal sName As String) As String
        Dim efPath As String = String.Empty
        Dim iMod As Integer = 0
        Dim iVal As Integer = 1

        If String.IsNullOrEmpty(mShow.ShowPath) Then Return efPath

        Dim doResize As Boolean = False
        If Master.eSettings.TVShowExtrafanartsResize Then
            If _image Is Nothing Then FromMemoryStream()
            doResize = _image.Width > Master.eSettings.TVShowExtrafanartsWidth OrElse _image.Height > Master.eSettings.TVShowExtrafanartsHeight
        End If

        Try
            If doResize Then
                ImageUtils.ResizeImage(_image, Master.eSettings.TVShowExtrafanartsWidth, Master.eSettings.TVShowExtrafanartsHeight)
                'need to align _immage and _ms
                UpdateMSfromImg(_image)
            End If

            For Each a In FileUtils.GetFilenameList.TVShow(mShow, Enums.ModifierType.MainExtrafanarts)
                If Not String.IsNullOrEmpty(a) Then
                    If Not Directory.Exists(a) Then
                        Directory.CreateDirectory(a)
                    End If
                    If String.IsNullOrEmpty(sName) Then
                        iMod = Functions.GetExtrafanartsModifier(a)
                        iVal = iMod + 1
                        sName = Path.Combine(a, String.Concat("extrafanart", iVal, ".jpg"))
                    End If
                    efPath = Path.Combine(a, sName)
                    Save(efPath)
                End If
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try

        Return efPath
    End Function
    ''' <summary>
    ''' Save the image as a TV Show's fanart
    ''' </summary>
    ''' <param name="mShow"><c>Database.DBElement</c> representing the TV Show being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsTVShowFanart(ByVal mShow As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        If String.IsNullOrEmpty(mShow.ShowPath) Then Return strReturn

        Dim doResize As Boolean = False
        If Master.eSettings.TVShowFanartResize Then
            If _image Is Nothing Then FromMemoryStream()
            doResize = _image.Width > Master.eSettings.TVShowFanartWidth OrElse _image.Height > Master.eSettings.TVShowFanartHeight
        End If

        Try
            If doResize Then
                ImageUtils.ResizeImage(_image, Master.eSettings.TVShowFanartWidth, Master.eSettings.TVShowFanartHeight)
                'need to align _immage and _ms
                UpdateMSfromImg(_image)
            End If

            For Each a In FileUtils.GetFilenameList.TVShow(mShow, Enums.ModifierType.MainFanart)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as a TV Show's landscape
    ''' </summary>
    ''' <param name="mShow"><c>Database.DBElement</c> representing the TV Show being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsTVShowLandscape(ByVal mShow As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        If String.IsNullOrEmpty(mShow.ShowPath) Then Return strReturn

        Try
            For Each a In FileUtils.GetFilenameList.TVShow(mShow, Enums.ModifierType.MainLandscape)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' Save the image as a TV Show's poster
    ''' </summary>
    ''' <param name="mShow"><c>Database.DBElement</c> representing the TV Show being referred to</param>
    ''' <returns><c>String</c> path to the saved image</returns>
    ''' <remarks></remarks>
    Public Function SaveAsTVShowPoster(ByVal mShow As Database.DBElement) As String
        Dim strReturn As String = String.Empty

        If String.IsNullOrEmpty(mShow.ShowPath) Then Return strReturn

        Dim doResize As Boolean = False
        If Master.eSettings.TVShowPosterResize Then
            If _image Is Nothing Then FromMemoryStream()
            doResize = _image.Width > Master.eSettings.TVShowPosterWidth OrElse _image.Height > Master.eSettings.TVShowPosterHeight
        End If

        Try
            If doResize Then
                ImageUtils.ResizeImage(_image, Master.eSettings.TVShowPosterWidth, Master.eSettings.TVShowPosterHeight)
                'need to align _immage and _ms
                UpdateMSfromImg(_image)
            End If

            For Each a In FileUtils.GetFilenameList.TVShow(mShow, Enums.ModifierType.MainPoster)
                Save(a)
                strReturn = a
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return strReturn
    End Function
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="DBElement"></param>
    ''' <param name="DefaultImagesContainer">Contains all original or new images (determined on the basis of preferences) of DBElement. This Container is required for resetting an image.</param>
    ''' <param name="SearchResultsContainer">Contains all new scraped images</param>
    ''' <param name="ScrapeModifier">Defines which images are to be redetermined</param>
    ''' <param name="ContentType"></param>
    ''' <param name="DefaultSeasonImagesContainer">Contains all original or new images (determined on the basis of preferences) of each season. This Container is required for resetting an image.</param>
    ''' <param name="DefaultEpisodeImagesContainer">Contains all original or new images (determined on the basis of preferences) of each episode. This Container is required for resetting an image.</param>
    ''' <param name="IsAutoScraper">Is method called during autoscraping process?</param>
    ''' <remarks></remarks>
    Public Shared Sub SetDefaultImages(ByRef DBElement As Database.DBElement, _
                                       ByRef DefaultImagesContainer As MediaContainers.ImagesContainer, _
                                       ByRef SearchResultsContainer As MediaContainers.SearchResultsContainer, _
                                       ByRef ScrapeModifier As Structures.ScrapeModifier, _
                                       ByRef ContentType As Enums.ContentType, _
                                       Optional ByRef DefaultSeasonImagesContainer As List(Of MediaContainers.EpisodeOrSeasonImagesContainer) = Nothing, _
                                       Optional ByRef DefaultEpisodeImagesContainer As List(Of MediaContainers.EpisodeOrSeasonImagesContainer) = Nothing, _
                                       Optional ByVal IsAutoScraper As Boolean = True)

        Dim DoEpisodeFanart As Boolean = False
        Dim DoEpisodePoster As Boolean = False
        Dim DoMainBanner As Boolean = False
        Dim DoMainCharacterArt As Boolean = False
        Dim DoMainClearArt As Boolean = False
        Dim DoMainClearLogo As Boolean = False
        Dim DoMainDiscArt As Boolean = False
        Dim DoMainExtrafanarts As Boolean = False
        Dim DoMainExtrathumbs As Boolean = False
        Dim DoMainFanart As Boolean = False
        Dim DoMainLandscape As Boolean = False
        Dim DoMainPoster As Boolean = False
        Dim DoSeasonBanner As Boolean = False
        Dim DoSeasonFanart As Boolean = False
        Dim DoSeasonLandscape As Boolean = False
        Dim DoSeasonPoster As Boolean = False

        Select Case ContentType
            Case Enums.ContentType.Movie
                DoMainBanner = ScrapeModifier.MainBanner AndAlso Master.eSettings.MovieBannerAnyEnabled
                DoMainClearArt = ScrapeModifier.MainClearArt AndAlso Master.eSettings.MovieClearArtAnyEnabled
                DoMainClearLogo = ScrapeModifier.MainClearLogo AndAlso Master.eSettings.MovieClearLogoAnyEnabled
                DoMainDiscArt = ScrapeModifier.MainDiscArt AndAlso Master.eSettings.MovieDiscArtAnyEnabled
                DoMainExtrafanarts = ScrapeModifier.MainExtrafanarts AndAlso Master.eSettings.MovieExtrafanartsAnyEnabled
                DoMainExtrathumbs = ScrapeModifier.MainExtrathumbs AndAlso Master.eSettings.MovieExtrathumbsAnyEnabled
                DoMainFanart = ScrapeModifier.MainFanart AndAlso Master.eSettings.MovieFanartAnyEnabled
                DoMainLandscape = ScrapeModifier.MainLandscape AndAlso Master.eSettings.MovieLandscapeAnyEnabled
                DoMainPoster = ScrapeModifier.MainPoster AndAlso Master.eSettings.MoviePosterAnyEnabled
            Case Enums.ContentType.MovieSet
                DoMainBanner = ScrapeModifier.MainBanner AndAlso Master.eSettings.MovieSetBannerAnyEnabled
                DoMainClearArt = ScrapeModifier.MainClearArt AndAlso Master.eSettings.MovieSetClearArtAnyEnabled
                DoMainClearLogo = ScrapeModifier.MainClearLogo AndAlso Master.eSettings.MovieSetClearLogoAnyEnabled
                DoMainDiscArt = ScrapeModifier.MainDiscArt AndAlso Master.eSettings.MovieSetDiscArtAnyEnabled
                DoMainFanart = ScrapeModifier.MainFanart AndAlso Master.eSettings.MovieSetFanartAnyEnabled
                DoMainLandscape = ScrapeModifier.MainLandscape AndAlso Master.eSettings.MovieSetLandscapeAnyEnabled
                DoMainPoster = ScrapeModifier.MainPoster AndAlso Master.eSettings.MovieSetPosterAnyEnabled
            Case Enums.ContentType.TV
                DoEpisodeFanart = ScrapeModifier.EpisodeFanart AndAlso Master.eSettings.TVEpisodeFanartAnyEnabled
                DoEpisodePoster = ScrapeModifier.EpisodePoster AndAlso Master.eSettings.TVEpisodePosterAnyEnabled
                DoMainBanner = ScrapeModifier.MainBanner AndAlso Master.eSettings.TVShowBannerAnyEnabled
                DoMainCharacterArt = ScrapeModifier.MainCharacterArt AndAlso Master.eSettings.TVShowCharacterArtAnyEnabled
                DoMainClearArt = ScrapeModifier.MainClearArt AndAlso Master.eSettings.TVShowClearArtAnyEnabled
                DoMainClearLogo = ScrapeModifier.MainClearLogo AndAlso Master.eSettings.TVShowClearLogoAnyEnabled
                DoMainExtrafanarts = ScrapeModifier.MainExtrafanarts AndAlso Master.eSettings.TVShowExtrafanartsAnyEnabled
                DoMainFanart = ScrapeModifier.MainFanart AndAlso Master.eSettings.TVShowFanartAnyEnabled
                DoMainLandscape = ScrapeModifier.MainLandscape AndAlso Master.eSettings.TVShowLandscapeAnyEnabled
                DoMainPoster = ScrapeModifier.MainPoster AndAlso Master.eSettings.TVShowPosterAnyEnabled
                DoSeasonBanner = ScrapeModifier.SeasonBanner AndAlso Master.eSettings.TVSeasonBannerAnyEnabled
                DoSeasonFanart = ScrapeModifier.SeasonFanart AndAlso Master.eSettings.TVSeasonFanartAnyEnabled
                DoSeasonLandscape = ScrapeModifier.SeasonLandscape AndAlso Master.eSettings.TVSeasonLandscapeAnyEnabled
                DoSeasonPoster = ScrapeModifier.SeasonPoster AndAlso Master.eSettings.TVSeasonPosterAnyEnabled
            Case Enums.ContentType.TVShow
                DoMainBanner = ScrapeModifier.MainBanner AndAlso Master.eSettings.TVShowBannerAnyEnabled
                DoMainCharacterArt = ScrapeModifier.MainCharacterArt AndAlso Master.eSettings.TVShowCharacterArtAnyEnabled
                DoMainClearArt = ScrapeModifier.MainClearArt AndAlso Master.eSettings.TVShowClearArtAnyEnabled
                DoMainClearLogo = ScrapeModifier.MainClearLogo AndAlso Master.eSettings.TVShowClearLogoAnyEnabled
                DoMainExtrafanarts = ScrapeModifier.MainExtrafanarts AndAlso Master.eSettings.TVShowExtrafanartsAnyEnabled
                DoMainFanart = ScrapeModifier.MainFanart AndAlso Master.eSettings.TVShowFanartAnyEnabled
                DoMainLandscape = ScrapeModifier.MainLandscape AndAlso Master.eSettings.TVShowLandscapeAnyEnabled
                DoMainPoster = ScrapeModifier.MainPoster AndAlso Master.eSettings.TVShowPosterAnyEnabled
            Case Enums.ContentType.TVEpisode
                DoEpisodeFanart = ScrapeModifier.EpisodeFanart AndAlso Master.eSettings.TVEpisodeFanartAnyEnabled
                DoEpisodePoster = ScrapeModifier.EpisodePoster AndAlso Master.eSettings.TVEpisodePosterAnyEnabled
            Case Enums.ContentType.TVSeason
                DoSeasonBanner = (ScrapeModifier.SeasonBanner AndAlso Master.eSettings.TVSeasonBannerAnyEnabled) OrElse (ScrapeModifier.AllSeasonsBanner AndAlso Master.eSettings.TVAllSeasonsBannerAnyEnabled)
                DoSeasonFanart = (ScrapeModifier.SeasonFanart AndAlso Master.eSettings.TVSeasonFanartAnyEnabled) OrElse (ScrapeModifier.AllSeasonsFanart AndAlso Master.eSettings.TVAllSeasonsFanartAnyEnabled)
                DoSeasonLandscape = (ScrapeModifier.SeasonLandscape AndAlso Master.eSettings.TVSeasonLandscapeAnyEnabled) OrElse (ScrapeModifier.AllSeasonsLandscape AndAlso Master.eSettings.TVAllSeasonsLandscapeAnyEnabled)
                DoSeasonPoster = (ScrapeModifier.SeasonPoster AndAlso Master.eSettings.TVSeasonPosterAnyEnabled) OrElse (ScrapeModifier.AllSeasonsPoster AndAlso Master.eSettings.TVAllSeasonsPosterAnyEnabled)
        End Select

        'Main Banner
        If DoMainBanner OrElse DoSeasonBanner Then
            Dim defImg As MediaContainers.Image = Nothing

            Select Case ContentType
                Case Enums.ContentType.Movie
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.Banner.LocalFilePath) OrElse Not Master.eSettings.MovieBannerKeepExisting Then
                        Images.GetPreferredMovieBanner(SearchResultsContainer.MainBanners, defImg)
                    End If
                Case Enums.ContentType.MovieSet
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.Banner.LocalFilePath) OrElse Not Master.eSettings.MovieSetBannerKeepExisting Then
                        Images.GetPreferredMovieSetBanner(SearchResultsContainer.MainBanners, defImg)
                    End If
                Case Enums.ContentType.TV, Enums.ContentType.TVShow
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.Banner.LocalFilePath) OrElse Not Master.eSettings.TVShowBannerKeepExisting Then
                        Images.GetPreferredTVShowBanner(SearchResultsContainer.MainBanners, defImg)
                    End If
                Case Enums.ContentType.TVSeason
                    If DBElement.TVSeason.Season = 999 Then
                        If String.IsNullOrEmpty(DBElement.ImagesContainer.Banner.LocalFilePath) OrElse Not Master.eSettings.TVAllSeasonsBannerKeepExisting Then
                            Images.GetPreferredTVAllSeasonsBanner(SearchResultsContainer.SeasonBanners, SearchResultsContainer.MainBanners, defImg)
                        End If
                    Else
                        If String.IsNullOrEmpty(DBElement.ImagesContainer.Banner.LocalFilePath) OrElse Not Master.eSettings.TVSeasonBannerKeepExisting Then
                            Images.GetPreferredTVSeasonBanner(SearchResultsContainer.SeasonBanners, defImg, DBElement.TVSeason.Season)
                        End If
                    End If
            End Select

            If defImg IsNot Nothing Then
                DBElement.ImagesContainer.Banner = defImg
                DefaultImagesContainer.Banner = defImg
            Else
                DefaultImagesContainer.Banner = DBElement.ImagesContainer.Banner
            End If
        Else
            DefaultImagesContainer.Banner = DBElement.ImagesContainer.Banner
        End If

        'Main CharacterArt
        If DoMainCharacterArt Then
            Dim defImg As MediaContainers.Image = Nothing

            Select Case ContentType
                Case Enums.ContentType.TV, Enums.ContentType.TVShow
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.CharacterArt.LocalFilePath) OrElse Not Master.eSettings.TVShowCharacterArtKeepExisting Then
                        Images.GetPreferredTVShowCharacterArt(SearchResultsContainer.MainCharacterArts, defImg)
                    End If
            End Select

            If defImg IsNot Nothing Then
                DBElement.ImagesContainer.CharacterArt = defImg
                DefaultImagesContainer.CharacterArt = defImg
            Else
                DefaultImagesContainer.CharacterArt = DBElement.ImagesContainer.CharacterArt
            End If
        Else
            DefaultImagesContainer.CharacterArt = DBElement.ImagesContainer.CharacterArt
        End If

        'Main ClearArt
        If DoMainClearArt Then
            Dim defImg As MediaContainers.Image = Nothing

            Select Case ContentType
                Case Enums.ContentType.Movie
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.ClearArt.LocalFilePath) OrElse Not Master.eSettings.MovieClearArtKeepExisting Then
                        Images.GetPreferredMovieClearArt(SearchResultsContainer.MainClearArts, defImg)
                    End If
                Case Enums.ContentType.MovieSet
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.ClearArt.LocalFilePath) OrElse Not Master.eSettings.MovieSetClearArtKeepExisting Then
                        Images.GetPreferredMovieSetClearArt(SearchResultsContainer.MainClearArts, defImg)
                    End If
                Case Enums.ContentType.TV, Enums.ContentType.TVShow
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.ClearArt.LocalFilePath) OrElse Not Master.eSettings.TVShowClearArtKeepExisting Then
                        Images.GetPreferredTVShowClearArt(SearchResultsContainer.MainClearArts, defImg)
                    End If
            End Select

            If defImg IsNot Nothing Then
                DBElement.ImagesContainer.ClearArt = defImg
                DefaultImagesContainer.ClearArt = defImg
            Else
                DefaultImagesContainer.ClearArt = DBElement.ImagesContainer.ClearArt
            End If
        Else
            DefaultImagesContainer.ClearArt = DBElement.ImagesContainer.ClearArt
        End If

        'Main ClearLogo
        If DoMainClearLogo Then
            Dim defImg As MediaContainers.Image = Nothing

            Select Case ContentType
                Case Enums.ContentType.Movie
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.ClearLogo.LocalFilePath) OrElse Not Master.eSettings.MovieClearLogoKeepExisting Then
                        Images.GetPreferredMovieClearLogo(SearchResultsContainer.MainClearLogos, defImg)
                    End If
                Case Enums.ContentType.MovieSet
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.ClearLogo.LocalFilePath) OrElse Not Master.eSettings.MovieSetClearLogoKeepExisting Then
                        Images.GetPreferredMovieSetClearLogo(SearchResultsContainer.MainClearLogos, defImg)
                    End If
                Case Enums.ContentType.TV, Enums.ContentType.TVShow
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.ClearLogo.LocalFilePath) OrElse Not Master.eSettings.TVShowClearLogoKeepExisting Then
                        Images.GetPreferredTVShowClearLogo(SearchResultsContainer.MainClearLogos, defImg)
                    End If
            End Select

            If defImg IsNot Nothing Then
                DBElement.ImagesContainer.ClearLogo = defImg
                DefaultImagesContainer.ClearLogo = defImg
            Else
                DefaultImagesContainer.ClearLogo = DBElement.ImagesContainer.ClearLogo
            End If
        Else
            DefaultImagesContainer.ClearLogo = DBElement.ImagesContainer.ClearLogo
        End If

        'Main DiscArt
        If DoMainDiscArt Then
            Dim defImg As MediaContainers.Image = Nothing

            Select Case ContentType
                Case Enums.ContentType.Movie
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.DiscArt.LocalFilePath) OrElse Not Master.eSettings.MovieDiscArtKeepExisting Then
                        Images.GetPreferredMovieDiscArt(SearchResultsContainer.MainDiscArts, defImg)
                    End If
                Case Enums.ContentType.MovieSet
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.DiscArt.LocalFilePath) OrElse Not Master.eSettings.MovieSetDiscArtKeepExisting Then
                        Images.GetPreferredMovieSetDiscArt(SearchResultsContainer.MainDiscArts, defImg)
                    End If
            End Select

            If defImg IsNot Nothing Then
                DBElement.ImagesContainer.DiscArt = defImg
                DefaultImagesContainer.DiscArt = defImg
            Else
                DefaultImagesContainer.DiscArt = DBElement.ImagesContainer.DiscArt
            End If
        Else
            DefaultImagesContainer.DiscArt = DBElement.ImagesContainer.DiscArt
        End If

        'Remove duplicate fanart from image scraperresults
        If DoMainExtrafanarts OrElse DoMainExtrathumbs OrElse DoMainFanart Then
            'If process is autoscraper, then make sure that extraimages is not the same as main image of movie (i.e. fanart.jpg of movie should not be part of extrafanart)
            If IsAutoScraper = True AndAlso Master.eSettings.GeneralImageFilterAutoscraper Then
                RemoveDuplicateImages(SearchResultsContainer.MainFanarts, DBElement.ImagesContainer.Fanart, MatchTolerance:=Master.eSettings.GeneralImageFilterFanartMatchTolerance)
            ElseIf IsAutoScraper = False AndAlso Master.eSettings.GeneralImageFilterImagedialog Then
                'only remove duplicates in the scraped imagelist, do not consider main image of movie (else current image of movie would not be selectable in image preview window!)
                RemoveDuplicateImages(SearchResultsContainer.MainFanarts, MatchTolerance:=Master.eSettings.GeneralImageFilterFanartMatchTolerance)
            End If
        End If
        'Remove duplicate posters from image scraperresults
        If DoMainPoster Then
            'If process is autoscraper, then make sure that extraimages is not the same as main image of movie (i.e. fanart.jpg of movie should not be part of extrafanart)
            If IsAutoScraper = True AndAlso Master.eSettings.GeneralImageFilterAutoscraper Then
                'since the following algorithm removes duplicates from beginning of list and keeps the images instance which is lower in list, we reverse the list to place images in preferred language at the end of the list
                SearchResultsContainer.MainPosters.Reverse()
                RemoveDuplicateImages(SearchResultsContainer.MainPosters, DBElement.ImagesContainer.Poster, MatchTolerance:=Master.eSettings.GeneralImageFilterPosterMatchTolerance)
            ElseIf IsAutoScraper = False AndAlso Master.eSettings.GeneralImageFilterImagedialog Then
                'since the following algorithm removes duplicates from beginning of list and keeps the images instance which is lower in list, we reverse the list to place images in preferred language at the end of the list
                SearchResultsContainer.MainPosters.Reverse()
                'only remove duplicates in the scraped imagelist, do not consider main image of movie (else current image of movie would not be selectable in image preview window!)
                RemoveDuplicateImages(SearchResultsContainer.MainPosters, MatchTolerance:=Master.eSettings.GeneralImageFilterPosterMatchTolerance)
            End If
        End If

        'Main Extrafanarts
        If DoMainExtrafanarts Then
            Dim bKeepExisting As Boolean = False
            Dim iLimit As Integer = 0
            Dim iDifference As Integer = 0

            Select Case ContentType
                Case Enums.ContentType.Movie
                    bKeepExisting = Master.eSettings.MovieExtrafanartsKeepExisting
                    iLimit = Master.eSettings.MovieExtrafanartsLimit
                Case Enums.ContentType.TV, Enums.ContentType.TVShow
                    bKeepExisting = Master.eSettings.TVShowExtrafanartsKeepExisting
                    iLimit = Master.eSettings.TVShowExtrafanartsLimit
            End Select

            If Not bKeepExisting OrElse Not DBElement.ImagesContainer.Extrafanarts.Count >= iLimit OrElse iLimit = 0 Then
                iDifference = iLimit - DBElement.ImagesContainer.Extrafanarts.Count
                Dim defImgList As New List(Of MediaContainers.Image)

                Select Case ContentType
                    Case Enums.ContentType.Movie
                        Images.GetPreferredMovieExtrafanarts(SearchResultsContainer.MainFanarts, defImgList, If(Not bKeepExisting, iLimit, iDifference))
                    Case Enums.ContentType.TV, Enums.ContentType.TVShow
                        Images.GetPreferredTVShowExtrafanarts(SearchResultsContainer.MainFanarts, defImgList, If(Not bKeepExisting, iLimit, iDifference))
                End Select

                If Not bKeepExisting Then
                    DBElement.ImagesContainer.Extrafanarts = defImgList
                    DefaultImagesContainer.Extrafanarts = DBElement.ImagesContainer.Extrafanarts
                Else
                    DBElement.ImagesContainer.Extrafanarts.AddRange(defImgList)
                    DefaultImagesContainer.Extrafanarts.AddRange(DBElement.ImagesContainer.Extrafanarts)
                End If
            Else
                DefaultImagesContainer.Extrafanarts = DBElement.ImagesContainer.Extrafanarts
            End If
        Else
            DefaultImagesContainer.Extrafanarts = DBElement.ImagesContainer.Extrafanarts
        End If

        'Main Extrathumbs
        If DoMainExtrathumbs Then
            Dim bKeepExisting As Boolean = False
            Dim iLimit As Integer = 0
            Dim iDifference As Integer = 0

            Select Case ContentType
                Case Enums.ContentType.Movie
                    bKeepExisting = Master.eSettings.MovieExtrathumbsKeepExisting
                    iLimit = Master.eSettings.MovieExtrathumbsLimit
            End Select

            If Not bKeepExisting OrElse Not DBElement.ImagesContainer.Extrathumbs.Count >= iLimit OrElse iLimit = 0 Then
                iDifference = iLimit - DBElement.ImagesContainer.Extrathumbs.Count
                Dim defImgList As New List(Of MediaContainers.Image)

                Select Case ContentType
                    Case Enums.ContentType.Movie
                        Images.GetPreferredMovieExtrathumbs(SearchResultsContainer.MainFanarts, defImgList, If(Not bKeepExisting, iLimit, iDifference))
                End Select

                If Not bKeepExisting Then
                    DBElement.ImagesContainer.Extrathumbs = (defImgList)
                    DefaultImagesContainer.Extrathumbs = (DBElement.ImagesContainer.Extrathumbs)
                Else
                    DBElement.ImagesContainer.Extrathumbs.AddRange(defImgList)
                    DefaultImagesContainer.Extrathumbs.AddRange(DBElement.ImagesContainer.Extrathumbs)
                End If
            Else
                DefaultImagesContainer.Extrathumbs = DBElement.ImagesContainer.Extrathumbs
            End If
        Else
            DefaultImagesContainer.Extrathumbs = DBElement.ImagesContainer.Extrathumbs
        End If

        'Main Fanart
        If DoMainFanart OrElse DoEpisodeFanart OrElse DoSeasonFanart Then
            Dim defImg As MediaContainers.Image = Nothing

            Select Case ContentType
                Case Enums.ContentType.Movie
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.Fanart.LocalFilePath) OrElse Not Master.eSettings.MovieFanartKeepExisting Then
                        Images.GetPreferredMovieFanart(SearchResultsContainer.MainFanarts, defImg)
                    End If
                Case Enums.ContentType.MovieSet
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.Fanart.LocalFilePath) OrElse Not Master.eSettings.MovieSetFanartKeepExisting Then
                        Images.GetPreferredMovieSetFanart(SearchResultsContainer.MainFanarts, defImg)
                    End If
                Case Enums.ContentType.TV, Enums.ContentType.TVShow
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.Fanart.LocalFilePath) OrElse Not Master.eSettings.TVShowFanartKeepExisting Then
                        Images.GetPreferredTVShowFanart(SearchResultsContainer.MainFanarts, defImg)
                    End If
                Case Enums.ContentType.TVEpisode
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.Fanart.LocalFilePath) OrElse Not Master.eSettings.TVEpisodeFanartKeepExisting Then
                        Images.GetPreferredTVEpisodeFanart(SearchResultsContainer.EpisodeFanarts, SearchResultsContainer.MainFanarts, defImg, DBElement.TVEpisode.Season, DBElement.TVEpisode.Episode)
                    End If
                Case Enums.ContentType.TVSeason
                    If DBElement.TVSeason.Season = 999 Then
                        If String.IsNullOrEmpty(DBElement.ImagesContainer.Fanart.LocalFilePath) OrElse Not Master.eSettings.TVAllSeasonsFanartKeepExisting Then
                            Images.GetPreferredTVAllSeasonsFanart(SearchResultsContainer.SeasonFanarts, SearchResultsContainer.MainFanarts, defImg)
                        End If
                    Else
                        If String.IsNullOrEmpty(DBElement.ImagesContainer.Fanart.LocalFilePath) OrElse Not Master.eSettings.TVSeasonFanartKeepExisting Then
                            Images.GetPreferredTVSeasonFanart(SearchResultsContainer.SeasonFanarts, SearchResultsContainer.MainFanarts, defImg, DBElement.TVSeason.Season)
                        End If
                    End If
            End Select

            If defImg IsNot Nothing Then
                DBElement.ImagesContainer.Fanart = defImg
                DefaultImagesContainer.Fanart = defImg
            Else
                DefaultImagesContainer.Fanart = DBElement.ImagesContainer.Fanart
            End If
        Else
            DefaultImagesContainer.Fanart = DBElement.ImagesContainer.Fanart
        End If

        'Main Landscape
        If DoMainLandscape OrElse DoSeasonLandscape Then
            Dim defImg As MediaContainers.Image = Nothing

            Select Case ContentType
                Case Enums.ContentType.Movie
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.Landscape.LocalFilePath) OrElse Not Master.eSettings.MovieLandscapeKeepExisting Then
                        Images.GetPreferredMovieLandscape(SearchResultsContainer.MainLandscapes, defImg)
                    End If
                Case Enums.ContentType.MovieSet
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.Landscape.LocalFilePath) OrElse Not Master.eSettings.MovieSetLandscapeKeepExisting Then
                        Images.GetPreferredMovieSetLandscape(SearchResultsContainer.MainLandscapes, defImg)
                    End If
                Case Enums.ContentType.TV, Enums.ContentType.TVShow
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.Landscape.LocalFilePath) OrElse Not Master.eSettings.TVShowLandscapeKeepExisting Then
                        Images.GetPreferredTVShowLandscape(SearchResultsContainer.MainLandscapes, defImg)
                    End If
                Case Enums.ContentType.TVSeason
                    If DBElement.TVSeason.Season = 999 Then
                        If String.IsNullOrEmpty(DBElement.ImagesContainer.Landscape.LocalFilePath) OrElse Not Master.eSettings.TVAllSeasonsLandscapeKeepExisting Then
                            Images.GetPreferredTVAllSeasonsLandscape(SearchResultsContainer.SeasonLandscapes, SearchResultsContainer.MainLandscapes, defImg)
                        End If
                    Else
                        If String.IsNullOrEmpty(DBElement.ImagesContainer.Landscape.LocalFilePath) OrElse Not Master.eSettings.TVSeasonLandscapeKeepExisting Then
                            Images.GetPreferredTVSeasonLandscape(SearchResultsContainer.SeasonLandscapes, defImg, DBElement.TVSeason.Season)
                        End If
                    End If
            End Select

            If defImg IsNot Nothing Then
                DBElement.ImagesContainer.Landscape = defImg
                DefaultImagesContainer.Landscape = defImg
            Else
                DefaultImagesContainer.Landscape = DBElement.ImagesContainer.Landscape
            End If
        Else
            DefaultImagesContainer.Landscape = DBElement.ImagesContainer.Landscape
        End If

        'Main Poster
        If DoMainPoster OrElse DoEpisodePoster OrElse DoSeasonPoster Then
            Dim defImg As MediaContainers.Image = Nothing

            Select Case ContentType
                Case Enums.ContentType.Movie
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.Poster.LocalFilePath) OrElse Not Master.eSettings.MoviePosterKeepExisting Then
                        Images.GetPreferredMoviePoster(SearchResultsContainer.MainPosters, defImg)
                    End If
                Case Enums.ContentType.MovieSet
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.Poster.LocalFilePath) OrElse Not Master.eSettings.MovieSetPosterKeepExisting Then
                        Images.GetPreferredMovieSetPoster(SearchResultsContainer.MainPosters, defImg)
                    End If
                Case Enums.ContentType.TV, Enums.ContentType.TVShow
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.Poster.LocalFilePath) OrElse Not Master.eSettings.TVShowPosterKeepExisting Then
                        Images.GetPreferredTVShowPoster(SearchResultsContainer.MainPosters, defImg)
                    End If
                Case Enums.ContentType.TVEpisode
                    If String.IsNullOrEmpty(DBElement.ImagesContainer.Poster.LocalFilePath) OrElse Not Master.eSettings.TVEpisodePosterKeepExisting Then
                        Images.GetPreferredTVEpisodePoster(SearchResultsContainer.EpisodePosters, defImg, DBElement.TVEpisode.Season, DBElement.TVEpisode.Episode)
                    End If
                Case Enums.ContentType.TVSeason
                    If DBElement.TVSeason.Season = 999 Then
                        If String.IsNullOrEmpty(DBElement.ImagesContainer.Poster.LocalFilePath) OrElse Not Master.eSettings.TVAllSeasonsPosterKeepExisting Then
                            Images.GetPreferredTVAllSeasonsPoster(SearchResultsContainer.SeasonPosters, SearchResultsContainer.MainPosters, defImg)
                        End If
                    Else
                        If String.IsNullOrEmpty(DBElement.ImagesContainer.Poster.LocalFilePath) OrElse Not Master.eSettings.TVSeasonPosterKeepExisting Then
                            Images.GetPreferredTVSeasonPoster(SearchResultsContainer.SeasonPosters, defImg, DBElement.TVSeason.Season)
                        End If
                    End If
            End Select

            If defImg IsNot Nothing Then
                DBElement.ImagesContainer.Poster = defImg
                DefaultImagesContainer.Poster = defImg
            Else
                DefaultImagesContainer.Poster = DBElement.ImagesContainer.Poster
            End If
        Else
            DefaultImagesContainer.Poster = DBElement.ImagesContainer.Poster
        End If

        'Episodes while tv show scraping
        If DefaultEpisodeImagesContainer IsNot Nothing Then
            For Each sEpisode As Database.DBElement In DBElement.Episodes
                Dim sContainer As New MediaContainers.EpisodeOrSeasonImagesContainer With {.Episode = sEpisode.TVEpisode.Episode, .Season = sEpisode.TVEpisode.Season}

                'Fanart
                If DoEpisodeFanart Then
                    Dim defImg As MediaContainers.Image = Nothing
                    If String.IsNullOrEmpty(sEpisode.ImagesContainer.Fanart.LocalFilePath) OrElse Not Master.eSettings.TVEpisodeFanartKeepExisting Then
                        Images.GetPreferredTVEpisodeFanart(SearchResultsContainer.EpisodeFanarts, SearchResultsContainer.MainFanarts, defImg, sContainer.Season, sContainer.Episode)
                    End If

                    If defImg IsNot Nothing Then
                        sEpisode.ImagesContainer.Fanart = defImg
                        sContainer.Fanart = defImg
                    Else
                        sContainer.Fanart = sEpisode.ImagesContainer.Fanart
                    End If
                Else
                    sContainer.Fanart = sEpisode.ImagesContainer.Fanart
                End If

                'Poster
                If DoEpisodePoster Then
                    Dim defImg As MediaContainers.Image = Nothing
                    If String.IsNullOrEmpty(sEpisode.ImagesContainer.Poster.LocalFilePath) OrElse Not Master.eSettings.TVEpisodePosterKeepExisting Then
                        Images.GetPreferredTVEpisodePoster(SearchResultsContainer.EpisodePosters, defImg, sContainer.Season, sContainer.Episode)
                    End If

                    If defImg IsNot Nothing Then
                        sEpisode.ImagesContainer.Poster = defImg
                        sContainer.Poster = defImg
                    Else
                        sContainer.Poster = sEpisode.ImagesContainer.Poster
                    End If
                Else
                    sContainer.Poster = sEpisode.ImagesContainer.Poster
                End If

                DefaultEpisodeImagesContainer.Add(sContainer)
            Next
        End If

        'Seasons while tv show scraping
        If DefaultSeasonImagesContainer IsNot Nothing Then
            For Each sSeason As Database.DBElement In DBElement.Seasons
                Dim sContainer As New MediaContainers.EpisodeOrSeasonImagesContainer With {.Season = sSeason.TVSeason.Season}

                'Banner
                If DoSeasonBanner Then
                    Dim defImg As MediaContainers.Image = Nothing
                    If sSeason.TVSeason.Season = 999 Then
                        If String.IsNullOrEmpty(sSeason.ImagesContainer.Banner.LocalFilePath) OrElse Not Master.eSettings.TVAllSeasonsBannerKeepExisting Then
                            Images.GetPreferredTVAllSeasonsBanner(SearchResultsContainer.SeasonBanners, SearchResultsContainer.MainBanners, defImg)
                        End If
                    Else
                        If String.IsNullOrEmpty(sSeason.ImagesContainer.Banner.LocalFilePath) OrElse Not Master.eSettings.TVSeasonBannerKeepExisting Then
                            Images.GetPreferredTVSeasonBanner(SearchResultsContainer.SeasonBanners, defImg, sContainer.Season)
                        End If
                    End If

                    If defImg IsNot Nothing Then
                        sSeason.ImagesContainer.Banner = defImg
                        sContainer.Banner = defImg
                    Else
                        sContainer.Banner = sSeason.ImagesContainer.Banner
                    End If
                Else
                    sContainer.Banner = sSeason.ImagesContainer.Banner
                End If

                'Fanart
                If DoSeasonFanart Then
                    Dim defImg As MediaContainers.Image = Nothing
                    If sSeason.TVSeason.Season = 999 Then
                        If String.IsNullOrEmpty(sSeason.ImagesContainer.Fanart.LocalFilePath) OrElse Not Master.eSettings.TVAllSeasonsFanartKeepExisting Then
                            Images.GetPreferredTVAllSeasonsFanart(SearchResultsContainer.SeasonFanarts, SearchResultsContainer.MainFanarts, defImg)
                        End If
                    Else
                        If String.IsNullOrEmpty(sSeason.ImagesContainer.Fanart.LocalFilePath) OrElse Not Master.eSettings.TVSeasonFanartKeepExisting Then
                            Images.GetPreferredTVSeasonFanart(SearchResultsContainer.SeasonFanarts, SearchResultsContainer.MainFanarts, defImg, sContainer.Season)
                        End If
                    End If

                    If defImg IsNot Nothing Then
                        sSeason.ImagesContainer.Fanart = defImg
                        sContainer.Fanart = defImg
                    Else
                        sContainer.Fanart = sSeason.ImagesContainer.Fanart
                    End If
                Else
                    sContainer.Fanart = sSeason.ImagesContainer.Fanart
                End If

                'Landscape
                If DoSeasonLandscape Then
                    Dim defImg As MediaContainers.Image = Nothing
                    If sSeason.TVSeason.Season = 999 Then
                        If String.IsNullOrEmpty(sSeason.ImagesContainer.Landscape.LocalFilePath) OrElse Not Master.eSettings.TVAllSeasonsLandscapeKeepExisting Then
                            Images.GetPreferredTVAllSeasonsLandscape(SearchResultsContainer.SeasonLandscapes, SearchResultsContainer.MainLandscapes, defImg)
                        End If
                    Else
                        If String.IsNullOrEmpty(sSeason.ImagesContainer.Landscape.LocalFilePath) OrElse Not Master.eSettings.TVSeasonLandscapeKeepExisting Then
                            Images.GetPreferredTVSeasonLandscape(SearchResultsContainer.SeasonLandscapes, defImg, sContainer.Season)
                        End If
                    End If

                    If defImg IsNot Nothing Then
                        sSeason.ImagesContainer.Landscape = defImg
                        sContainer.Landscape = defImg
                    Else
                        sContainer.Landscape = sSeason.ImagesContainer.Landscape
                    End If
                Else
                    sContainer.Landscape = sSeason.ImagesContainer.Landscape
                End If

                'Poster
                If DoSeasonPoster Then
                    Dim defImg As MediaContainers.Image = Nothing
                    If sSeason.TVSeason.Season = 999 Then
                        If String.IsNullOrEmpty(sSeason.ImagesContainer.Poster.LocalFilePath) OrElse Not Master.eSettings.TVAllSeasonsPosterKeepExisting Then
                            Images.GetPreferredTVAllSeasonsPoster(SearchResultsContainer.SeasonPosters, SearchResultsContainer.MainPosters, defImg)
                        End If
                    Else
                        If String.IsNullOrEmpty(sSeason.ImagesContainer.Poster.LocalFilePath) OrElse Not Master.eSettings.TVSeasonPosterKeepExisting Then
                            Images.GetPreferredTVSeasonPoster(SearchResultsContainer.SeasonPosters, defImg, sContainer.Season)
                        End If
                    End If

                    If defImg IsNot Nothing Then
                        sSeason.ImagesContainer.Poster = defImg
                        sContainer.Poster = defImg
                    Else
                        sContainer.Poster = sSeason.ImagesContainer.Poster
                    End If
                Else
                    sContainer.Poster = sSeason.ImagesContainer.Poster
                End If

                DefaultSeasonImagesContainer.Add(sContainer)
            Next
        End If
    End Sub
    ''' <summary>
    ''' Determines the <c>ImageCodecInfo</c> from a given <c>ImageFormat</c>
    ''' </summary>
    ''' <param name="Format"><c>ImageFormat</c> to query</param>
    ''' <returns><c>ImageCodecInfo</c> that matches the image format supplied in the parameter</returns>
    ''' <remarks></remarks>
    Private Shared Function GetEncoderInfo(ByVal Format As ImageFormat) As ImageCodecInfo
        Dim Encoders() As ImageCodecInfo = ImageCodecInfo.GetImageEncoders()

        For i As Integer = 0 To Encoders.Count - 1
            If Encoders(i).FormatID = Format.Guid Then
                Return Encoders(i)
            End If
        Next

        Return Nothing
    End Function
    ''' <summary>
    ''' Select the single most preferred Banner image
    ''' </summary>
    ''' <param name="ImageList">Source <c>List</c> of <c>MediaContainers.Image</c> holding available banners</param>
    ''' <param name="imgResult">Single <c>MediaContainers.Image</c>, if preferred image was found</param>
    ''' <returns><c>True</c> if a preferred image was found, <c>False</c> otherwise</returns>
    ''' <remarks></remarks>
    Public Shared Function GetPreferredMovieBanner(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If ImageList.Count = 0 Then Return False
        imgResult = Nothing

        If Master.eSettings.MovieBannerPrefSize = Enums.MovieBannerSize.Any Then
            imgResult = ImageList.First
        End If

        If imgResult Is Nothing Then
            imgResult = ImageList.Find(Function(f) f.MovieBannerSize = Master.eSettings.MovieBannerPrefSize)
        End If

        If imgResult Is Nothing AndAlso Not Master.eSettings.MovieBannerPrefSizeOnly AndAlso Not Master.eSettings.MovieBannerPrefSize = Enums.MovieBannerSize.Any Then
            imgResult = ImageList.First
        End If

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function
    ''' <summary>
    ''' Select the single most preferred Banner image
    ''' </summary>
    ''' <param name="ImageList">Source <c>List</c> of <c>MediaContainers.Image</c> holding available banners</param>
    ''' <param name="imgResult">Single <c>MediaContainers.Image</c>, if preferred image was found</param>
    ''' <returns><c>True</c> if a preferred image was found, <c>False</c> otherwise</returns>
    ''' <remarks></remarks>
    Public Shared Function GetPreferredMovieSetBanner(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If ImageList.Count = 0 Then Return False
        imgResult = Nothing

        If Master.eSettings.MovieSetBannerPrefSize = Enums.MovieBannerSize.Any Then
            imgResult = ImageList.First
        End If

        If imgResult Is Nothing Then
            imgResult = ImageList.Find(Function(f) f.MovieBannerSize = Master.eSettings.MovieSetBannerPrefSize)
        End If

        If imgResult Is Nothing AndAlso Not Master.eSettings.MovieSetBannerPrefSizeOnly AndAlso Not Master.eSettings.MovieSetBannerPrefSize = Enums.MovieBannerSize.Any Then
            imgResult = ImageList.First
        End If

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function
    ''' <summary>
    ''' Select the single most preferred Poster image
    ''' </summary>
    ''' <param name="ImageList">Source <c>List</c> of <c>MediaContainers.Image</c> holding available posters</param>
    ''' <param name="imgResult">Single <c>MediaContainers.Image</c>, if preferred image was found</param>
    ''' <returns><c>True</c> if a preferred image was found, <c>False</c> otherwise</returns>
    ''' <remarks></remarks>
    Public Shared Function GetPreferredMoviePoster(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If ImageList.Count = 0 Then Return False
        imgResult = Nothing

        If Master.eSettings.MoviePosterPrefSize = Enums.MoviePosterSize.Any Then
            imgResult = ImageList.First
        End If

        If imgResult Is Nothing Then
            imgResult = ImageList.Find(Function(f) f.MoviePosterSize = Master.eSettings.MoviePosterPrefSize)
        End If

        If imgResult Is Nothing AndAlso Not Master.eSettings.MoviePosterPrefSizeOnly AndAlso Not Master.eSettings.MoviePosterPrefSize = Enums.MoviePosterSize.Any Then
            imgResult = ImageList.First
        End If

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function
    ''' <summary>
    ''' Select the single most preferred Poster image
    ''' </summary>
    ''' <param name="ImageList">Source <c>List</c> of <c>MediaContainers.Image</c> holding available posters</param>
    ''' <param name="imgResult">Single <c>MediaContainers.Image</c>, if preferred image was found</param>
    ''' <returns><c>True</c> if a preferred image was found, <c>False</c> otherwise</returns>
    ''' <remarks></remarks>
    Public Shared Function GetPreferredMovieSetPoster(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If ImageList.Count = 0 Then Return False
        imgResult = Nothing

        If Master.eSettings.MovieSetPosterPrefSize = Enums.MoviePosterSize.Any Then
            imgResult = ImageList.First
        End If

        If imgResult Is Nothing Then
            imgResult = ImageList.Find(Function(f) f.MoviePosterSize = Master.eSettings.MovieSetPosterPrefSize)
        End If

        If imgResult Is Nothing AndAlso Not Master.eSettings.MovieSetPosterPrefSizeOnly AndAlso Not Master.eSettings.MovieSetPosterPrefSize = Enums.MoviePosterSize.Any Then
            imgResult = ImageList.First
        End If

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredMovieClearArt(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If ImageList.Count = 0 Then Return False

        imgResult = ImageList.First

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredMovieSetClearArt(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If ImageList.Count = 0 Then Return False

        imgResult = ImageList.First

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredMovieClearLogo(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If ImageList.Count = 0 Then Return False

        imgResult = ImageList.First

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredMovieSetClearLogo(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If ImageList.Count = 0 Then Return False

        imgResult = ImageList.First

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredMovieDiscArt(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If ImageList.Count = 0 Then Return False

        imgResult = ImageList.First

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function
    ''' <summary>
    ''' Fetch a list of preferred extraFanart
    ''' </summary>
    ''' <param name="ImageList">Source <c>List</c> of <c>MediaContainers.Image</c> holding available extraFanart</param>
    ''' <returns><c>List</c> of image URLs that fit the preferred thumb size</returns>
    ''' <remarks></remarks>
    Public Shared Function GetPreferredMovieExtrafanarts(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResultList As List(Of MediaContainers.Image), ByVal iLimit As Integer) As Boolean
        If ImageList.Count = 0 Then Return False

        If Master.eSettings.MovieExtrafanartsPrefSize = Enums.MovieFanartSize.Any Then
            For Each img As MediaContainers.Image In ImageList
                imgResultList.Add(img)
                iLimit -= 1
                If iLimit = 0 Then Exit For
            Next
        End If

        If (imgResultList Is Nothing OrElse imgResultList.Count < iLimit) AndAlso Master.eSettings.MovieExtrafanartsPrefSizeOnly Then
            For Each img As MediaContainers.Image In ImageList.Where(Function(f) f.MovieFanartSize = Master.eSettings.MovieExtrafanartsPrefSize)
                imgResultList.Add(img)
                iLimit -= 1
                If iLimit = 0 Then Exit For
            Next
        End If

        If (imgResultList Is Nothing OrElse imgResultList.Count < iLimit) AndAlso Not Master.eSettings.MovieExtrafanartsPrefSizeOnly AndAlso Not Master.eSettings.MovieExtrafanartsPrefSize = Enums.MovieFanartSize.Any Then
            For Each img As MediaContainers.Image In ImageList.Where(Function(f) Not String.IsNullOrEmpty(f.URLOriginal))
                imgResultList.Add(img)
                iLimit -= 1
                If iLimit = 0 Then Exit For
            Next
        End If



        'If Master.eSettings.MovieFanartPrefSize = Enums.MovieFanartSize.Any Then
        '    imgResult = ImageList.First
        'End If

        'If imgResult Is Nothing Then
        '    imgResult = ImageList.Find(Function(f) f.MovieFanartSize = Master.eSettings.MovieFanartPrefSize)
        'End If

        'If imgResult Is Nothing AndAlso Not Master.eSettings.MovieFanartPrefSizeOnly AndAlso Not Master.eSettings.MovieFanartPrefSize = Enums.MovieFanartSize.Any Then
        '    imgResult = ImageList.First
        'End If

        If imgResultList IsNot Nothing AndAlso imgResultList.Count > 0 Then
            Return True
        Else
            Return False
        End If
    End Function
    ''' <summary>
    ''' Fetch a list of preferred extrathumbs
    ''' </summary>
    ''' <param name="ImageList">Source <c>List</c> of <c>MediaContainers.Image</c> holding available extrathumbs</param>
    ''' <returns><c>List</c> of image URLs that fit the preferred thumb size</returns>
    ''' <remarks></remarks>
    Public Shared Function GetPreferredMovieExtrathumbs(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResultList As List(Of MediaContainers.Image), ByVal iLimit As Integer) As Boolean
        If ImageList.Count = 0 Then Return False

        If Master.eSettings.MovieExtrathumbsPrefSize = Enums.MovieFanartSize.Any Then
            For Each img As MediaContainers.Image In ImageList
                imgResultList.Add(img)
                iLimit -= 1
                If iLimit = 0 Then Exit For
            Next
        End If

        If (imgResultList Is Nothing OrElse imgResultList.Count < iLimit) AndAlso Master.eSettings.MovieExtrathumbsPrefSizeOnly Then
            For Each img As MediaContainers.Image In ImageList.Where(Function(f) f.MovieFanartSize = Master.eSettings.MovieExtrathumbsPrefSize)
                imgResultList.Add(img)
                iLimit -= 1
                If iLimit = 0 Then Exit For
            Next
        End If

        If (imgResultList Is Nothing OrElse imgResultList.Count < iLimit) AndAlso Not Master.eSettings.MovieExtrathumbsPrefSizeOnly AndAlso Not Master.eSettings.MovieExtrathumbsPrefSize = Enums.MovieFanartSize.Any Then
            For Each img As MediaContainers.Image In ImageList.Where(Function(f) Not String.IsNullOrEmpty(f.URLOriginal))
                imgResultList.Add(img)
                iLimit -= 1
                If iLimit = 0 Then Exit For
            Next
        End If



        'If Master.eSettings.MovieFanartPrefSize = Enums.MovieFanartSize.Any Then
        '    imgResult = ImageList.First
        'End If

        'If imgResult Is Nothing Then
        '    imgResult = ImageList.Find(Function(f) f.MovieFanartSize = Master.eSettings.MovieFanartPrefSize)
        'End If

        'If imgResult Is Nothing AndAlso Not Master.eSettings.MovieFanartPrefSizeOnly AndAlso Not Master.eSettings.MovieFanartPrefSize = Enums.MovieFanartSize.Any Then
        '    imgResult = ImageList.First
        'End If

        If imgResultList IsNot Nothing AndAlso imgResultList.Count > 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredMovieSetDiscArt(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If ImageList.Count = 0 Then Return False

        imgResult = ImageList.First

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredMovieLandscape(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If ImageList.Count = 0 Then Return False

        imgResult = ImageList.First

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredMovieSetLandscape(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If ImageList.Count = 0 Then Return False

        imgResult = ImageList.First

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function
    ''' <summary>
    ''' Select the single most preferred Fanart image
    ''' </summary>
    ''' <param name="ImageList">Source <c>List</c> of <c>MediaContainers.Image</c> holding available fanart</param>
    ''' <param name="imgResult">Single <c>MediaContainers.Image</c>, if preferred image was found</param>
    ''' <returns><c>True</c> if a preferred image was found, <c>False</c> otherwise</returns>
    ''' <remarks></remarks>
    Public Shared Function GetPreferredMovieFanart(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If ImageList.Count = 0 Then Return False
        imgResult = Nothing

        If Master.eSettings.MovieFanartPrefSize = Enums.MovieFanartSize.Any Then
            imgResult = ImageList.First
        End If

        If imgResult Is Nothing Then
            imgResult = ImageList.Find(Function(f) f.MovieFanartSize = Master.eSettings.MovieFanartPrefSize)
        End If

        If imgResult Is Nothing AndAlso Not Master.eSettings.MovieFanartPrefSizeOnly AndAlso Not Master.eSettings.MovieFanartPrefSize = Enums.MovieFanartSize.Any Then
            imgResult = ImageList.First
        End If

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function
    ''' <summary>
    ''' Select the single most preferred Fanart image
    ''' </summary>
    ''' <param name="ImageList">Source <c>List</c> of <c>MediaContainers.Image</c> holding available fanart</param>
    ''' <param name="imgResult">Single <c>MediaContainers.Image</c>, if preferred image was found</param>
    ''' <returns><c>True</c> if a preferred image was found, <c>False</c> otherwise</returns>
    ''' <remarks></remarks>
    Public Shared Function GetPreferredMovieSetFanart(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If ImageList.Count = 0 Then Return False
        imgResult = Nothing

        If Master.eSettings.MovieSetFanartPrefSize = Enums.MovieFanartSize.Any Then
            imgResult = ImageList.First
        End If

        If imgResult Is Nothing Then
            imgResult = ImageList.Find(Function(f) f.MovieFanartSize = Master.eSettings.MovieSetFanartPrefSize)
        End If

        If imgResult Is Nothing AndAlso Not Master.eSettings.MovieSetFanartPrefSizeOnly AndAlso Not Master.eSettings.MovieSetFanartPrefSize = Enums.MovieFanartSize.Any Then
            imgResult = ImageList.First
        End If

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredTVAllSeasonsBanner(ByRef SeasonImageList As List(Of MediaContainers.Image), ByRef ShowImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If SeasonImageList.Count = 0 AndAlso ShowImageList.Count = 0 Then Return False

        If Not SeasonImageList.Count = 0 Then
            If Master.eSettings.TVAllSeasonsBannerPrefSize = Enums.TVBannerSize.Any Then
                imgResult = SeasonImageList.Find(Function(f) f.Season = 999)
            End If

            If imgResult Is Nothing Then
                imgResult = SeasonImageList.Find(Function(f) f.TVBannerSize = Master.eSettings.TVAllSeasonsBannerPrefSize AndAlso f.Season = 999)
            End If

            If imgResult Is Nothing AndAlso Not Master.eSettings.TVAllSeasonsBannerPrefSizeOnly AndAlso Not Master.eSettings.TVAllSeasonsBannerPrefSize = Enums.TVBannerSize.Any Then
                imgResult = SeasonImageList.Find(Function(f) f.Season = 999)
            End If
        End If

        If Not ShowImageList.Count = 0 Then
            If imgResult Is Nothing AndAlso Master.eSettings.TVAllSeasonsBannerPrefSize = Enums.TVBannerSize.Any Then
                imgResult = ShowImageList.First
            End If

            If imgResult Is Nothing Then
                imgResult = ShowImageList.Find(Function(f) f.TVBannerSize = Master.eSettings.TVAllSeasonsBannerPrefSize)
            End If

            If imgResult Is Nothing AndAlso Not Master.eSettings.TVAllSeasonsBannerPrefSizeOnly AndAlso Not Master.eSettings.TVAllSeasonsBannerPrefSize = Enums.TVBannerSize.Any Then
                imgResult = ShowImageList.First
            End If
        End If

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredTVAllSeasonsFanart(ByRef SeasonImageList As List(Of MediaContainers.Image), ByRef ShowImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If SeasonImageList.Count = 0 AndAlso ShowImageList.Count = 0 Then Return False


        If Not SeasonImageList.Count = 0 Then
            If Master.eSettings.TVAllSeasonsFanartPrefSize = Enums.TVPosterSize.Any Then
                imgResult = SeasonImageList.Find(Function(f) f.Season = 999)
            End If

            If imgResult Is Nothing Then
                imgResult = SeasonImageList.Find(Function(f) f.TVFanartSize = Master.eSettings.TVAllSeasonsFanartPrefSize AndAlso f.Season = 999)
            End If

            If imgResult Is Nothing AndAlso Not Master.eSettings.TVAllSeasonsFanartPrefSizeOnly AndAlso Not Master.eSettings.TVAllSeasonsFanartPrefSize = Enums.TVFanartSize.Any Then
                imgResult = SeasonImageList.Find(Function(f) f.Season = 999)
            End If
        End If

        If Not ShowImageList.Count = 0 Then
            If imgResult Is Nothing AndAlso Master.eSettings.TVAllSeasonsFanartPrefSize = Enums.TVPosterSize.Any Then
                imgResult = ShowImageList.First
            End If

            If imgResult Is Nothing Then
                imgResult = ShowImageList.Find(Function(f) f.TVFanartSize = Master.eSettings.TVAllSeasonsFanartPrefSize)
            End If

            If imgResult Is Nothing AndAlso Not Master.eSettings.TVAllSeasonsFanartPrefSizeOnly AndAlso Not Master.eSettings.TVAllSeasonsFanartPrefSize = Enums.TVFanartSize.Any Then
                imgResult = ShowImageList.First
            End If
        End If

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredTVAllSeasonsLandscape(ByRef SeasonImageList As List(Of MediaContainers.Image), ByRef ShowImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If SeasonImageList.Count = 0 AndAlso ShowImageList.Count = 0 Then Return False

        imgResult = SeasonImageList.Find(Function(f) f.Season = 999)

        If imgResult Is Nothing Then
            imgResult = ShowImageList.First
        End If

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredTVAllSeasonsPoster(ByRef SeasonImageList As List(Of MediaContainers.Image), ByRef ShowImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If SeasonImageList.Count = 0 AndAlso ShowImageList.Count = 0 Then Return False

        If Not SeasonImageList.Count = 0 Then
            If Master.eSettings.TVAllSeasonsPosterPrefSize = Enums.TVPosterSize.Any Then
                imgResult = SeasonImageList.Find(Function(f) f.Season = 999)
            End If

            If imgResult Is Nothing Then
                imgResult = SeasonImageList.Find(Function(f) f.TVPosterSize = Master.eSettings.TVAllSeasonsPosterPrefSize AndAlso f.Season = 999)
            End If

            If imgResult Is Nothing AndAlso Not Master.eSettings.TVAllSeasonsPosterPrefSizeOnly AndAlso Not Master.eSettings.TVAllSeasonsPosterPrefSize = Enums.TVPosterSize.Any Then
                imgResult = SeasonImageList.Find(Function(f) f.Season = 999)
            End If
        End If

        If Not ShowImageList.Count = 0 Then
            If imgResult Is Nothing AndAlso Master.eSettings.TVAllSeasonsPosterPrefSize = Enums.TVPosterSize.Any Then
                imgResult = ShowImageList.First
            End If

            If imgResult Is Nothing Then
                imgResult = ShowImageList.Find(Function(f) f.TVPosterSize = Master.eSettings.TVAllSeasonsPosterPrefSize)
            End If

            If imgResult Is Nothing AndAlso Not Master.eSettings.TVAllSeasonsPosterPrefSizeOnly AndAlso Not Master.eSettings.TVAllSeasonsPosterPrefSize = Enums.TVPosterSize.Any Then
                imgResult = ShowImageList.First
            End If
        End If

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredTVEpisodeFanart(ByRef EpisodeImageList As List(Of MediaContainers.Image), ByRef ShowImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image, ByVal iSeason As Integer, ByVal iEpisode As Integer) As Boolean
        If EpisodeImageList.Count = 0 AndAlso ShowImageList.Count = 0 Then Return False

        If Not EpisodeImageList.Count = 0 Then
            If Master.eSettings.TVEpisodeFanartPrefSize = Enums.TVFanartSize.Any Then
                imgResult = EpisodeImageList.Find(Function(f) f.Episode = iEpisode AndAlso f.Season = iSeason)
            End If

            If imgResult Is Nothing Then
                imgResult = EpisodeImageList.Find(Function(f) f.TVFanartSize = Master.eSettings.TVEpisodeFanartPrefSize AndAlso f.Episode = iEpisode AndAlso f.Season = 999)
            End If

            If imgResult Is Nothing AndAlso Not Master.eSettings.TVEpisodeFanartPrefSizeOnly AndAlso Not Master.eSettings.TVEpisodeFanartPrefSize = Enums.TVFanartSize.Any Then
                imgResult = EpisodeImageList.Find(Function(f) f.Episode = iEpisode AndAlso f.Season = 999)
            End If
        End If

        If Not ShowImageList.Count = 0 Then
            If imgResult Is Nothing AndAlso Master.eSettings.TVEpisodeFanartPrefSize = Enums.TVFanartSize.Any Then
                imgResult = ShowImageList.First
            End If

            If imgResult Is Nothing Then
                imgResult = ShowImageList.Find(Function(f) f.TVFanartSize = Master.eSettings.TVEpisodeFanartPrefSize)
            End If

            If imgResult Is Nothing AndAlso Not Master.eSettings.TVEpisodeFanartPrefSizeOnly AndAlso Not Master.eSettings.TVEpisodeFanartPrefSize = Enums.TVFanartSize.Any Then
                imgResult = ShowImageList.First
            End If
        End If

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredTVEpisodePoster(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image, ByVal iSeason As Integer, ByVal iEpisode As Integer) As Boolean
        If ImageList.Count = 0 Then Return False

        If Master.eSettings.TVEpisodePosterPrefSize = Enums.TVEpisodePosterSize.Any Then
            imgResult = ImageList.Find(Function(f) f.Episode = iEpisode AndAlso f.Season = iSeason)
        End If

        If imgResult Is Nothing Then
            imgResult = ImageList.Find(Function(f) f.TVEpisodePosterSize = Master.eSettings.TVEpisodePosterPrefSize AndAlso f.Episode = iEpisode AndAlso f.Season = iSeason)
        End If

        If imgResult Is Nothing AndAlso Not Master.eSettings.TVEpisodePosterPrefSizeOnly AndAlso Not Master.eSettings.TVEpisodePosterPrefSize = Enums.TVEpisodePosterSize.Any Then
            imgResult = ImageList.Find(Function(f) f.Episode = iEpisode AndAlso f.Season = iSeason)
        End If

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function
    ''' <summary>
    ''' Select the single most preferred Banner image
    ''' </summary>
    ''' <param name="ImageList">Source <c>List</c> of <c>MediaContainers.Image</c> holding available banners</param>
    ''' <param name="imgResult">Single <c>MediaContainers.Image</c>, if preferred image was found</param>
    ''' <returns><c>True</c> if a preferred image was found, <c>False</c> otherwise</returns>
    ''' <remarks></remarks>
    Public Shared Function GetPreferredTVSeasonBanner(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image, ByVal iSeason As Integer) As Boolean
        If ImageList.Count = 0 Then Return False

        If Master.eSettings.TVSeasonBannerPrefSize = Enums.TVBannerSize.Any Then
            imgResult = ImageList.Find(Function(f) f.Season = iSeason)
        End If

        If imgResult Is Nothing Then
            imgResult = ImageList.Find(Function(f) f.TVBannerSize = Master.eSettings.TVSeasonBannerPrefSize AndAlso f.Season = iSeason)
        End If

        If imgResult Is Nothing AndAlso Not Master.eSettings.TVSeasonBannerPrefSizeOnly AndAlso Not Master.eSettings.TVSeasonBannerPrefSize = Enums.TVBannerSize.Any Then
            imgResult = ImageList.Find(Function(f) f.Season = iSeason)
        End If

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredTVSeasonFanart(ByRef SeasonImageList As List(Of MediaContainers.Image), ByRef ShowImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image, ByVal iSeason As Integer) As Boolean
        If SeasonImageList.Count = 0 AndAlso ShowImageList.Count = 0 Then Return False

        If Not SeasonImageList.Count = 0 Then
            If Master.eSettings.TVSeasonFanartPrefSize = Enums.TVFanartSize.Any Then
                imgResult = SeasonImageList.Find(Function(f) f.Season = iSeason)
            End If

            If imgResult Is Nothing Then
                imgResult = SeasonImageList.Find(Function(f) f.TVFanartSize = Master.eSettings.TVSeasonFanartPrefSize AndAlso f.Season = iSeason)
            End If

            If imgResult Is Nothing AndAlso Not Master.eSettings.TVSeasonFanartPrefSizeOnly AndAlso Not Master.eSettings.TVSeasonFanartPrefSize = Enums.TVFanartSize.Any Then
                imgResult = SeasonImageList.Find(Function(f) f.Season = iSeason)
            End If
        End If

        If Not ShowImageList.Count = 0 Then
            If imgResult Is Nothing AndAlso Master.eSettings.TVSeasonFanartPrefSize = Enums.TVFanartSize.Any Then
                imgResult = ShowImageList.First
            End If

            If imgResult Is Nothing Then
                imgResult = ShowImageList.Find(Function(f) f.TVFanartSize = Master.eSettings.TVSeasonFanartPrefSize)
            End If

            If imgResult Is Nothing AndAlso Not Master.eSettings.TVSeasonFanartPrefSizeOnly AndAlso Not Master.eSettings.TVSeasonFanartPrefSize = Enums.TVFanartSize.Any Then
                imgResult = ShowImageList.First
            End If
        End If

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredTVSeasonLandscape(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image, ByVal iSeason As Integer) As Boolean
        If ImageList.Count = 0 Then Return False

        imgResult = ImageList.Find(Function(f) f.Season = iSeason)

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredTVSeasonPoster(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image, ByVal iSeason As Integer) As Boolean
        If ImageList.Count = 0 Then Return False

        If Master.eSettings.TVSeasonPosterPrefSize = Enums.TVSeasonPosterSize.Any Then
            imgResult = ImageList.Find(Function(f) f.Season = iSeason)
        End If

        If imgResult Is Nothing Then
            imgResult = ImageList.Find(Function(f) f.TVSeasonPosterSize = Master.eSettings.TVSeasonPosterPrefSize AndAlso f.Season = iSeason)
        End If

        If imgResult Is Nothing AndAlso Not Master.eSettings.TVSeasonPosterPrefSizeOnly AndAlso Not Master.eSettings.TVSeasonPosterPrefSize = Enums.TVSeasonPosterSize.Any Then
            imgResult = ImageList.Find(Function(f) f.Season = iSeason)
        End If

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function
    ''' <summary>
    ''' Select the single most preferred Banner image
    ''' </summary>
    ''' <param name="ImageList">Source <c>List</c> of <c>MediaContainers.Image</c> holding available banners</param>
    ''' <param name="imgResult">Single <c>MediaContainers.Image</c>, if preferred image was found</param>
    ''' <returns><c>True</c> if a preferred image was found, <c>False</c> otherwise</returns>
    ''' <remarks></remarks>
    Public Shared Function GetPreferredTVShowBanner(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If ImageList.Count = 0 Then Return False

        If Master.eSettings.TVShowBannerPrefSize = Enums.TVBannerSize.Any Then
            imgResult = ImageList.First
        End If

        If imgResult Is Nothing Then
            imgResult = ImageList.Find(Function(f) f.TVBannerSize = Master.eSettings.TVShowBannerPrefSize)
        End If

        If imgResult Is Nothing AndAlso Not Master.eSettings.TVShowBannerPrefSizeOnly AndAlso Not Master.eSettings.TVShowBannerPrefSize = Enums.TVBannerSize.Any Then
            imgResult = ImageList.First
        End If

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredTVShowCharacterArt(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If ImageList.Count = 0 Then Return False

        imgResult = ImageList.First

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredTVShowClearArt(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If ImageList.Count = 0 Then Return False

        imgResult = ImageList.First

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredTVShowClearLogo(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If ImageList.Count = 0 Then Return False

        imgResult = ImageList.First

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredTVShowExtrafanarts(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResultList As List(Of MediaContainers.Image), ByVal iLimit As Integer) As Boolean
        If ImageList.Count = 0 Then Return False

        If Master.eSettings.TVShowExtrafanartsPrefSize = Enums.TVFanartSize.Any Then
            For Each img As MediaContainers.Image In ImageList
                imgResultList.Add(img)
                iLimit -= 1
                If iLimit = 0 Then Exit For
            Next
        End If

        If (imgResultList Is Nothing OrElse imgResultList.Count < iLimit) AndAlso Master.eSettings.TVShowExtrafanartsPrefSizeOnly Then
            For Each img As MediaContainers.Image In ImageList.Where(Function(f) f.TVFanartSize = Master.eSettings.TVShowExtrafanartsPrefSize)
                imgResultList.Add(img)
                iLimit -= 1
                If iLimit = 0 Then Exit For
            Next
        End If

        If (imgResultList Is Nothing OrElse imgResultList.Count < iLimit) AndAlso Not Master.eSettings.TVShowExtrafanartsPrefSizeOnly AndAlso Not Master.eSettings.TVShowExtrafanartsPrefSize = Enums.TVFanartSize.Any Then
            For Each img As MediaContainers.Image In ImageList.Where(Function(f) Not String.IsNullOrEmpty(f.URLOriginal))
                imgResultList.Add(img)
                iLimit -= 1
                If iLimit = 0 Then Exit For
            Next
        End If



        'If Master.eSettings.MovieFanartPrefSize = Enums.MovieFanartSize.Any Then
        '    imgResult = ImageList.First
        'End If

        'If imgResult Is Nothing Then
        '    imgResult = ImageList.Find(Function(f) f.MovieFanartSize = Master.eSettings.MovieFanartPrefSize)
        'End If

        'If imgResult Is Nothing AndAlso Not Master.eSettings.MovieFanartPrefSizeOnly AndAlso Not Master.eSettings.MovieFanartPrefSize = Enums.MovieFanartSize.Any Then
        '    imgResult = ImageList.First
        'End If

        If imgResultList IsNot Nothing AndAlso imgResultList.Count > 0 Then
            Return True
        Else
            Return False
        End If
    End Function
    ''' <summary>
    ''' Select the single most preferred Fanart image
    ''' </summary>
    ''' <param name="ImageList">Source <c>List</c> of <c>MediaContainers.Image</c> holding available banners</param>
    ''' <param name="imgResult">Single <c>MediaContainers.Image</c>, if preferred image was found</param>
    ''' <returns><c>True</c> if a preferred image was found, <c>False</c> otherwise</returns>
    ''' <remarks></remarks>
    Public Shared Function GetPreferredTVShowFanart(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If ImageList.Count = 0 Then Return False

        If Master.eSettings.TVShowFanartPrefSize = Enums.TVFanartSize.Any Then
            imgResult = ImageList.First
        End If

        If imgResult Is Nothing Then
            imgResult = ImageList.Find(Function(f) f.TVFanartSize = Master.eSettings.TVShowFanartPrefSize)
        End If

        If imgResult Is Nothing AndAlso Not Master.eSettings.TVShowFanartPrefSizeOnly AndAlso Not Master.eSettings.TVShowFanartPrefSize = Enums.TVFanartSize.Any Then
            imgResult = ImageList.First
        End If

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function GetPreferredTVShowLandscape(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If ImageList.Count = 0 Then Return False

        imgResult = ImageList.First

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function
    ''' <summary>
    ''' Select the single most preferred Poster image
    ''' </summary>
    ''' <param name="ImageList">Source <c>List</c> of <c>MediaContainers.Image</c> holding available banners</param>
    ''' <param name="imgResult">Single <c>MediaContainers.Image</c>, if preferred image was found</param>
    ''' <returns><c>True</c> if a preferred image was found, <c>False</c> otherwise</returns>
    ''' <remarks></remarks>
    Public Shared Function GetPreferredTVShowPoster(ByRef ImageList As List(Of MediaContainers.Image), ByRef imgResult As MediaContainers.Image) As Boolean
        If ImageList.Count = 0 Then Return False

        If Master.eSettings.TVShowPosterPrefSize = Enums.TVPosterSize.Any Then
            imgResult = ImageList.First
        End If

        If imgResult Is Nothing Then
            imgResult = ImageList.Find(Function(f) f.TVPosterSize = Master.eSettings.TVShowPosterPrefSize)
        End If

        If imgResult Is Nothing AndAlso Not Master.eSettings.TVShowPosterPrefSizeOnly AndAlso Not Master.eSettings.TVShowPosterPrefSize = Enums.TVPosterSize.Any Then
            imgResult = ImageList.First
        End If

        If imgResult IsNot Nothing Then
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Remove duplicate images from a given list of images
    ''' </summary>
    ''' <param name="ImageList">Source <c>List</c> of <c>MediaContainers.Image</c> holding available extraFanart</param>
    ''' <param name="CurrentImage">Optional: Current image of video file (i.e. fanart.jpg)</param>
    ''' <returns>true: no errors, false: no images to compare</returns>
    ''' <remarks>
    ''' 2015/09/23 Cocotus - First implementation
    ''' Used to avoid duplicate images
    ''' </remarks>
    Public Shared Function RemoveDuplicateImages(ByRef ImageList As List(Of MediaContainers.Image), Optional ByVal CurrentImage As MediaContainers.Image = Nothing, Optional ByVal MatchTolerance As Integer = 5) As Boolean
        If ImageList.Count = 0 Then Return False
        Dim lstScrapedImages As New List(Of Images)
        Dim lstSimilarImages As New List(Of Tuple(Of Integer, Integer))
        Dim currentimagesimilarity As Integer = 0

        'To compare images for similarity we need to load them
        'Checking for similarity means we need to load images to compare the content! -> Need to download ´the scraped image
        'If the images aren't avalaible in cache or stored local, download them
        For Each singleImage In ImageList
            Dim ScrapedImage As New Images
            If File.Exists(singleImage.LocalFilePath) Then
                ScrapedImage.FromFile(singleImage.LocalFilePath, LoadBitmap:=True)
            ElseIf Master.eSettings.MovieImagesCacheEnabled AndAlso File.Exists(singleImage.CacheThumbPath) Then
                ScrapedImage.FromFile(singleImage.CacheThumbPath, LoadBitmap:=True)
                'need to download
            ElseIf Not singleImage.URLThumb Is Nothing AndAlso Not String.IsNullOrEmpty(singleImage.URLThumb) Then
                ScrapedImage.FromWeb(singleImage.URLThumb, LoadBitmap:=True)
            ElseIf Not singleImage.URLOriginal Is Nothing AndAlso Not String.IsNullOrEmpty(singleImage.URLOriginal) Then
                ScrapedImage.FromWeb(singleImage.URLOriginal, LoadBitmap:=True)
            End If
            lstScrapedImages.Add(ScrapedImage)
        Next

        '1. Step (Optional): Remove any image  scraped imagelist which is identical to (current) image (i.e. fanart) of movie!
        If Not CurrentImage Is Nothing AndAlso File.Exists(CurrentImage.LocalFilePath) Then
            For i = 0 To lstScrapedImages.Count - 1
                Dim referenceitem = lstScrapedImages(i)
                'stores index of image in imagelist with calculacted Similarityvalue
                'invalid comparison (one of images is nothing?!)
                If referenceitem Is Nothing OrElse referenceitem.Image Is Nothing Then
                    currentimagesimilarity = 99
                    'First image is Nothing -> no need to compare anything!
                    logger.Warn("[RemoveDuplicateImages] Image is nothing. Can't compare images! Image at Index: " & i)
                Else
                    currentimagesimilarity = ImageUtils.ImageComparison.GetSimilarity(referenceitem.Image, CurrentImage.LocalFilePath, ImageUtils.ImageComparison.Algorithm.AverageHash)
                    'Combine with pHash?!
                    'If currentimagesimilarity > MatchTolerance Then
                    '    currentimagesimilarity = ImageUtils.ImageComparison.GetSimilarity(referenceitem.Image, CurrentImage.LocalFilePath, ImageUtils.ImageComparison.Algorithm.PHash)
                    'End If
                End If
                Dim newSimilarityvalue = Tuple.Create(i, currentimagesimilarity)
                lstSimilarImages.Add(newSimilarityvalue)
            Next
        End If

        '2. Step: Calculate similarity for each image combination in imagelist - basically we compare each image in lstScrapedImages to find out which images are identical to each other
        For i = 0 To lstScrapedImages.Count - 1
            Dim referenceitem = lstScrapedImages(i)
            For j = i + 1 To lstScrapedImages.Count - 1

                'check for invalid images (one of images is nothing?!)
                If referenceitem Is Nothing OrElse referenceitem.Image Is Nothing Then
                    currentimagesimilarity = 99
                    'First image is nothing -> no need to compare anything!
                    logger.Warn("[RemoveDuplicateImages] Image 1 is nothing. Can't compare images! Image at Index: " & i)
                ElseIf lstScrapedImages.Item(j) Is Nothing OrElse lstScrapedImages.Item(j).Image Is Nothing Then
                    currentimagesimilarity = 99
                    'Second image is nothing -> no need to compare anything!
                    logger.Warn("[RemoveDuplicateImages] Image 2 is nothing. Can't compare images! Image at Index: " & j)
                Else
                    'stores index of image in imagelist with calculacted Similarityvalue
                    currentimagesimilarity = ImageUtils.ImageComparison.GetSimilarity(referenceitem.Image, lstScrapedImages.Item(j).Image, ImageUtils.ImageComparison.Algorithm.AverageHash)
                    'Combine with pHash?!
                    'If currentimagesimilarity > MatchTolerance Then
                    '    currentimagesimilarity = ImageUtils.ImageComparison.GetSimilarity(referenceitem.Image, lstScrapedImages.Item(j).Image, ImageUtils.ImageComparison.Algorithm.PHash)
                    'End If
                End If
                Dim newSimilarityvalue = Tuple.Create(i, currentimagesimilarity)
                lstSimilarImages.Add(newSimilarityvalue)
            Next
        Next

        'Sort Similaritylist by similarityvalue
        lstSimilarImages.Sort(Function(x, y) y.Item2.CompareTo(x.Item2))
        lstSimilarImages.Reverse()

        logger.Trace("[RemoveDuplicateImages] Ignore all images with MatchTolerance less/equal then : " & MatchTolerance & "...")
        'logging used for debugging in tests
        'For Each calculatedimage In lstSimilarImages
        '    If calculatedimage.Item1 <= MatchTolerance Then
        '        logger.Trace("[RemoveDuplicateImages] Ignore image with MatchTolerance: " & calculatedimage.Item2 & " at Index: " & calculatedimage.Item1 & " Name: " & ImageList.Item(calculatedimage.Item1).URLOriginal)
        '    Else
        '        logger.Trace("[RemoveDuplicateImages] Keep image with MatchTolerance: " & calculatedimage.Item2 & " at Index: " & calculatedimage.Item1 & " Name: " & ImageList.Item(calculatedimage.Item1).URLOriginal)
        '    End If
        'Next

        '3. Step: finally remove duplicate image at index calculated above
        For i = ImageList.Count - 1 To 0 Step -1
            If lstSimilarImages.Any(Function(c) c.Item1 = i AndAlso c.Item2 <= MatchTolerance) Then
                logger.Trace("[RemoveDuplicateImages] Ignore image: " & ImageList.Item(i).URLOriginal & " ...")
                ImageList.RemoveAt(i)
            End If
        Next

        Return True

    End Function
#End Region 'Methods

End Class