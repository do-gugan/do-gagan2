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
    /// Window_Replace.xaml の相互作用ロジック
    /// </summary>
    public partial class Window_Replace : Window
    {
        public Window_Replace()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 置換実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReplace_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TB_Search.Text))
            {
                MessageBox.Show("「検索」欄は空にできません。");
                return;
            }
            AppModel.Records.Replace(TB_Search.Text, TB_Replace.Text);
            Close();
        }

        /// <summary>
        /// キャンセル（閉じる）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Clicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //候補マッチ数を更新
        private void TB_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            Lbl_MatchCount.Content = "該当数: " + AppModel.Records.GetMatchPlaces(TB_Search.Text);
        }
    }
}
