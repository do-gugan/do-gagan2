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

namespace do_gagan2
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        Storyboard _storyboard = null;
        bool isPlaying = false;
        bool ListBoxAutoScrollEnabled = false;

        public MainWindow()
        {
            InitializeComponent();
            AppModel.MainWindow = this;
            AppModel.Records = new Do_gagan_Records();

            //スキップ秒数速度の設定コンテクストメニュー
            Btn_SkipForward.ContextMenu = new ContextMenu();
            MenuItem[] MI_SkipForwardFromButton = new MenuItem[10];
            MenuItem[] MI_SkipForwardFromMenu = new MenuItem[10];
            string[] skipSecsFw = new string[10] {"3秒", "5秒", "10秒", "15秒", "30秒", "60秒", "90秒", "3分", "5分", "10分"};
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
                MI_SkipSecBackword.Items.Add(MI_SkipBackwardFromMenu[i]);
            }

            //スキップボタンのTooltipをセット
            Btn_SkipForward.ToolTip = AppModel.SkipSecString(Properties.Settings.Default.SkipForwardSec) + "進む";
            Btn_SkipBackward.ToolTip = AppModel.SkipSecString(Properties.Settings.Default.SkipForwardSec) + "戻る";

            //スキップ秒数選択メニューのチェックをセット
            CheckAllSkipForwardSecs(AppModel.SkipSecIndexNumber(Properties.Settings.Default.SkipForwardSec));
            CheckAllSkipBackwardSecs(AppModel.SkipSecIndexNumber(Properties.Settings.Default.SkipBackwardSec));
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
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            //終了処理


            Application.Current.Shutdown();
        }

        /// <summary>
        /// 動画ファイルを選択するダイアログを表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenMovie(object sender, RoutedEventArgs e)
        {
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
            ofd.Filter = "メディアファイル(*.mp4;*.mpg;*.avi;*.wmv;*.mp3;*.flac;*.wav;*.aac;*.m4a;*.ogg)|*.mp4;*.mpg;*.avi;*.wmv;*.mp3;*.flac;*.wav;*.aac;*.m4a;*.ogg|すべてのファイル(*.*)|*.*";
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
                //最後に使ったフォルダを記憶
                Properties.Settings.Default.LastMovieFolder = Path.GetDirectoryName(ofd.FileName);
                Properties.Settings.Default.Save();
                //OKボタンがクリックされたとき、選択されたファイル名を表示する
                OpenMovie(ofd.FileName);
            }
        }

        private void OpenMovie(string moviePath)
        {
            //既存ログをクリア
            AppModel.Records.Clear();

            Lb_DropHere.Visibility = Visibility.Hidden;

            //ログを読み込む
            //Ver2.0形式のファイルが存在したら読み、なければ1.0形式のファイルを探して読む。

            if (!LogReader.LoadDGGFile(moviePath))
            {
                LogReader.LoadTXTFile(moviePath);
            }

            ListBoxAutoScrollEnabled = true;

            //動画を再生
            if (_storyboard != null)
                Stop();

            //ウインドウタイトルにファイル名を表示
            Title = "動画眼 - " + Path.GetFileName(moviePath);

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
        }

        //再生・一時停止ボタン
        private void Btn_PlayPause_click(object sender, RoutedEventArgs e)
        {
            PlayPause();
        }

        //再生一時停止トグル操作
        private void PlayPause()
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
        private void MoveRelative(int sec)
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
                    TextBlock_Time.Text = Player.Clock.CurrentTime.Value.Minutes.ToString("D2") + ":" + Player.Clock.CurrentTime.Value.Seconds.ToString("D2") + "/" + Player.Clock.NaturalDuration.TimeSpan.Minutes.ToString() + ":" + Player.Clock.NaturalDuration.TimeSpan.Seconds.ToString();

                //動画が終了している場合
                if (Player.NaturalDuration.HasTimeSpan == true && Player.Clock.CurrentTime.Value.TotalMilliseconds == Player.NaturalDuration.TimeSpan.TotalMilliseconds)
                {
                    //再生停止
                    Stop();
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                PlayPause();
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.W)
            {
                MoveRelative(Properties.Settings.Default.SkipForwardSec);
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Q)
            {
                MoveRelative(Properties.Settings.Default.SkipBackwardSec * -1);
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
                if (ext == ".aac" || ext == ".m4a" || ext == ".mp3" || ext == ".wav" || ext == ".flac" || ext == ".ogg" || ext == ".mpg" || ext == ".mp4" || ext == ".mov" || ext == ".avi")
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
            Console.WriteLine("DragCancelled");
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
            TB_Search.Text = "";
            ListBox_Records.DataContext = AppModel.Records.Records;
        }

        private void TB_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            Console.WriteLine(TB_Search.Text);
            AppModel.FilteredRecords = null;
            AppModel.FilteredRecords = AppModel.Records.Records.Where(r => r.Transcript.Contains(TB_Search.Text)).ToList();
            Console.WriteLine("Count:"+AppModel.FilteredRecords.Count);
            ListBox_Records.DataContext = AppModel.FilteredRecords;
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
            Btn_SkipForward.ToolTip = (sender as MenuItem).Header + "進む";
            int sec = Properties.Settings.Default.SkipForwardSec;
            int num = 0;
            switch ((sender as MenuItem).Header)
            {
                case "3秒":
                    sec = 3;
                    num = 0;
                    break;
                case "5秒":
                    sec = 5;
                    num = 1;
                    break;
                case "10秒":
                    sec = 10;
                    num = 2;
                    break;
                case "15秒":
                    sec = 15;
                    num = 3;
                    break;
                case "30秒":
                    sec = 30;
                    num = 4;
                    break;
                case "60秒":
                    sec = 60;
                    num = 5;
                    break;
                case "90秒":
                    sec = 90;
                    num = 6;
                    break;
                case "3分":
                    sec = 180;
                    num = 7;
                    break;
                case "5分":
                    sec = 300;
                    num = 8;
                    break;
                case "10分":
                    sec = 600;
                    num = 9;
                    break;
            }
            //設定変更
            Properties.Settings.Default.SkipForwardSec = sec;
            Properties.Settings.Default.Save();
            //該当メニューにチェック
            CheckAllSkipForwardSecs(num);

        }
        private void MI_SkipBackward_Click(object sender, RoutedEventArgs e)
        {
            string btnName = (sender as MenuItem).Header.ToString().Replace("-", "");
            Btn_SkipBackward.ToolTip = btnName + "戻る";
            int sec = Properties.Settings.Default.SkipBackwardSec;
            int num = 0;
            switch (btnName)
            {
                case "3秒":
                    sec = 3;
                    num = 0;
                    break;
                case "5秒":
                    sec = 5;
                    num = 1;
                    break;
                case "10秒":
                    sec = 10;
                    num = 2;
                    break;
                case "15秒":
                    sec = 15;
                    num = 3;
                    break;
                case "30秒":
                    sec = 30;
                    num = 4;
                    break;
                case "60秒":
                    sec = 60;
                    num = 5;
                    break;
                case "90秒":
                    sec = 90;
                    num = 6;
                    break;
                case "3分":
                    sec = 180;
                    num = 7;
                    break;
                case "5分":
                    sec = 300;
                    num = 8;
                    break;
                case "10分":
                    sec = 600;
                    num = 9;
                    break;
            }
            //設定変更
            Properties.Settings.Default.SkipBackwardSec = sec;
            Properties.Settings.Default.Save();
            //該当メニューにチェック
            CheckAllSkipBackwardSecs(num);

        }

        //スキップ秒数のメニューとボタンコンテクストメニューのチェックを一旦全部外し指定行目をチェック
        private void CheckAllSkipForwardSecs(int index)
        {
            //全部オフ
            foreach (MenuItem mi in MI_SkipSecFoward.Items)
            {
                mi.IsChecked = false;
            }
            foreach (MenuItem mi in Btn_SkipForward.ContextMenu.Items)
            {
                mi.IsChecked = false;
            }
            //指定番目をチェック
            MenuItem onMenu = MI_SkipSecFoward.Items[index] as MenuItem;
            onMenu.IsChecked = true;
            MenuItem onButton = Btn_SkipForward.ContextMenu.Items[index] as MenuItem;
            onButton.IsChecked = true;

        }
        private void CheckAllSkipBackwardSecs(int index)
        {
            //全部オフ
            foreach (MenuItem mi in MI_SkipSecBackword.Items)
            {
                mi.IsChecked = false;
            }
            foreach (MenuItem mi in Btn_SkipBackward.ContextMenu.Items)
            {
                mi.IsChecked = false;
            }
            //指定番目をチェック
            MenuItem onMenu = MI_SkipSecBackword.Items[index] as MenuItem;
            onMenu.IsChecked = true;
            MenuItem onButton = Btn_SkipBackward.ContextMenu.Items[index] as MenuItem;
            onButton.IsChecked = true;

        }

        #endregion

    }
}
