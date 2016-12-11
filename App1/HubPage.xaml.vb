Imports App1.Common
Imports App1.Data

' ハブ アプリケーション テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=391641 を参照してください

Public NotInheritable Class HubPage
    Inherits Page

    Private WithEvents _navigationHelper As New NavigationHelper(Me)
    Private ReadOnly _defaultViewModel As New ObservableDictionary
    Private ReadOnly _resourceLoader As ResourceLoader = ResourceLoader.GetForCurrentView("Resources")

    ''' <summary>
    ''' グループ化された項目のコレクションが表示されるページです。
    ''' </summary>
    Public Sub New()
        InitializeComponent()

        ' ハブは縦向きでのみサポートされています
        DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait

        NavigationCacheMode = NavigationCacheMode.Required
    End Sub

    ''' <summary>
    ''' この <see cref="Page"/> に関連付けられた <see cref="NavigationHelper"/> を取得します。
    ''' </summary>
    Public ReadOnly Property NavigationHelper As NavigationHelper
        Get
            Return _navigationHelper
        End Get
    End Property

    ''' <summary>
    ''' この <see cref="Page"/> のビュー モデルを取得します。
    ''' これは厳密に型指定されたビュー モデルに変更できます。
    ''' </summary>
    Public ReadOnly Property DefaultViewModel As ObservableDictionary
        Get
            Return _defaultViewModel
        End Get
    End Property

    ''' <summary>
    ''' このページには、移動中に渡されるコンテンツを設定します。前のセッションからページを
    ''' 再作成する場合は、保存状態も指定されます。
    ''' </summary>
    ''' <param name="sender">
    ''' イベントのソース (通常、<see cref="NavigationHelper"/>)。
    ''' </param>
    ''' <param name="e">このページが最初に要求されたときに
    ''' <see cref="Frame.Navigate"/> に渡されたナビゲーション パラメーターと、
    ''' 前のセッションでこのページによって保存された状態のディクショナリを提供する
    ''' セッション。ページに初めてアクセスするとき、状態は null になります。</param>
    Private Async Sub NavigationHelper_LoadState(sender As Object, e As LoadStateEventArgs) Handles _navigationHelper.LoadState
        ' TODO: 対象となる問題領域に適したデータ モデルを作成し、サンプル データを置き換えます
        Dim sampleDataGroups As IEnumerable(Of SampleDataGroup) = Await SampleDataSource.GetGroupsAsync()
        DefaultViewModel("Groups") = sampleDataGroups
    End Sub

    ''' <summary>
    ''' アプリケーションが中断される場合、またはページがナビゲーション キャッシュから破棄される場合、
    ''' このページに関連付けられた状態を保存します。値は、
    ''' <see cref="SuspensionManager.SessionState"/> のシリアル化の要件に準拠する必要があります。
    ''' </summary>
    ''' <param name="sender">イベントのソース (通常、<see cref="NavigationHelper"/>)</param>
    ''' <param name="e">シリアル化可能な状態で作成される空のディクショナリを提供するイベント データ
    '''。</param>
    Private Sub NavigationHelper_SaveState(sender As Object, e As SaveStateEventArgs) Handles _navigationHelper.SaveState
        ' TODO: ページの一意の状態をここに保存します。
    End Sub

    ''' <summary>
    ''' <see cref="SectionPage"/> のクリックされたグループの詳細を表示します。
    ''' </summary>
    Private Sub GroupSection_ItemClick(sender As Object, e As ItemClickEventArgs)
        Dim groupId As String = DirectCast(e.ClickedItem, SampleDataGroup).UniqueId
        If Not Frame.Navigate(GetType(SectionPage), groupId) Then
            Throw New Exception(_resourceLoader.GetString("NavigationFailedExceptionMessage"))
        End If
    End Sub

    ''' <summary>
    ''' <see cref="ItemPage"/> でクリックされた項目の詳細を表示します
    ''' </summary>
    Private Sub ItemView_ItemClick(sender As Object, e As ItemClickEventArgs)
        Dim itemId As String = DirectCast(e.ClickedItem, SampleDataItem).UniqueId
        If Not Frame.Navigate(GetType(ItemPage), itemId) Then
            Throw New Exception(_resourceLoader.GetString("NavigationFailedExceptionMessage"))
        End If
    End Sub

#Region "NavigationHelper の登録"

    ''' <summary>
    ''' このセクションに示したメソッドは、NavigationHelper がページの
    ''' ナビゲーション メソッドに応答できるようにするためにのみ使用します。
    ''' <para>
    ''' ページ固有のロジックは、
    ''' <see cref="NavigationHelper.LoadState"/>
    ''' および <see cref="NavigationHelper.SaveState"/>。
    ''' LoadState メソッドでは、前のセッションで保存されたページの状態に加え、
    ''' ナビゲーション パラメーターを使用できます。
    ''' </para>
    ''' </summary>
    ''' <param name="e">このページにどのように到達したかを説明するイベント データ。</param>
    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
        _navigationHelper.OnNavigatedTo(e)
    End Sub

    Protected Overrides Sub OnNavigatedFrom(e As NavigationEventArgs)
        _navigationHelper.OnNavigatedFrom(e)
    End Sub

#End Region

End Class
