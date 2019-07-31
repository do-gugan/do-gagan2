﻿using System;
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

        #region スキップ秒数周り
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
                case 15:
                    result = "15秒";
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

        public static string SkipSecBtnLabel(int sec)
        {
            string result = "";
            switch (sec)
            {
                case 3:
                    result = "3sec";
                    break;
                case 5:
                    result = "5sec";
                    break;
                case 10:
                    result = "10sec";
                    break;
                case 15:
                    result = "15sec";
                    break;
                case 30:
                    result = "30sec";
                    break;
                case 60:
                    result = "60sec";
                    break;
                case 90:
                    result = "90sec";
                    break;
                case 180:
                    result = "3min";
                    break;
                case 300:
                    result = "5min";
                    break;
                case 600:
                    result = "10min";
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
                case 15:
                    result = 3;
                    break;
                case 30:
                    result = 4;
                    break;
                case 60:
                    result = 5;
                    break;
                case 90:
                    result = 6;
                    break;
                case 180:
                    result = 7;
                    break;
                case 300:
                    result = 8;
                    break;
                case 600:
                    result = 9;
                    break;
            }
            return result;
        }

    /// <summary>
        /// "3秒"などのメニュー文字列から秒数を返す
        /// </summary>
        public static int SkipSecIndexFromBtnName(string btnName)
        {
            int sec = 0;
            switch (btnName)
            {
                case "3秒":
                    sec = 3;
                    break;
                case "5秒":
                    sec = 5;
                    break;
                case "10秒":
                    sec = 10;
                    break;
                case "15秒":
                    sec = 15;
                    break;
                case "30秒":
                    sec = 30;
                    break;
                case "60秒":
                    sec = 60;
                    break;
                case "90秒":
                    sec = 90;
                    break;
                case "3分":
                    sec = 180;
                    break;
                case "5分":
                    sec = 300;
                    break;
                case "10分":
                    sec = 600;
                    break;
            }
            return sec;
        }
        #endregion
    }
}
