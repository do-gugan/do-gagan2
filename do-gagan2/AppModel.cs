using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace do_gagan2
{
    //アプリ全体からアクセスできる情報、オブジェクトの集積用静的オブジェクト
    static class AppModel
    {
        public static Do_gagan_Records Records;
        public static List<Dogagan_Record> FilteredRecords;
        public static MainWindow MainWindow;

        //使い方ガイドURLを開く
        public static void GoUsageGuide()
        {
            System.Diagnostics.Process.Start("https://do-gugan.com/tools/do-gagan2/usage.html");
        }

        //バージョン情報を開く
        public static void ShowVersionInfo()
        {
            MessageBox.Show("動画眼 Version " + GetVersion() + "\r\r©2019 Do-gugan");
        }
        public static string GetVersion()
        {
            System.Diagnostics.FileVersionInfo ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return ver.FileMajorPart + "." + ver.FileMinorPart + "." + ver.FileBuildPart;
        }

        //設定にあるスキップ秒数から文字列表記を返す
        public static string SkipSecString(int sec)
        {
            string result = "";
            switch (sec)
            {
                case 3:
                    result = "3秒";
                    break;
                case 5:
                    result = "5秒";
                    break;
                case 10:
                    result = "10秒";
                    break;
                case 30:
                    result = "30秒";
                    break;
                case 60:
                    result = "60秒";
                    break;
                case 90:
                    result = "90秒";
                    break;
                case 180:
                    result = "3分";
                    break;
                case 300:
                    result = "5分";
                    break;
                case 600:
                    result = "10分";
                    break;
            }
            return result;
    }

        //設定にあるスキップ秒数からメニューのインデックス番号
        public static int SkipSecIndexNumber(int sec)
        {
            int result = 0;
            switch (sec)
            {
                case 3:
                    result = 0;
                    break;
                case 5:
                    result = 1;
                    break;
                case 10:
                    result = 2;
                    break;
                case 30:
                    result = 3;
                    break;
                case 60:
                    result = 4;
                    break;
                case 90:
                    result = 5;
                    break;
                case 180:
                    result = 6;
                    break;
                case 300:
                    result = 7;
                    break;
                case 600:
                    result = 8;
                    break;
            }
            return result;
        }

    }
}
