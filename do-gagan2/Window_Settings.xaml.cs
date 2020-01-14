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
using System.Windows.Shapes;

namespace do_gagan2
{
    /// <summary>
    /// Window_Settings.xaml の相互作用ロジック
    /// </summary>
    public partial class Window_Settings : Window
    {
        public Window_Settings()
        {
            InitializeComponent();
        }

        //保存して閉じる
        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            Close();
        }

        //保存しないで閉じる
        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {

            Close();
        }

        //ファンクション定型文を初期化する
        private void Reset_FunctionTemplates_Click(object sender, RoutedEventArgs e)
        {
            TB_F1.Text = "タスク開始:$t$c";
            TB_F2.Text = "参加者「$t$c」";
            TB_F3.Text = "進行役「$t$c」";
            TB_F4.Text = "見所！:$t$c";
            TB_F5.Text = "タスク完了:$t$c";
        }
    }
}
