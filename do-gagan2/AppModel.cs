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

    }
}
