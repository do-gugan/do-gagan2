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
    /// Window_NewMemo.xaml の相互作用ロジック
    /// </summary>
    public partial class Window_NewMemo : Window
    {
        public Window_NewMemo()
        {
            InitializeComponent();
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

        }
        private void Btn_F2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_F3_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Btn_F4_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_F5_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
