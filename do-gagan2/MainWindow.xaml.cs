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
using System.Windows.Shapes;
using System.IO;

namespace do_gagan2
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
            ofd.FilterIndex = 2;
            //タイトルを設定する
            ofd.Title = "動画/音声ファイルを選択してください";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd.RestoreDirectory = true;

            //ダイアログを表示する
            if (ofd.ShowDialog() == true)
            {
                //最後に使ったフォルダを記憶
                Properties.Settings.Default.LastMovieFolder = System.IO.Path.GetDirectoryName(ofd.FileName);
                Properties.Settings.Default.Save();
                //OKボタンがクリックされたとき、選択されたファイル名を表示する
                OpenMovie(ofd.FileName);
            }
        }

        private void OpenMovie(string moviePath)
        {
            Player.Source = new Uri(moviePath, UriKind.Absolute);
            Player.LoadedBehavior = MediaState.Manual;
            Player.Play();

        }

        //再生・一時停止ボタン
        private void Btn_PlayPause(object sender, RoutedEventArgs e)
        {
            PlayPause();
        }

        //再生一時停止操作
        private void PlayPause()
        {
        }

    }
}
