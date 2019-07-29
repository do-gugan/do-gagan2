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

        public MainWindow()
        {
            InitializeComponent();
            AppModel.Records = new Do_gagan_Records();
        }

        #region 基本再生操作

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

            //ログを読み込む
            //Ver2.0形式のファイルが存在したら読み、なければ1.0形式のファイルを探して読む。

            if (!LogReader.LoadDGGFile(moviePath))
            {
                LogReader.LoadTXTFile(moviePath);
            }
                
            
            //動画を再生
            if (_storyboard != null)
                Stop();

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
            MoveRelative(Properties.Settings.Default.SkipForwardSec * -1);
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
                MoveRelative(Properties.Settings.Default.SkipBackwordSec * -1);
            }
        }
        #endregion

        #region スライダー関連
        //動画を開いた時にスライダーの最大値を動画の長さにあわせる
        private void Element_MediaOpened(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Duraton:" + Player.NaturalDuration.TimeSpan.TotalSeconds + "sec");
            Slider_Time.Maximum = Player.NaturalDuration.TimeSpan.TotalMilliseconds;
        }
        //スライダーを更新
        private void MediaTimeChanged(object sender, EventArgs e)
        {
            Console.WriteLine("tick");
            Slider_Time.Value = Player.Position.TotalMilliseconds;
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


    }
}
