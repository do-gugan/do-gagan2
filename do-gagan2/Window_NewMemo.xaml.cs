using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace do_gagan2
{
    /// <summary>
    /// Window_NewMemo.xaml の相互作用ロジック
    /// </summary>
    public partial class Window_NewMemo : Window, INotifyPropertyChanged
    {
        public double LockedPosition { get; set; } = 0.0;
        public int SpeakerID { get; set; } = 0;

        public Window_NewMemo(double position, int speaker)
        {
            InitializeComponent();

            //保存位置を復元
            if (Properties.Settings.Default.MemoWindowPositionLeft != 0 && Properties.Settings.Default.MemoWindowPositionTop != 0)
            {
                RecoverWindowBounds();
                //Console.WriteLine("Memo Resuming Positon: Left=" + Left + " Top=" + Top);
            }

            this.DataContext = this;
            LockedPosition = position;
            SpeakerID = speaker;
            OnPropertyChanged("LockedPosition");
            OnPropertyChanged("SpeakerID");
            TB_Memo.Focus();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //マウスボタン押下状態でなければ何もしない  
            if (e.ButtonState != MouseButtonState.Pressed) return;

            this.DragMove();
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                Btn_F1_Click(null, null);
            }
            if (e.Key == Key.F2)
            {
                Btn_F2_Click(null, null);
            }
            if (e.Key == Key.F3)
            {
                Btn_F3_Click(null, null);
            }
            if (e.Key == Key.F4)
            {
                Btn_F4_Click(null, null);
            }
            if (e.Key == Key.F5)
            {
                Btn_F5_Click(null, null);
            }
            if (e.Key == Key.Enter)
            {
                Btn_Save_Click(null, null);
            }
        }

        private void Btn_LoclOn_Decrease1sec_Click(object sender, RoutedEventArgs e)
        {
            if (LockedPosition -1.0 < 0)
            {
                LockedPosition = 0.0;
            } else
            {
                LockedPosition -= 1.0;
            }
            OnPropertyChanged("LockedPosition");
            AppModel.MainWindow.MoveAbsolute((int)LockedPosition); //再生位置を移動
            TB_Memo.Focus();
        }

        private void Btn_LoclOn_Increase1sec_Click(object sender, RoutedEventArgs e)
        {
            if (LockedPosition + 1.0 < AppModel.MainWindow.GetMediaDuration())
            {
                LockedPosition += 1.0;

            } else
            {
                LockedPosition = AppModel.MainWindow.GetMediaDuration();
            }

            OnPropertyChanged("LockedPosition");
            AppModel.MainWindow.MoveAbsolute((int)LockedPosition); //再生位置を移動
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
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //ウインドウ位置を保存
            SaveWindowBounds();

            //base.OnClosing(e);
        }

        /// <summary>
        /// ウィンドウの位置・サイズを保存します。
        /// </summary>
        void SaveWindowBounds()
        {
            //Console.WriteLine("Memo Closing: Left=" + Left + " Top=" + Top);

            var settings = Properties.Settings.Default;
            settings.MemoWindowPositionLeft = Left;
            settings.MemoWindowPositionTop = Top;
            settings.Save();
        }

        /// <summary>
        /// ウィンドウの位置・サイズを復元します。
        /// </summary>
        private void RecoverWindowBounds()
        {
            var settings = Properties.Settings.Default;
            // 左
            if (settings.MemoWindowPositionLeft >= 0 &&
                (settings.MemoWindowPositionLeft + Width) < SystemParameters.VirtualScreenWidth)
            { Left = settings.MemoWindowPositionLeft; }
            // 上
            if (settings.MemoWindowPositionTop >= 0 &&
                (settings.MemoWindowPositionTop + Height) < SystemParameters.VirtualScreenHeight)
            { Top = settings.MemoWindowPositionTop; }
        }
    }
}
