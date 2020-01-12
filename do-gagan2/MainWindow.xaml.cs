using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Windows.Media.Animation;
using System.Windows.Ink;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using Microsoft.VisualBasic;


namespace do_gagan2
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        Storyboard _storyboard = null;
        bool isPlaying = false;
        bool ListBoxAutoScrollEnabled = false;
        DispatcherTimer Timer_KeepVolumeSliderOpen;
        bool isWhileFiltering = false;

        //自動保存設定バインディングプロパティ
        public bool isAutoSaveEnabled {
            get {
                Console.WriteLine("get:"+ Properties.Settings.Default.isAutoSaveEnabled);
                return Properties.Settings.Default.isAutoSaveEnabled;
            }
            set {
                Console.WriteLine("set:"+value);
                Properties.Settings.Default.isAutoSaveEnabled = value;
                Properties.Settings.Default.Save();
                Console.WriteLine("saved:" + Properties.Settings.Default.isAutoSaveEnabled);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            AppModel.MainWindow = this;
            AppModel.Records = new Do_gagan_Records();

            //スキップ秒数速度の設定コンテクストメニュー
            Btn_SkipForward.ContextMenu = new ContextMenu();
            MenuItem[] MI_SkipForwardFromButton = new MenuItem[10];
            MenuItem[] MI_SkipForwardFromMenu = new MenuItem[10];
            string[] skipSecsFw = new string[10] { "3秒", "5秒", "10秒", "15秒", "30秒", "60秒", "90秒", "3分", "5分", "10分" };

            for (int i = 0; i < 10; i++)
            {
                //>>ボタン右クリック用
                MI_SkipForwardFromButton[i] = new MenuItem();
                MI_SkipForwardFromButton[i].Header = skipSecsFw[i];
                MI_SkipForwardFromButton[i].Click += MI_SkipForward_Click;
                MI_SkipForwardFromButton[i].IsCheckable = true;
                Btn_SkipForward.ContextMenu.Items.Add(MI_SkipForwardFromButton[i]);

                //メニュー用
                MI_SkipForwardFromMenu[i] = new MenuItem();
                MI_SkipForwardFromMenu[i].Header = skipSecsFw[i];
                MI_SkipForwardFromMenu[i].Click += MI_SkipForward_Click;
                MI_SkipForwardFromMenu[i].IsCheckable = true;
                MI_SkipSecFoward.Items.Add(MI_SkipForwardFromMenu[i]);

            }

            Btn_SkipBackward.ContextMenu = new ContextMenu();
            MenuItem[] MI_SkipBackwardFromButton = new MenuItem[10];
            MenuItem[] MI_SkipBackwardFromMenu = new MenuItem[10];
            string[] skipSecsBack = new string[10] { "-3秒", "-5秒", "-10秒", "-15秒", "-30秒", "-60秒", "-90秒", "-3分", "-5分", "-10分" };
            for (int i = 0; i < 10; i++)
            {
                MI_SkipBackwardFromButton[i] = new MenuItem();
                MI_SkipBackwardFromButton[i].Header = skipSecsBack[i];
                MI_SkipBackwardFromButton[i].Click += MI_SkipBackward_Click;
                MI_SkipBackwardFromButton[i].IsCheckable = true;
                Btn_SkipBackward.ContextMenu.Items.Add(MI_SkipBackwardFromButton[i]);

                //メニュー用
                MI_SkipBackwardFromMenu[i] = new MenuItem();
                MI_SkipBackwardFromMenu[i].Header = skipSecsBack[i];
                MI_SkipBackwardFromMenu[i].Click += MI_SkipBackward_Click;
                MI_SkipBackwardFromMenu[i].IsCheckable = true;
                MI_SkipSecBackward.Items.Add(MI_SkipBackwardFromMenu[i]);
            }

            SetSkipSecUI();
            Player.Volume = Properties.Settings.Default.LastAudioVolume;
            Slider_Volume.Value = Player.Volume *100;

            //ジェスチャーインク設定
            InkCanvas1.DefaultDrawingAttributes.Color = Colors.LightBlue;

            //リストのソート設定（ビュー側でソートするだけ。コレクション自体は並び変わっていないので、保存時は別途ソートする）
            var collectionView = CollectionViewSource.GetDefaultView(AppModel.Records.Records);
            // Ageプロパティで昇順にソートする
            collectionView.SortDescriptions.Add(new SortDescription("TimeStamp", ListSortDirection.Ascending));

            //NewMemoブロックを初期化
            NewMemo_Initialize();

            //自動保存設定を反映
            isAutoSaveEnabled = Properties.Settings.Default.isAutoSaveEnabled;
            Console.WriteLine("AutoSaveEnabled:" + isAutoSaveEnabled);
        }

        #region 基本再生操作
        /// <summary>
        /// アプリケーション終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Quit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        //WPFでは終了イベントハンドラが必要
        protected virtual void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //終了処理
            if (AppModel.IsCurrentFileDirty)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("変更を上書き保存しますか？", "上書き保存確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                switch (result)
                {
                    case MessageBoxResult.Cancel:
                        //閉じない
                        e.Cancel = true;
                        break;
                    case MessageBoxResult.Yes:
                        if (SaveLog() )
                        {
                            Application.Current.Shutdown();
                        } else
                        {
                            //保存に失敗したら閉じない
                            e.Cancel = true;
                        }                        
                        break;
                    case MessageBoxResult.No:
                        Application.Current.Shutdown();
                        break;

                }
            }
        }

        /// <summary>
        /// 新規メディアに切り替える前に、未保存データを保存するか確認
        /// </summary>
        /// <returns>
        /// true: 切り替えを続行してOK
        /// false: 処理を中断する
        /// </returns>
        private bool ConfirmBeforeOpenMovie()
        {
            bool ret = false; //返値
            //終了処理
            if (AppModel.IsCurrentFileDirty)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("変更を上書き保存しますか？", "上書き保存確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                switch (result)
                {
                    case MessageBoxResult.Cancel:
                        //キャンセルされたから中断
                        ret = false;
                        break;
                    case MessageBoxResult.Yes:
                        if (SaveLog())
                        {
                            //保存に成功したから大丈夫
                            ret = true;
                        }
                        else
                        {
                            //保存に失敗したら閉じない
                            ret = false;
                        }
                        break;
                    case MessageBoxResult.No:
                        //保存しないを選んだから大丈夫
                        ret = true;
                        break;

                }
            } else
            {
                //未保存データはなし
                ret = true;
            }
            return ret;
        }


        /// <summary>
        /// 動画ファイルを選択するダイアログを表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenMovie(object sender, RoutedEventArgs e)
        {
            if (_storyboard != null) _storyboard.Pause(this);

            //未保存データがある場合は確認
            if (ConfirmBeforeOpenMovie() == false) return;

            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Multiselect = false;

            //初期ディレクトリをセット
            ofd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory); //デスクトップ
            if (!String.IsNullOrEmpty(Properties.Settings.Default.LastMovieFolder))
            {
                if (Directory.Exists(Properties.Settings.Default.LastMovieFolder))
                {
                    //フォルダが存在すればセット
                    ofd.InitialDirectory = Properties.Settings.Default.LastMovieFolder;
                }
            }

            //[ファイルの種類]に表示される選択肢を指定する
            //指定しないとすべてのファイルが表示される
            ofd.Filter = "メディアファイル(*.mp4;*.mpg;*.avi;*.wmv;*.mov;*.mp3;*.flac;*.wav;*.aac;*.m4a;*.ogg)|*.mp4;*.mpg;*.avi;*.wmv;*.mov;*.mp3;*.flac;*.wav;*.aac;*.m4a;*.ogg|すべてのファイル(*.*)|*.*";
            //[ファイルの種類]ではじめに選択されるものを指定する
            //2番目の「すべてのファイル」が選択されているようにする
            ofd.FilterIndex = 1;
            //タイトルを設定する
            ofd.Title = "動画/音声ファイルを選択してください";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd.RestoreDirectory = true;

            //ダイアログを表示する
            if (ofd.ShowDialog() == true)
            {
                OpenMovie(ofd.FileName);
            }
        }

        private void OpenMovie(string moviePath)
        {
            //最後に使ったフォルダを記憶
            Properties.Settings.Default.LastMovieFolder = Path.GetDirectoryName(moviePath);
            Properties.Settings.Default.Save();

            AppModel.CurrentMovieFilePath = moviePath;

            //ログファイルを切り離す
            AppModel.CurrentLogFilePath = "suppress";

            //既存ログをクリア
            AppModel.Records.Clear();

            Lb_DropHere.Visibility = Visibility.Hidden;

            //ログを読み込む
            //Ver2.0形式のファイルが存在したら読み、なければ1.0形式のファイルを探して読む。

            if (!LogReader.SearchDGGFile(moviePath))
            {
                if (!LogReader.SearchTXTFile(moviePath))
                {
                    Console.WriteLine("どちらのファイルも存在しない");
                    //設定形式を調べる
                    //ログファイルパスを作成してセットする
                    string logPath = Path.Combine(Path.GetDirectoryName(moviePath), Path.GetFileNameWithoutExtension(moviePath) + ".dggn.txt"); //2.0形式
                    Console.WriteLine("new log:" + logPath);
                    AppModel.CurrentLogFilePath = logPath;
                    ListBox_Records.DataContext = AppModel.Records.Records;
                    MI_Replace.IsEnabled = true;
                }
            }

            ListBoxAutoScrollEnabled = true;

            //動画を再生
            if (_storyboard != null)
                _storyboard.Stop();

            //ウインドウタイトルにファイル名を表示
            AppModel.IsCurrentFileDirty = false;

            //メディアタイムラインを作成
            MediaTimeline mediaTimeline = new MediaTimeline(new Uri(moviePath));
            mediaTimeline.CurrentTimeInvalidated += new EventHandler(mediaTimeline_CurrentTimeInvalidated);
            Storyboard.SetTargetName(mediaTimeline, Player.Name);

            //ストーリーボードを作成・開始
            _storyboard = new Storyboard();
            _storyboard.Children.Add(mediaTimeline);
            _storyboard.Begin(this, true);
            Slider_Time.IsEnabled = true;
            isPlaying = true;

            //UIを有効化
            TB_Search.IsEnabled = true;
            Btn_SkipBackward.IsEnabled = true;
            Btn_Play.IsEnabled = true;
            Btn_SkipForward.IsEnabled = true;
            MI_PlayBackControl.IsEnabled = true;
            MI_AddLog.IsEnabled = true;
            MI_Save.IsEnabled = true;
            MI_AutoSave.IsEnabled = true;
            MI_SaveNew.IsEnabled = true;
            //CB_NewLog.IsEnabled = true;

            //動画ファイルの時だけキャプチャボタンを有効化
            if (AppModel.getMediaType() == MediaType.Video)
            {
                Btn_Capture.IsEnabled = true;
            }

            Btn_Play.Focus();
        }

        //再生・一時停止ボタン
        private void Btn_PlayPause_click(object sender, RoutedEventArgs e)
        {
            PlayPause();
        }

        //再生一時停止トグル操作
        public void PlayPause()
        {
            if (_storyboard != null)
            {
                if (isPlaying)
                {
                    _storyboard.Pause(this);
                    //Player.Pause();
                    isPlaying = false;
                } else
                {
                    _storyboard.Resume(this);
                    //Player.Play();
                    isPlaying = true;
                }
            }
        }

        private void Stop()
        {
            isPlaying = false;
        }

        //前方ジャンプ
        private void Btn_SkipForward_click(object sender, RoutedEventArgs e)
        {
            MoveRelative(Properties.Settings.Default.SkipForwardSec);
        }
        //後方ジャンプ
        private void Btn_SkipBackward_click(object sender, RoutedEventArgs e)
        {
            MoveRelative(Properties.Settings.Default.SkipBackwardSec * -1);
        }

        //指定秒数に相対移動
        public void MoveRelative(int sec)
        {
            if (_storyboard != null)
            {
                TimeSpan offset = Player.Position + new TimeSpan(0, 00, sec);
                if (offset > Player.NaturalDuration.TimeSpan)
                {
                    offset = Player.NaturalDuration.TimeSpan - new TimeSpan(0, 0, 2);
                } else if (offset < new TimeSpan(0))
                {
                    Console.WriteLine("minus");
                    offset = TimeSpan.Zero;
                }

                _storyboard.SeekAlignedToLastTick(this,offset,TimeSeekOrigin.BeginTime);
                if (isPlaying)
                    _storyboard.Resume(this);

                ListBoxAutoScrollEnabled = true;
            }

        }

        //動画の指定位置にジャンプ
        public void MoveAbsolute(int sec)
        {
            if (_storyboard != null)
            {
                Console.WriteLine(sec);
                _storyboard.SeekAlignedToLastTick(this, new TimeSpan(0, 00, sec), TimeSeekOrigin.BeginTime);
            }
        }

        //動画の総再生時間を返す
        public double GetMediaDuration()
        {
            if (_storyboard != null)
            {
                return Player.NaturalDuration.TimeSpan.TotalSeconds;
            } else
            {
                return 0.0;            }
        }

        //メディアタイムラインの現在時間が無効化された時
        void mediaTimeline_CurrentTimeInvalidated(object sender, EventArgs e)
        {
            //ストーリーボードがnullでない（＝再生する（している）動画が存在する）場合
            if (_storyboard != null)
            {
                //タイムスライダの更新
                Slider_Time.Value = Player.Clock.CurrentTime.Value.TotalMilliseconds;

                //再生箇所に相当するログをハイライト
                HighlightLog();
                //
                if (Player.Clock.CurrentTime.HasValue && Player.Clock.NaturalDuration.HasTimeSpan)
                {
                    //1時間半なら90分と表示
                    string current = (Player.Clock.CurrentTime.Value.Hours * 60 + Player.Clock.CurrentTime.Value.Minutes).ToString("D2") + ":" + Player.Clock.CurrentTime.Value.Seconds.ToString("D2");
                    string total = (Player.Clock.NaturalDuration.TimeSpan.Hours * 60 + Player.Clock.NaturalDuration.TimeSpan.Minutes).ToString("D2") + ":" + Player.Clock.NaturalDuration.TimeSpan.Seconds.ToString("D2");
                    TextBlock_Time.Text = current + "/" + total;
                }

                //動画が終了している場合
                if (Player.NaturalDuration.HasTimeSpan == true && Player.Clock.CurrentTime.Value.TotalMilliseconds == Player.NaturalDuration.TimeSpan.TotalMilliseconds)
                {
                    //再生停止
                    Stop();
                }
            }
        }

        //キーボード入力に反応
        public void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        PlayPause();
                        e.Handled = true; //テキストボックスなどにスペースが挿入されない様、イベントを断ち切る
                    }
                    break;
                case Key.S:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        SaveLog();
                    }
                    break;
                case Key.W:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        MoveRelative(Properties.Settings.Default.SkipForwardSec);
                    }
                    break;
                case Key.Q:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        MoveRelative(Properties.Settings.Default.SkipBackwardSec * -1);
                    }
                    break;
                case Key.M:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        Toggle_NewLogBlock();
                        e.Handled = true;
                    }
                    break;

                //NewMemoブロック
                case Key.F1:
                    Btn_F1_Click(null, null);
                    break;
                case Key.F2:
                    Btn_F2_Click(null, null);
                    break;
                case Key.F3:
                    Btn_F3_Click(null, null);
                    break;
                case Key.F4:
                    Btn_F4_Click(null, null);
                    break;
                case Key.F5:
                    Btn_F5_Click(null, null);
                    break;
                case Key.Enter:
                    Btn_Save_Click(null, null);
                    e.Handled = true;
                    break;
                case Key.L:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        Update_LockOn();
                    }
                    break;

            }
        }

        //操作パネル上でAltショートカットが押された時の処理
        private void Window_AccessKeyPressed(object sender, AccessKeyPressedEventArgs e)
        {
            switch (e.Key)
            {
                case "W":
                    MoveRelative(Properties.Settings.Default.SkipForwardSec);
                    e.Handled = true;
                    break;
                case "Q":
                    MoveRelative(Properties.Settings.Default.SkipBackwardSec * -1);
                    e.Handled = true;
                    break;
                case "S":
                    PlayPause();
                    e.Handled = true;
                    break;
                case "L":
                    Update_LockOn();
                    e.Handled = true;
                    break;
                case "M":
                    Toggle_NewLogBlock();
                    e.Handled = true;
                    break;
            }

        }
        #endregion

        #region メディアのドラッグ＆ドロップ
        private void MediaElement_DragEnter(object sender, DragEventArgs e)
        {
            Console.WriteLine("DragEnter");

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.All;
                //Lb_DropHere.Visibility = Visibility.Hidden;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        //リストにドラッグされたファイルを逐次処理して登録
        private void MediaElement_Drop(object sender, DragEventArgs e)
        {
            //未保存データがある場合は確認
            if (ConfirmBeforeOpenMovie() == false) return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            Console.WriteLine("Dropped:" + files.Count());
            if (files.Count() > 1)
            {
                MessageBox.Show("メディアファイルは1つだけドロップしてください。");
                return;
            }

            foreach (var file in files)
            {
                //Console.WriteLine(file);
                string ext = System.IO.Path.GetExtension(file).ToLower();
                if (ext == ".aac" || ext == ".m4a" || ext == ".mp3" || ext == ".wav" || ext == ".flac" || ext == ".ogg" || ext == ".mpg" || ext == ".mp4" || ext == ".mov" || ext == ".avi" || ext == ".wmv")
                {
                    Lb_DropHere.Visibility = Visibility.Hidden;
                    OpenMovie(file);
                }
                else
                {
                    MessageBox.Show(System.IO.Path.GetFileName(file) + "は非対応ファイルです。");
                }
            }
        }

        /// <summary>
        /// ドラッグがキャンセルされた
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaElement_DragLeave(object sender, DragEventArgs e)
        {
            //Console.WriteLine("DragCancelled");
            Lb_DropHere.Visibility = Visibility.Visible;
        }
        #endregion

        #region スライダー関連
        //動画を開いた時にスライダーの最大値を動画の長さにあわせる
        private void Element_MediaOpened(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Duraton:" + Player.NaturalDuration.TimeSpan.TotalSeconds + "sec");
            Slider_Time.Maximum = Player.NaturalDuration.TimeSpan.TotalMilliseconds;
        }
        //スライダーを更新（再生中に定期的に呼ばれる）→呼ばれてない
        private void MediaTimeChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Tick");
            Slider_Time.Value = Player.Position.TotalMilliseconds;

            //ログのフォーカス行を移動する
            HighlightLog();

        }
        //タイムスライダがドラッグ開始した時
        private void sliderTime_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            Console.WriteLine("Slider DragStarted");
            //再生を一時停止（mediaTimeline_CurrentTimeInvalidatedの発生を防ぐ）
            if (isPlaying)
                _storyboard.Pause(this);
        }

        //タイムスライダがドラッグ完了した時
        private void sliderTime_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            Console.WriteLine("Slider Ended");
            //シークする
            _storyboard.Seek(this, new TimeSpan((long)Math.Floor(Slider_Time.Value * TimeSpan.TicksPerMillisecond)), TimeSeekOrigin.BeginTime);
            //再生を再開
            if (isPlaying)
                _storyboard.Resume(this);

            ListBoxAutoScrollEnabled = true;

        }


        #endregion

        #region ヘルプメニュー関連
        private void MenuItem_UsageGuide(object sender, RoutedEventArgs e)
        {
            AppModel.GoUsageGuide();
        }

        private void MenuItem_VersionInfo(object sender, RoutedEventArgs e)
        {
            AppModel.ShowVersionInfo();
        }

        #endregion

        #region リスト項目関連

        //項目がクリックされた
        private void LogClicked(object sender, MouseButtonEventArgs e)
        {
            Dogagan_Record item = (e.OriginalSource as FrameworkElement)?.DataContext as Dogagan_Record;
            if (item != null) {
                //Console.WriteLine(item.TimeStamp + " " + item.Transcript);
                _storyboard.Seek(this, new TimeSpan(0,0,(int)item.TimeStamp+1), TimeSeekOrigin.BeginTime);
                ListBoxAutoScrollEnabled = true;

                //一時停止中なら再生再開する
                _storyboard.Resume(this);
                isPlaying = true;
            }
        }

        //動画の再生位置にあわせて該当行をハイライトする
        private void HighlightLog()
        {
            var current = AppModel.Records.Records.Where(r => r.TimeStamp < Player.Position.TotalSeconds).LastOrDefault();
            if (current != null && ListBoxAutoScrollEnabled==true)
            {
                //Console.WriteLine("Time:" + current.TimeStamp + " Text:" + current.Transcript);
                ListBox_Records.SelectedItem = current;
                ListBox_Records.ScrollIntoView(current);
            }
        }


        //リスト項目が右クリックされてコンテクストメニューが開いた
        private void StackPanel_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            //現在の話者ラベルを変更メニューのチェックに反映
            Dogagan_Record item = (e.OriginalSource as FrameworkElement)?.DataContext as Dogagan_Record;
            StackPanel sp = sender as StackPanel;
            foreach(MenuItem mi in sp.ContextMenu.Items)
            {
                if (mi.Header.ToString() == "話者ラベル") {
                    foreach (MenuItem mi_spk in mi.Items)
                    {
                        if (mi_spk.Header.ToString() == item.Speaker)
                        {
                            mi_spk.IsChecked = true;
                        } else
                        {
                            mi_spk.IsChecked = false;
                        }
                    }
                }

            }
        }


        //リスト項目の右クリックメニューから話者変更
        private void MI_SpeakerLabelChange_Click(object sender, RoutedEventArgs e)
        {
            Dogagan_Record item = (e.OriginalSource as FrameworkElement)?.DataContext as Dogagan_Record;
            var newSpeakerLabel = e.Source as MenuItem;
            item.Speaker = newSpeakerLabel.Header.ToString();
            item.Renew();
            AppModel.IsCurrentFileDirty = true;
        }


        //削除
        private void MI_LogDelete_Clicked(object sender, RoutedEventArgs e)
        {
            Dogagan_Record item = (e.OriginalSource as FrameworkElement)?.DataContext as Dogagan_Record;
            if (item != null)
            {
                AppModel.Records.Delete(item);
            } else
            {
                MessageBox.Show("行削除に失敗しました。");
            }
        }

        /// <summary>
        /// リスト上でマウスホイールが操作されたら自動スクロールを無効化する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ListBoxAutoScrollEnabled = false;
        }

        #endregion

        #region 検索（フィルタ）,置換関係

        //検索ボックスをクリア
        private void Btn_SearchClear_Click(object sender, RoutedEventArgs e)
        {
            isWhileFiltering = true;
            TB_Search.Text = "";
            ListBox_Records.DataContext = AppModel.Records.Records;
            isWhileFiltering = false;

        }

        private void TB_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            isWhileFiltering = true;
            AppModel.FilteredRecords = null;
            AppModel.FilteredRecords = AppModel.Records.Records.Where(r => r.TranscriptForSearch.Contains(Strings.StrConv(TB_Search.Text, VbStrConv.Wide, 0x411).ToLower())).ToList();
            //Console.WriteLine("Count:"+AppModel.FilteredRecords.Count);
            ListBox_Records.DataContext = AppModel.FilteredRecords;
            isWhileFiltering = false;
        }
        //置換
        private void MI_Replace_Click(object sender, RoutedEventArgs e)
        {
            Window_Replace replace = new Window_Replace();
            replace.ShowDialog();
        }
        #endregion


        #region スキップ秒数変更ボタン
        private void MI_SkipForward_Click(object sender, RoutedEventArgs e)
        {
            string btnName = (sender as MenuItem).Header.ToString();
            int sec = AppModel.SkipSecIndexFromBtnName(btnName);

            //設定変更
            Properties.Settings.Default.SkipForwardSec = sec;
            Properties.Settings.Default.Save();

            SetSkipSecUI();
        }
        private void MI_SkipBackward_Click(object sender, RoutedEventArgs e)
        {
            string btnName = (sender as MenuItem).Header.ToString().Replace("-", ""); //マイナス記号を抜く
            int sec = AppModel.SkipSecIndexFromBtnName(btnName);
            
            //設定変更
            Properties.Settings.Default.SkipBackwardSec = sec;
            Properties.Settings.Default.Save();

            SetSkipSecUI();
        }


        //スキップ秒数設定変更時に、メニューのチェックとボタン表記、ToolTipを更新する
        private void SetSkipSecUI()
        {
            //メニュー全部オフ
            //Forward
            foreach (MenuItem mi in MI_SkipSecFoward.Items)
            {
                mi.IsChecked = false;
            }
            foreach (MenuItem mi in Btn_SkipForward.ContextMenu.Items)
            {
                mi.IsChecked = false;
            }
            //Backword
            foreach (MenuItem mi in MI_SkipSecBackward.Items)
            {
                mi.IsChecked = false;
            }
            foreach (MenuItem mi in Btn_SkipBackward.ContextMenu.Items)
            {
                mi.IsChecked = false;
            }

            int ForwardSec = Properties.Settings.Default.SkipForwardSec;
            int ForwardSecIndex = AppModel.SkipSecIndexNumber(ForwardSec);
            string ForwardSecString = AppModel.SkipSecString(ForwardSec);
            string ForwardSecBtnLabel = AppModel.SkipSecBtnLabel(ForwardSec);
            //Console.WriteLine(ForwardSec + "ForwardSecBtnLabel:" + ForwardSecBtnLabel);

            int BackwardSec = Properties.Settings.Default.SkipBackwardSec;
            int BackwardSecIndex = AppModel.SkipSecIndexNumber(BackwardSec);
            string BackwardSecString = AppModel.SkipSecString(BackwardSec);
            string BackwardSecBtnLabel = AppModel.SkipSecBtnLabel(BackwardSec);


            //メニュー指定番目をチェック
            (MI_SkipSecFoward.Items[ForwardSecIndex] as MenuItem).IsChecked = true;
            (Btn_SkipForward.ContextMenu.Items[ForwardSecIndex] as MenuItem).IsChecked = true;
            (MI_SkipSecBackward.Items[BackwardSecIndex] as MenuItem).IsChecked = true;
            (Btn_SkipBackward.ContextMenu.Items[BackwardSecIndex] as MenuItem).IsChecked = true;

            //ボタンラベル
            Btn_SkipForwardSecLabel.Content = ForwardSecBtnLabel;
            Btn_SkipBackwardSecLabel.Content = BackwardSecBtnLabel;

            //ボタンTooltip
            Btn_SkipForward.ToolTip = ForwardSecString + "進む\n右クリックで変更";
            Btn_SkipBackward.ToolTip = BackwardSecString + "戻る\n右クリックで変更";
        }

        #endregion

        #region 音量スライダー
        private void Btn_Volume_Click(object sender, RoutedEventArgs e)
        {
            Popup_VolumeSlider.IsOpen = !Popup_VolumeSlider.IsOpen;
            //表示されたら自動非表示までのタイマーをセット
            if (Popup_VolumeSlider.IsOpen == true)
            {
                //表示した
                SetVolumeSliderDisappearTimer();
            } else
            {
                //消した
                Timer_KeepVolumeSliderOpen.Stop();
            }
        }

        private void Slider_Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.Volume = Slider_Volume.Value / 100;
            Properties.Settings.Default.LastAudioVolume = Player.Volume;
            Properties.Settings.Default.Save();
            //Console.WriteLine("Saved Volume:"+Properties.Settings.Default.LastAudioVolume);
        }

        private void Popup_VolumeSlider_MouseEnter(object sender, MouseEventArgs e)
        {
            Timer_KeepVolumeSliderOpen.Stop();
        }

        private void Popup_VolumeSlider_MouseLeave(object sender, MouseEventArgs e)
        {
            SetVolumeSliderDisappearTimer(1);
        }

        private void SetVolumeSliderDisappearTimer(int sec = 3)
        {
            if (Timer_KeepVolumeSliderOpen != null)
            {
                Timer_KeepVolumeSliderOpen.Stop();
                Timer_KeepVolumeSliderOpen = null;
            }
            Timer_KeepVolumeSliderOpen = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            Timer_KeepVolumeSliderOpen.Interval = TimeSpan.FromSeconds(sec);
            Timer_KeepVolumeSliderOpen.Tick += new EventHandler(DispatcherTimer_Tick);
            Timer_KeepVolumeSliderOpen.Start();
        }

        //時間が経ったらスライダーを非表示にする
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Popup_VolumeSlider.IsOpen = false;
            Timer_KeepVolumeSliderOpen.Stop();
        }


        #endregion

        #region ジェスチャの認識
        private void inkCanvas1_Gesture(object sender, InkCanvasGestureEventArgs e)
        {
            // ジェスチャの認識結果を取得
            var gestureResults = e.GetGestureRecognitionResults();

            // 認識結果の信頼性が高い順にメッセージを表示する
            if (gestureResults[0].RecognitionConfidence == RecognitionConfidence.Strong)
            {
                //Console.WriteLine("Gesture:"+gestureResults[0].ApplicationGesture.ToString());
                //認識可能ジェスチャー一覧
                //https://docs.microsoft.com/ja-jp/dotnet/api/system.windows.ink.applicationgesture?redirectedfrom=MSDN&view=netframework-4.8

                if (_storyboard != null)
                    //動画読み込み後
                    switch (gestureResults[0].ApplicationGesture.ToString())
                    {
                        case "Tap":
                            PlayPause();
                            break;
                        case "Right":
                            MoveRelative(Properties.Settings.Default.SkipForwardSec);
                            break;
                        case "Left":
                            MoveRelative(Properties.Settings.Default.SkipBackwardSec * -1);
                            break;
                        case "Up":
                            break;
                        case "Down":
                            break;
                        default:
                            break;
                    }
            }
        }
        #endregion

        private void Popup_VolumeSlider_LostFocus(object sender, RoutedEventArgs e)
        {
            Popup_VolumeSlider.IsOpen = false;
        }

        #region ファイル保存関連
        /// <summary>
        /// 上書き保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MI_Save_Click(object sender, RoutedEventArgs e)
        {
            SaveLog();
        }
        public bool SaveLog(bool suppressUpdateDirtyFlag = false)
        {
            //編集中のセルから抜けるために、検索欄にフォーカス
            Btn_Play.Focus();

            string body = "";
            Encoding enc;

            //メディアファイル切り替え時の保存は抑止
            if (AppModel.CurrentLogFilePath== "suppress") return true;

                if (Path.GetFileName(AppModel.CurrentLogFilePath).EndsWith(".dggn.txt"))
            {
                //V2フォーマットで保存
                Console.WriteLine("Save V2");
                body = AppModel.Records.ToString(false,FileFormatVersion.Type2);
                enc = Encoding.GetEncoding("UTF-8");
            } else if (Path.GetExtension(AppModel.CurrentLogFilePath) == ".txt")
            {
                //V1フォーマットで保存
                Console.WriteLine("Save V1");
                body = AppModel.Records.ToString(false,FileFormatVersion.Type1);
                enc = Encoding.GetEncoding("Shift_JIS");
            } else
            {
                MessageBox.Show("未知の拡張子のため上書き保存できませんでした。");
                return false;
            }
            using (StreamWriter writer = new StreamWriter(AppModel.CurrentLogFilePath, false, enc))
            {
                writer.WriteLine(body);
            }
            AppModel.IsCurrentFileDirty = false;
            return true;
        }

        /// <summary>
        /// 新規保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MI_SaveNew_Click(object sender, RoutedEventArgs e)
        {
            //編集中のセルから抜けるために、検索欄にフォーカス
            Btn_Play.Focus();

            if (_storyboard == null) return;
            _storyboard.Pause(this);

            var result = MessageBox.Show("動画と異なるファイル名で保存した記録ファイルは、次回動画を開く時に自動では読み込まれません。\n（動画と同じ名前の記録ファイルが自動で読み込まれます）", "確認", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Cancel) return;

            var sfd = new SaveFileDialog();

            //はじめのファイル名を指定する
            //はじめに「ファイル名」で表示される文字列を指定する
            sfd.FileName = "新しいログファイル.dggn";
            //はじめに表示されるフォルダを指定する
            sfd.InitialDirectory = Path.GetDirectoryName(AppModel.CurrentLogFilePath);
            //[ファイルの種類]に表示される選択肢を指定する
            //指定しない（空の文字列）の時は、現在のディレクトリが表示される
            sfd.Filter = "動画眼2.x形式タブ区切りテキストファイル[UTF-8](.dggn.txt)|*.dggn|動画眼1.x形式タブ区切りテキストファイル[Shift-JIS](.txt)|*.txt";
            //[ファイルの種類]ではじめに選択されるものを指定する
            //2番目の「すべてのファイル」が選択されているようにする
            sfd.FilterIndex = 1;
            //タイトルを設定する
            sfd.Title = "保存するログファイルのファイル名と形式を選択してください";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            sfd.RestoreDirectory = true;
            //既に存在するファイル名を指定したとき警告する
            sfd.OverwritePrompt = true;
            //存在しないパスが指定されたとき警告を表示する
            sfd.CheckPathExists = true;
            //ダイアログを表示する
            if (sfd.ShowDialog() == true)
            {
                //OKボタンがクリックされたとき、選択されたファイル名を表示する
                Console.WriteLine(sfd.FilterIndex);
                string body = "";
                string newFilePath = "";
                Encoding enc;
                if (sfd.FilterIndex == 1)
                {
                    //2.0形式で保存
                    //V2フォーマットで保存
                    body = AppModel.Records.ToString(false, FileFormatVersion.Type2);
                    enc = Encoding.GetEncoding("UTF-8");
                    newFilePath = sfd.FileName.Replace(".dggn", ".dggn.txt");

                }
                else
                {
                    //1.0形式で保存
                    //V1フォーマットで保存
                    body = AppModel.Records.ToString(false, FileFormatVersion.Type1);
                    enc = Encoding.GetEncoding("Shift_JIS");
                    newFilePath = sfd.FileName;
                }
                //Console.WriteLine(newFilePath);

                bool go = false; //保存実行フラグ
                if (File.Exists(newFilePath)){
                    var result2 = MessageBox.Show(newFilePath +"は存在します。上書きしますか？","上書き確認",MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (result2 == MessageBoxResult.OK) go = true;
                } else
                {
                    //既存ファイルがないからgo
                    go = true;
                }

                if (go)
                {
                    Console.WriteLine("Save実行");
                    using (StreamWriter writer = new StreamWriter(newFilePath, false, enc))
                    {
                        writer.WriteLine(body);
                    }
                }

            }

        }
        #endregion

        //新規メモボタン（NewMemoブロックの表示トグル）
        private void Toggle_NewLogBlock()
        {
            if (NewMemo.Visibility == Visibility.Visible)
            {
                CB_NewLog_Unchecked(null, null);
            }
            else
            {
                CB_NewLog_Checked(null, null);
            }
        }
        private void MI_ToggleNewMemo_Click(object sender, RoutedEventArgs e)
        {
            Toggle_NewLogBlock();
        }

        private void CB_NewLog_Checked(object sender, RoutedEventArgs e)
        {
            CB_NewLog.IsChecked = true;
            MI_ToggleNewMemo.IsChecked = true;
            NewMemo.Visibility = Visibility.Visible;
            Update_LockOn(); //こっちを後にすること
            Properties.Settings.Default.NewMemoBlockDisplayed = true;
            Properties.Settings.Default.Save();
        }

        private void CB_NewLog_Unchecked(object sender, RoutedEventArgs e)
        {
            CB_NewLog.IsChecked = false;
            MI_ToggleNewMemo.IsChecked = false;
            NewMemo.Visibility = Visibility.Collapsed;
            Properties.Settings.Default.NewMemoBlockDisplayed = false;
            Properties.Settings.Default.Save();
        }


        //セルのテキストが変更された
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Console.WriteLine("Hoge");
            //フィルタリング時になぜか呼ばれるのでフラグで除外
            if (isWhileFiltering == false)
            {
                Console.WriteLine("Fuga");
                //var changedTextBox = e.Source as TextBox;
                //Console.WriteLine("TextChanged:"+changedTextBox.Text);
                //ダーティフラグを立てる
                AppModel.IsCurrentFileDirty = true;
            }

        }


        //今開いているログに、別ファイルのログをマージする
        private void MI_AddLog_Click(object sender, RoutedEventArgs e)
        {
            if (_storyboard != null) _storyboard.Pause(this);

            OpenFileDialog ofd = new OpenFileDialog();

            //初期ディレクトリをセット
            ofd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory); //デスクトップ
            if (!String.IsNullOrEmpty(Properties.Settings.Default.LastMovieFolder))
            {
                if (Directory.Exists(Properties.Settings.Default.LastMovieFolder))
                {
                    //フォルダが存在すればセット
                    ofd.InitialDirectory = Properties.Settings.Default.LastMovieFolder;
                }
            }

            //[ファイルの種類]に表示される選択肢を指定する
            //指定しないとすべてのファイルが表示される
            //ofd.Filter = "ログファイル(*.txt)|*.txt|すべてのファイル(*.*)|*.*";
            ofd.Filter = "動画眼2.x形式タブ区切りテキストファイル[UTF-8](.dggn.txt)|*.dggn.txt|動画眼1.x形式タブ区切りテキストファイル[Shift-JIS](.txt)|*.txt";

            //[ファイルの種類]ではじめに選択されるものを指定する
            //2番目の「すべてのファイル」が選択されているようにする
            ofd.FilterIndex = 1;
            //タイトルを設定する
            ofd.Title = "追加読み込み（マージ）するログファイルを選択してください";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd.RestoreDirectory = true;
            ofd.Multiselect = false;

            //ダイアログを表示する
            if (ofd.ShowDialog() == true)
            {
                //フィルター選択よりファイル拡張子を信頼して分岐
                if (Path.GetFileName(ofd.FileName).EndsWith(".dggn.txt")){
                    LogReader.LoadDGGFile(ofd.FileName);
                } else
                {
                    LogReader.LoadTXTFile(ofd.FileName);
                }
            }
        }

        /// <summary>
        /// セルを選択位置で分割する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MI_SeparateCell_Click(object sender, RoutedEventArgs e)
        {
            Dogagan_Record item = (e.OriginalSource as FrameworkElement)?.DataContext as Dogagan_Record;
            MenuItem mi = sender as MenuItem;
            Console.WriteLine("mi.parent:"+mi.Parent.GetType());
            ContextMenu cm = mi.Parent as ContextMenu;
            TextBox tb = cm.PlacementTarget as TextBox; //対象のテキストボックス
            if (tb.SelectionLength > 0)
            {
                MessageBox.Show("1文字以上の文字を選択していると分割できません。");
            }
            else if (tb.SelectionStart == 0 || tb.SelectionStart >= tb.Text.Length)
            {
                MessageBox.Show("この位置では分割できません。");
            } else 
            {
                string text1 = tb.Text.Substring(0, tb.SelectionStart);
                string text2 = tb.Text.Substring(tb.SelectionStart, tb.Text.Length- tb.SelectionStart);
                item.Transcript = text1;
                item.Renew();
                Dogagan_Record newItem = new Dogagan_Record();
                newItem.TimeStamp = item.TimeStamp + 1;
                newItem.Transcript = text2;
                newItem.Speaker = "0";
                AppModel.Records.Add(newItem);

            }
        }


        //現在のコマを静止画で保存
        private void Btn_Capture_Click(object sender, RoutedEventArgs e)
        {

            int width = (int)(Player.NaturalVideoWidth);
            int height = (int)(Player.NaturalVideoHeight);

            Console.WriteLine("x:" + Player.ActualWidth + " NaturalX"+Player.NaturalVideoWidth);
            Console.WriteLine("media w:"+ MediaDockPanel.ActualWidth);
            //画面のdpiを取得
            PresentationSource source = PresentationSource.FromVisual(this);
            double dpiX, dpiY;
            if (source != null)
            {
                dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
            } else
            {
                dpiX = 96.0;
                dpiY = 96.0;
            }
            Console.WriteLine(source.CompositionTarget.TransformToDevice.M11);
            // レンダリングするビットマップを用意
            var bitmap = new RenderTargetBitmap(width, height, dpiX, dpiY, PixelFormats.Pbgra32);

            // ビットマップに Visual をレンダリング
            bitmap.Render(Player);

            // PNG として保存
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            //
            var current = Player.Position.ToString(@"hh\Hmm\Mss\S");
            //ファイル名
            string fileName = Path.Combine(Path.GetDirectoryName(AppModel.CurrentMovieFilePath), Path.GetFileNameWithoutExtension(AppModel.CurrentMovieFilePath) + "_" + current + ".png");
            using (var stream = File.OpenWrite(fileName))
            {
                encoder.Save(stream);
            }
        }


        #region NewMemo
        /// <summary>
        /// ウインドウ下部の新規メモ欄ブロック
        /// </summary>

        public double LockedPosition { get; set; } = 0.0;

        public int SpeakerID { get; set; } = 0;

        //メモ欄表示
        //public Window_NewMemo(double position, int speaker)
        public void NewMemo_Initialize ()
        {
            DataContext = this;
            LockedPosition = 0.0;
            SpeakerID = 0;
            OnPropertyChanged("LockedPosition");
            OnPropertyChanged("SpeakerID");

            if (Properties.Settings.Default.NewMemoBlockDisplayed==true)
            {
                CB_NewLog_Checked(null, null);
                TB_Memo.Focus();
            }

        }

        private void Btn_F1_Click(object sender, RoutedEventArgs e)
        {
            TB_Memo.Text = "タスク開始:" + TB_Memo.Text;
            TB_Memo.Select(TB_Memo.Text.Length, 0); //末尾にカーソル
            TB_Memo.Focus();
        }
        private void Btn_F2_Click(object sender, RoutedEventArgs e)
        {
            TB_Memo.Text = "参加者「" + TB_Memo.Text + "」";
            TB_Memo.Select(4, 0); //"「"の次にカーソル
            TB_Memo.Focus();
        }

        private void Btn_F3_Click(object sender, RoutedEventArgs e)
        {
            TB_Memo.Text = "進行役「" + TB_Memo.Text + "」";
            TB_Memo.Select(4, 0); //"「"の次にカーソル
            TB_Memo.Focus();
        }
        private void Btn_F4_Click(object sender, RoutedEventArgs e)
        {
            TB_Memo.Text = "見所！:" + TB_Memo.Text;
            TB_Memo.Select(TB_Memo.Text.Length, 0); //末尾にカーソル
            TB_Memo.Focus();
        }

        private void Btn_F5_Click(object sender, RoutedEventArgs e)
        {
            TB_Memo.Text = "タスク完了:" + TB_Memo.Text;
            TB_Memo.Select(TB_Memo.Text.Length, 0); //末尾にカーソル
            TB_Memo.Focus();
        }

        //ロックオン秒数を現在の再生時間に更新
        private void Update_LockOn()
        {
            Console.WriteLine("UpdateLockOn");
            LockedPosition = Player.Position.TotalSeconds;
            OnPropertyChanged("LockedPosition");
            if (NewMemo.Visibility != Visibility.Visible)
            {
                CB_NewLog_Checked(null, null); //NewLogブロックを表示
            }
            TB_Memo.Focus();
        }

        private void Btn_LockOn_Decrease1sec_Click(object sender, RoutedEventArgs e)
        {
            if (LockedPosition - 1.0 < 0)
            {
                LockedPosition = 0.0;
            }
            else
            {
                LockedPosition -= 1.0;
            }
            OnPropertyChanged("LockedPosition");
            MoveAbsolute((int)LockedPosition); //再生位置を移動
            TB_Memo.Focus();
        }

        private void Btn_LockOn_Increase1sec_Click(object sender, RoutedEventArgs e)
        {
            if (LockedPosition + 1.0 < GetMediaDuration())
            {
                LockedPosition += 1.0;

            }
            else
            {
                LockedPosition = GetMediaDuration();
            }

            OnPropertyChanged("LockedPosition");
            MoveAbsolute((int)LockedPosition); //再生位置を移動
            TB_Memo.Focus();
        }

        private void Btn_Speaker_Decrease_Click(object sender, RoutedEventArgs e)
        {
            if (SpeakerID > 0)
                SpeakerID -= 1;
            OnPropertyChanged("SpeakerID");
            TB_Memo.Focus();
        }

        private void Btn_Speaker_Increase_Click(object sender, RoutedEventArgs e)
        {
            SpeakerID += 1;
            OnPropertyChanged("SpeakerID");
            TB_Memo.Focus();
        }


        //データバインディングの更新に必要
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //行保存を実行
        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            Dogagan_Record rec = new Dogagan_Record();
            rec.TimeStamp = (float)LockedPosition;
            rec.Transcript = TB_Memo.Text;
            rec.Speaker = SpeakerID.ToString();

            AppModel.Records.Add(rec);
            AppModel.IsCurrentFileDirty = true;

            //フィールドをクリア
            LockedPosition = 0.0;
            TB_Memo.Text = "";
        }


        #endregion

        private void TB_Speaker_GotFocus(object sender, RoutedEventArgs e)
        {
            TB_Memo.Focus();
        }


        //自動保存のON/OFF
        private void MI_AutoSave_Click(object sender, RoutedEventArgs e)
        {
            //if (isAutoSaveEnabled == true)
            //{
            //    isAutoSaveEnabled = false;
            //    //MI_AutoSave.IsChecked = false;
            //} else
            //{
            //    isAutoSaveEnabled = true;
            //    //MI_AutoSave.IsChecked = true;
            //}
            isAutoSaveEnabled = isAutoSaveEnabled;
            OnPropertyChanged("isAutoSaveEnabled");
        }

        private void TB_LockedTimeCode_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Update_LockOn();
            e.Handled = true;
        }
    }
}
