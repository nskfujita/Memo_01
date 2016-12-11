Namespace Common
    ''' <summary>
    ''' NavigationHelper は、ページ間のナビゲーションで使用されます。 NavigationManager のコマンドを使用して、
    ''' ナビゲーション操作を実行できるだけでなく、Windows や Windows Phone のハードウェア
    ''' の戻るボタンでナビゲーション操作を実行するための標準マウス ショートカットおよび
    ''' キーボード ショートカットを登録できます。さらに、SuspensionManger も統合されているため、
    ''' ページ間で移動するときのプロセス継続時間管理および状態管理も処理できます。
    ''' </summary>
    ''' <remarks>
    ''' <example>
    ''' NavigationHelper を利用するには、この 2 つの手順に従うか、
    ''' BasicPage などの BlankPage 以外のページ アイテム テンプレートを使用します。
    ''' 
    ''' 1) ページのコンストラクター内などの場所に NaivgationHelper インスタンスを作成し、
    '''     LoadState イベントと SaveState イベントに対するコールバックを
    '''     登録します。
    ''' <code>
    '''     Public NotInheritable Class MyPage
    '''         Inherits Page
    ''' 
    '''         Public Sub New()
    '''             InitializeComponent();
    '''             Me._navigationHelper = New Common.NavigationHelper(Me)
    '''             AddHandler Me._navigationHelper.LoadState, AddressOf NavigationHelper_LoadState
    '''             AddHandler Me._navigationHelper.SaveState, AddressOf NavigationHelper_SaveState
    '''         End Sub
    '''     
    '''     Private Sub NavigationHelper_LoadState(sender As Object, e As Common.LoadStateEventArgs)
    '''     End Sub
    ''' 
    '''     Private Sub NavigationHelper_SaveState(sender As Object, e As Common.SaveStateEventArgs)
    '''     End Sub
    ''' </code>
    ''' 
    ''' 2) ページがナビゲーションに追加されるたびに、そのページを登録して NavigationHelper を呼び出すには、
    '''     <see cref="Windows.UI.Xaml.Controls.Page.OnNavigatedTo"/> 
    '''     イベントと <see cref="Windows.UI.Xaml.Controls.Page.OnNavigatedFrom"/> イベントをオーバーライドします。
    ''' <code>
    '''     Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
    '''         _navigationHelper.OnNavigatedTo(e)
    '''     
    '''     Protected Overrides Sub OnNavigatedFrom(e As NavigationEventArgs)
    '''         _navigationHelper.OnNavigatedFrom(e)
    ''' </code>
    ''' </example>
    ''' </remarks>
    Public Class NavigationHelper
        Inherits DependencyObject
        Private Property Page() As Page
        Private ReadOnly Property Frame() As Frame
            Get
                Return Me.Page.Frame
            End Get
        End Property

        Public Sub New(page As Page)
            Me.Page = page

            ' このページがビジュアル ツリーの一部である場合、次の 2 つの変更を行います:
            ' 1) アプリケーションのビューステートをページの表示状態にマップする
            ' 2) ハードウェアのナビゲーション要求を処理する
            AddHandler Me.Page.Loaded,
                Sub(sender, e)
#If WINDOWS_PHONE_APP Then
                    AddHandler HardwareButtons.BackPressed, AddressOf HardwareButtons_BackPressed
#Else
                    ' キーボードおよびマウスのナビゲーションは、ウィンドウ全体を使用する場合のみ適用されます
                    If Me.Page.ActualHeight = Window.Current.Bounds.Height AndAlso Me.Page.ActualWidth = Window.Current.Bounds.Width Then
                        ' ウィンドウで直接待機するため、フォーカスは不要です
                        AddHandler Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated, AddressOf CoreDispatcher_AcceleratorKeyActivated
                        AddHandler Window.Current.CoreWindow.PointerPressed, AddressOf Me.CoreWindow_PointerPressed
                    End If
#End If
                End Sub

            ' ページが表示されない場合、同じ変更を元に戻します
            AddHandler Me.Page.Unloaded,
                Sub(sender, e)
#If WINDOWS_PHONE_APP Then
                    RemoveHandler HardwareButtons.BackPressed, AddressOf HardwareButtons_BackPressed
#Else
                    RemoveHandler Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated, AddressOf CoreDispatcher_AcceleratorKeyActivated
                    RemoveHandler Window.Current.CoreWindow.PointerPressed, AddressOf Me.CoreWindow_PointerPressed
#End If
                End Sub
        End Sub

#Region "ナビゲーション サポート"

        Private _goBackCommand As RelayCommand
        Public Property GoBackCommand() As RelayCommand
            Get
                If _goBackCommand Is Nothing Then
                    _goBackCommand = New RelayCommand(AddressOf Me.GoBack, AddressOf Me.CanGoBack)
                End If
                Return _goBackCommand
            End Get
            Set(value As RelayCommand)
                _goBackCommand = value
            End Set
        End Property

        Private _goForwardCommand As RelayCommand
        Public ReadOnly Property GoForwardCommand() As RelayCommand
            Get
                If _goForwardCommand Is Nothing Then
                    _goForwardCommand = New RelayCommand(AddressOf Me.GoForward, AddressOf Me.CanGoForward)
                End If
                Return _goForwardCommand
            End Get
        End Property

        Public Overridable Function CanGoBack() As Boolean
            Return Me.Frame IsNot Nothing AndAlso Me.Frame.CanGoBack
        End Function
        Public Overridable Function CanGoForward() As Boolean
            Return Me.Frame IsNot Nothing AndAlso Me.Frame.CanGoForward
        End Function

        Public Overridable Sub GoBack()
            If Me.Frame IsNot Nothing AndAlso Me.Frame.CanGoBack Then
                Me.Frame.GoBack()
            End If
        End Sub
        Public Overridable Sub GoForward()
            If Me.Frame IsNot Nothing AndAlso Me.Frame.CanGoForward Then
                Me.Frame.GoForward()
            End If
        End Sub

#If WINDOWS_PHONE_APP Then
        '''<summary>
        ''' 戻るボタンの押下を処理し、ルート フレームの履歴を使用して移動します。
        ''' </summary>
        Private Sub HardwareButtons_BackPressed(sender As Object, e As BackPressedEventArgs)
            If Me.GoBackCommand.CanExecute(Nothing) Then
                e.Handled = True
                Me.GoBackCommand.Execute(Nothing)
            End If
        End Sub
#Else
        ''' <summary>
        ''' このページがアクティブで、ウィンドウ全体を使用する場合、Alt キーの組み合わせなどのシステム キーを含む、
        ''' キーボード操作で呼び出されます。ページがフォーカスされていないときでも、
        ''' ページ間のキーボード ナビゲーションの検出に使用されます。
        ''' </summary>
        ''' <param name="sender">イベントをトリガーしたインスタンス。</param>
        ''' <param name="e">イベントが発生する条件を説明するイベント データ。</param>
        Private Sub CoreDispatcher_AcceleratorKeyActivated(sender As Windows.UI.Core.CoreDispatcher,
                                                           e As Windows.UI.Core.AcceleratorKeyEventArgs)
            Dim virtualKey As Windows.System.VirtualKey = e.VirtualKey

            ' 左方向キーや右方向キー、または専用に設定した前に戻るキーや次に進むキーを押した場合のみ、
            ' 詳細を調査します
            If (e.EventType = Windows.UI.Core.CoreAcceleratorKeyEventType.SystemKeyDown OrElse
                e.EventType = Windows.UI.Core.CoreAcceleratorKeyEventType.KeyDown) AndAlso
                (virtualKey = Windows.System.VirtualKey.Left OrElse
                virtualKey = Windows.System.VirtualKey.Right OrElse
                virtualKey = 166 OrElse
                virtualKey = 167) Then

                ' 押された修飾子キーを確認します
                Dim coreWindow As Windows.UI.Core.CoreWindow = Window.Current.CoreWindow
                Dim downState As Windows.UI.Core.CoreVirtualKeyStates = Windows.UI.Core.CoreVirtualKeyStates.Down
                Dim menuKey As Boolean = (coreWindow.GetKeyState(Windows.System.VirtualKey.Menu) And downState) = downState
                Dim controlKey As Boolean = (coreWindow.GetKeyState(Windows.System.VirtualKey.Control) And downState) = downState
                Dim shiftKey As Boolean = (coreWindow.GetKeyState(Windows.System.VirtualKey.Shift) And downState) = downState
                Dim noModifiers As Boolean = Not menuKey AndAlso Not controlKey AndAlso Not shiftKey
                Dim onlyAlt As Boolean = menuKey AndAlso Not controlKey AndAlso Not shiftKey

                If (virtualKey = 166 AndAlso noModifiers) OrElse
                    (virtualKey = Windows.System.VirtualKey.Left AndAlso onlyAlt) Then

                    ' 前に戻るキーまたは Alt キーを押しながら左方向キーを押すと前に戻ります
                    e.Handled = True
                    Me.GoBackCommand.Execute(Nothing)
                ElseIf (virtualKey = 167 AndAlso noModifiers) OrElse
                    (virtualKey = Windows.System.VirtualKey.Right AndAlso onlyAlt) Then

                    ' 次に進むキーまたは Alt キーを押しながら右方向キーを押すと次に進みます
                    e.Handled = True
                    Me.GoForwardCommand.Execute(Nothing)
                End If
            End If
        End Sub

        ''' <summary>
        ''' このページがアクティブで、ウィンドウ全体を使用する場合、マウスのクリック、タッチ スクリーンのタップなどの
        ''' 操作で呼び出されます。ページ間を移動するため、マウス ボタンのクリックによるブラウザー スタイルの
        ''' 次に進むおよび前に戻る操作の検出に使用されます。
        ''' </summary>
        ''' <param name="sender">イベントをトリガーしたインスタンス。</param>
        ''' <param name="e">イベントが発生する条件を説明するイベント データ。</param>
        Private Sub CoreWindow_PointerPressed(sender As Windows.UI.Core.CoreWindow,
                                              e As Windows.UI.Core.PointerEventArgs)
            Dim properties As Windows.UI.Input.PointerPointProperties = e.CurrentPoint.Properties

            ' 左、右、および中央ボタンを使用したボタン操作を無視します
            If properties.IsLeftButtonPressed OrElse properties.IsRightButtonPressed OrElse
                properties.IsMiddleButtonPressed Then Return

            ' [戻る] または [進む] を押すと適切に移動します (両方同時には押しません)
            Dim backPressed As Boolean = properties.IsXButton1Pressed
            Dim forwardPressed As Boolean = properties.IsXButton2Pressed
            If backPressed Xor forwardPressed Then
                e.Handled = True
                If backPressed Then Me.GoBackCommand.Execute(Nothing)
                If forwardPressed Then Me.GoForwardCommand.Execute(Nothing)
            End If
        End Sub
#End If

#End Region

#Region "プロセス継続時間管理"

        Private _pageKey As String

        ''' <summary>
        ''' このページがフレームに表示されるときに呼び出されます。
        ''' </summary>
        ''' <param name="e">このページにどのように到達したかを説明するイベント データ。Parameter 
        ''' プロパティは、表示するグループを示します。</param>
        Public Sub OnNavigatedTo(e As Navigation.NavigationEventArgs)
            Dim frameState As Dictionary(Of String, Object) = SuspensionManager.SessionStateForFrame(Me.Frame)
            Me._pageKey = "Page-" & Me.Frame.BackStackDepth

            If e.NavigationMode = Navigation.NavigationMode.New Then

                ' 新しいページをナビゲーション スタックに追加するとき、次に進むナビゲーションの
                ' 既存の状態をクリアします
                Dim nextPageKey As String = Me._pageKey
                Dim nextPageIndex As Integer = Me.Frame.BackStackDepth
                While (frameState.Remove(nextPageKey))
                    nextPageIndex += 1
                    nextPageKey = "Page-" & nextPageIndex
                End While


                ' ナビゲーション パラメーターを新しいページに渡します
                RaiseEvent LoadState(Me, New LoadStateEventArgs(e.Parameter, Nothing))
            Else

                ' ナビゲーション パラメーターおよび保存されたページの状態をページに渡します。
                ' このとき、中断状態の読み込みや、キャッシュから破棄されたページの再作成と同じ対策を
                ' 使用します
                RaiseEvent LoadState(Me, New LoadStateEventArgs(e.Parameter, DirectCast(frameState(Me._pageKey), Dictionary(Of String, Object))))
            End If
        End Sub

        ''' <summary>
        ''' このページがフレームに表示されなくなるときに呼び出されます。
        ''' </summary>
        ''' <param name="e">このページにどのように到達したかを説明するイベント データ。Parameter 
        ''' プロパティは、表示するグループを示します。</param>
        Public Sub OnNavigatedFrom(e As Navigation.NavigationEventArgs)
            Dim frameState As Dictionary(Of String, Object) = SuspensionManager.SessionStateForFrame(Me.Frame)
            Dim pageState As New Dictionary(Of String, Object)()
            RaiseEvent SaveState(Me, New SaveStateEventArgs(pageState))
            frameState(_pageKey) = pageState
        End Sub

        Public Event LoadState As LoadStateEventHandler
        Public Event SaveState As SaveStateEventHandler
#End Region

    End Class

    Public Class LoadStateEventArgs
        Inherits EventArgs

        Public Property NavigationParameter() As Object
        Public Property PageState() As Dictionary(Of String, Object)

        Public Sub New(navigationParameter As Object, pageState As Dictionary(Of String, Object))
            MyBase.New()
            Me.NavigationParameter = navigationParameter
            Me.PageState = pageState
        End Sub
    End Class
    Public Delegate Sub LoadStateEventHandler(sender As Object, e As LoadStateEventArgs)

    Public Class SaveStateEventArgs
        Inherits EventArgs

        Public Property PageState() As Dictionary(Of String, Object)

        Public Sub New(pageState As Dictionary(Of String, Object))
            MyBase.New()
            Me.PageState = pageState
        End Sub

    End Class

    Public Delegate Sub SaveStateEventHandler(sender As Object, e As SaveStateEventArgs)
End Namespace