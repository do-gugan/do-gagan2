﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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


        #region 入力値のチェック
        private static readonly Regex _regex = new Regex("[^0-9.]"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        //キー入力時点で数字以外はハネる
        private void TB_AutoSaveInterval_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsTextAllowed(e.Text))
            {
                e.Handled = true;
            }
        }

        //入力完了時点で指定範囲外をハネる
        private void TB_AutoSaveInterval_TextChanged(object sender, TextChangedEventArgs e)
        {
            int i = 0;
            if (int.TryParse(TB_AutoSaveInterval.Text, out i))
            {
                if (i>1 && i< 100)
                {
                    e.Handled = true;
                } else
                {
                    MessageBox.Show("自動保存間隔は1-99分までの範囲で指定してください。");
                    e.Handled = false;
                    Properties.Settings.Default.AutoSaveInterval = 5;
                }
            } else
            {
                MessageBox.Show("自動保存間隔は数値で指定してください。");
                e.Handled = false;
                Properties.Settings.Default.AutoSaveInterval = 5;
            }
            Properties.Settings.Default.Save();
        }

        //Shift押下時のジャンプ倍率をチェック（数値のみ）
        private void TB_MultiplyFactorForSkipWithShiftKey_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsTextAllowed(e.Text))
            {
                e.Handled = true;
            }
        }
        //入力完了時点で指定範囲外をハネる
        private void TB_MultiplyFactorForSkipWithShiftKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            double d = 0;
            if (double.TryParse(TB_MultiplyFactorForSkipWithShiftKey.Text, out d))
            {
                if (d > 0.1 && d < 10)
                {
                    e.Handled = true;
                }
                else
                {
                    MessageBox.Show("ジャンプ倍率は0.1-10倍までの範囲で指定してください。");
                    e.Handled = false;
                    Properties.Settings.Default.MultiplyFactorForSkipWithShiftKey = 2;
                }
            }
            else
            {
                MessageBox.Show("ジャンプ倍率は数値で指定してください。");
                e.Handled = false;
                Properties.Settings.Default.MultiplyFactorForSkipWithShiftKey = 5;
            }
            Properties.Settings.Default.Save();

        }
        #endregion
    }
}
