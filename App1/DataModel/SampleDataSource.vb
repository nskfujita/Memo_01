﻿Namespace Data
    ' このファイルで定義されるデータ モデルは、メンバーが追加、削除、または変更されるときの通知をサポートする、
    ' しています。選択されたプロパティ名は標準の項目テンプレートのデータ バインディングと一致しています。
    '
    ' アプリケーションでは、このモデルを開始位置として使用してこのモデル上でビルドするか、完全に破棄して
    ' 必要に応じて置き換えることがあります。このモデルを使用する場合、アプリケーションが最初に起動されるときに、
    ' App.xaml の分離コードでデータ読み込みタスクを開始することで、アプリの応答性を
    ' 向上できる可能性があります。


    ''' <summary>
    ''' 汎用項目データ モデル。
    ''' </summary>
    Public Class SampleDataItem
        Private Shared _baseUri As New Uri("ms-appx:///")

        Public Sub New(uniqueId As String, title As String, subtitle As String, imagePath As String, description As String, content As String)
            Me.UniqueId = uniqueId
            Me.Title = title
            Me.Subtitle = subtitle
            Me.Description = description
            Me.ImagePath = imagePath
            Me.Content = content
        End Sub

        Private _uniqueId As String
        Public Property UniqueId As String
            Get
                Return _uniqueId
            End Get
            Private Set(value As String)
                _uniqueId = value
            End Set
        End Property


        Private _title As String
        Public Property Title As String
            Get
                Return _title
            End Get
            Private Set(value As String)
                _title = value
            End Set
        End Property


        Private _subtitle As String
        Public Property Subtitle As String
            Get
                Return _subtitle
            End Get
            Private Set(value As String)
                _subtitle = value
            End Set
        End Property

        Private _description As String
        Public Property Description As String
            Get
                Return _description
            End Get
            Private Set(value As String)
                _description = value
            End Set
        End Property

        Private _imagePath As String
        Public Property ImagePath As String
            Get
                Return _imagePath
            End Get
            Private Set(value As String)
                _imagePath = value
            End Set
        End Property

        Private _content As String
        Public Property Content As String
            Get
                Return _content
            End Get
            Private Set(value As String)
                _content = value
            End Set
        End Property

        Private _image As ImageSource = Nothing
        Public ReadOnly Property Image As ImageSource
            Get
                If Me._image Is Nothing AndAlso Me._imagePath IsNot Nothing Then
                    Me._image = New BitmapImage(New Uri(SampleDataItem._baseUri, Me._imagePath))
                End If
                Return Me._image
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Me.Title
        End Function
    End Class

    ''' <summary>
    ''' 汎用グループ データ モデル。
    ''' </summary>
    Public Class SampleDataGroup
        Private Shared _baseUri As New Uri("ms-appx:///")

        Public Sub New(uniqueId As String, title As String, subtitle As String, imagePath As String, description As String)
            Me.UniqueId = uniqueId
            Me.Title = title
            Me.Subtitle = subtitle
            Me.Description = description
            Me.ImagePath = imagePath
            Me.Items = New ObservableCollection(Of SampleDataItem)()
        End Sub

        Private _uniqueId As String
        Public Property UniqueId As String
            Get
                Return _uniqueId
            End Get
            Private Set(value As String)
                _uniqueId = value
            End Set
        End Property

        Private _title As String
        Public Property Title As String
            Get
                Return _title
            End Get
            Private Set(value As String)
                _title = value
            End Set
        End Property

        Private _subtitle As String
        Public Property Subtitle As String
            Get
                Return _subtitle
            End Get
            Private Set(value As String)
                _subtitle = value
            End Set
        End Property

        Private _description As String
        Public Property Description As String
            Get
                Return _description
            End Get
            Private Set(value As String)
                _description = value
            End Set
        End Property

        Private _imagePath As String
        Public Property ImagePath As String
            Get
                Return _imagePath
            End Get
            Private Set(value As String)
                _imagePath = value
            End Set
        End Property

        Private _items As ObservableCollection(Of SampleDataItem)
        Public Property Items As ObservableCollection(Of SampleDataItem)
            Get
                Return _items
            End Get
            Private Set(value As ObservableCollection(Of SampleDataItem))
                _items = value
            End Set
        End Property


        Private _image As ImageSource = Nothing
        Public ReadOnly Property Image As ImageSource
            Get
                If Me._image Is Nothing AndAlso Me._imagePath IsNot Nothing Then
                    Me._image = New BitmapImage(New Uri(SampleDataGroup._baseUri, Me._imagePath))
                End If
                Return Me._image
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Me.Title
        End Function
    End Class

    ''' <summary>
    ''' 静的な json ファイルから読み取られたコンテンツを使用して、グループおよびアイテムのコレクションを作成します。
    ''' 
    ''' SampleDataSource は、プロジェクト内の静的な json ファイルから読み取られたデータを使用して
    ''' 初期化します。これにより、設計時と実行時の両方のサンプル データが得られます。
    ''' </summary>
    Public NotInheritable Class SampleDataSource
        Private Shared _sampleDataSource As New SampleDataSource()

        Private _groups As New ObservableCollection(Of SampleDataGroup)()
        Public ReadOnly Property Groups As ObservableCollection(Of SampleDataGroup)
            Get
                Return Me._groups
            End Get
        End Property

        Public Shared Async Function GetGroupsAsync() As Task(Of IEnumerable(Of SampleDataGroup))
            Await _sampleDataSource.GetSampleDataAsync()
            Return _sampleDataSource.Groups
        End Function

        Public Shared Async Function GetGroupAsync(uniqueId As String) As Task(Of SampleDataGroup)
            Await _sampleDataSource.GetSampleDataAsync()
            ' サイズの小さいデータ セットでは単純な一方向の検索を実行できます
            Dim matches As IEnumerable(Of SampleDataGroup) = _sampleDataSource.Groups.Where(Function(group) group.UniqueId.Equals(uniqueId))
            If matches.Count() = 1 Then Return matches.First()
            Return Nothing
        End Function

        Public Shared Async Function GetItemAsync(uniqueId As String) As Task(Of SampleDataItem)
            Await _sampleDataSource.GetSampleDataAsync()
            ' サイズの小さいデータ セットでは単純な一方向の検索を実行できます
            Dim matches As IEnumerable(Of SampleDataItem) = _sampleDataSource.Groups.SelectMany(Function(group) group.Items).Where(Function(item) item.UniqueId.Equals(uniqueId))
            If matches.Count() = 1 Then Return matches.First()
            Return Nothing
        End Function

        Private Async Function GetSampleDataAsync() As Task

            If Me._groups.Count <> 0 Then
                Return
            End If

            Dim dataUri As New Uri("ms-appx:///DataModel/SampleData.json")

            Dim file As StorageFile = Await StorageFile.GetFileFromApplicationUriAsync(dataUri)
            Dim jsonText As String = Await FileIO.ReadTextAsync(file)
            Dim jsonObject As JsonObject = jsonObject.Parse(jsonText)
            Dim jsonArray As JsonArray = jsonObject("Groups").GetArray()

            For Each groupValue As JsonValue In jsonArray
                Dim groupObject As JsonObject = groupValue.GetObject()
                Dim group As New SampleDataGroup(groupObject("UniqueId").GetString(), groupObject("Title").GetString(), groupObject("Subtitle").GetString(), groupObject("ImagePath").GetString(), groupObject("Description").GetString())

                For Each itemValue As JsonValue In groupObject("Items").GetArray()
                    Dim itemObject As JsonObject = itemValue.GetObject()
                    group.Items.Add(New SampleDataItem(itemObject("UniqueId").GetString(), itemObject("Title").GetString(), itemObject("Subtitle").GetString(), itemObject("ImagePath").GetString(), itemObject("Description").GetString(), itemObject("Content").GetString()))
                Next

                Me.Groups.Add(group)
            Next
        End Function
    End Class
End Namespace