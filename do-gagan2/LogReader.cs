using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using ByteDev.Subtitles.SubRip;

namespace do_gagan2
{

    /// <summary>
    /// ログの読み込みを行う静的オブジェクト
    /// </summary>
    static class LogReader
    {
        /// <summary>
        /// 動画眼2.0形式が存在すれば読み込む
        /// </summary>
        /// <param name="moviePath">動画のフルパス</param>
        /// <returns>読み込み成功したらtrue</returns>
        /// ファイル形式（タブ区切りUTF-8テキスト）
        /// 0.0[TAB]発話、メモ[TAB]話者フラグ[TAB]信頼度
        /// 前2フィールドのみ必須
        public static bool SearchDGGFile(string moviePath)
        {
            string logPath = Path.Combine(Path.GetDirectoryName(moviePath), Path.GetFileNameWithoutExtension(moviePath) + ".dggn.txt");
            Console.WriteLine("Search:" + logPath);
            if (!File.Exists(logPath))
            {
                //見つからなければfalseを返して終了
                Console.WriteLine(".dggnファイル無し");
                return false;
            }
            return LoadDGGFile(logPath);
        }

        /// <summary>
        /// 指定のログファイルを読み込む
        /// </summary>
        /// <param name="logPath"></param>
        /// <returns>読み込み成功したらtrue</returns>
        public static bool LoadDGGFile(string logPath)
        {
            AppModel.CurrentLogFilePath = logPath;
            //読み込み処理
            string line = "";
            using (StreamReader sr = new StreamReader(logPath, Encoding.GetEncoding("UTF-8")))
            {

                while ((line = sr.ReadLine()) != null)
                {
                    if (!String.IsNullOrEmpty(line))
                    {
                        string[] fields = line.Split('\t');
                        Dogagan_Record rec = new Dogagan_Record();

                        //タイムコードを変換＆整合性チェック
                        float ts;
                        bool isFloat = float.TryParse(fields[0], out ts);
                        if (!isFloat)
                        {
                            MessageBox.Show("タイムコード形式が不正な行があり、ログ読み込みを中断します。\r" + line);
                            return false;
                        }
                        else
                        {
                            rec.TimeStamp = ts;
                        }
                        rec.Transcript = fields[1];
                        rec.Speaker = fields[2];
                        AppModel.Records.Add(rec);
                        rec.Renew();
                    }

                }

                //foreach(var rec in AppModel.Records.Records)
                //{
                //    Console.WriteLine("tc:"+rec.TimeStamp + " text:"+rec.Transcript);
                //}
                AppModel.MainWindow.ListBox_Records.DataContext = AppModel.Records.Records;
                AppModel.MainWindow.MI_Replace.IsEnabled = true;
            }
            AppModel.CurrentLogFilePath = logPath;
            return true;

        }

        /// <summary>
        /// 動画眼1.0形式が存在すれば読み込む
        /// </summary>
        /// <param name="moviePath">動画のフルパス</param>
        /// <returns>読み込み成功したらtrue</returns>
        /// ファイル形式（タブ区切りSJISテキスト）
        /// 00:00:00[TAB]発話、メモ
        public static bool SearchTXTFile(string moviePath)
        {
            string logPath = Path.Combine(Path.GetDirectoryName(moviePath), Path.GetFileNameWithoutExtension(moviePath) + ".txt");
            Console.WriteLine("Search:" + logPath);
            if (!File.Exists(logPath))
            {
                //見つからなければfalseを返して終了
                Console.WriteLine(".txtファイル無し");
                return false;
            }
            return LoadTXTFile(logPath);
        }

        public static bool LoadTXTFile(string logPath)
        {

            //読み込み処理
            string line = "";
            using (StreamReader sr = new StreamReader(logPath, Encoding.GetEncoding("Shift_JIS")))
            {

                while ((line = sr.ReadLine()) != null)
                {
                    if (!String.IsNullOrEmpty(line))
                    {
                        string[] fields = line.Split('\t');
                        //Console.WriteLine("code:" + fields[0] + " text:" + fields[1]);
                        Dogagan_Record rec = new Dogagan_Record();
                        string[] tc = fields[0].Split(':');

                        //タイムコードを変換＆整合性チェック
                        int hour, min, sec;
                        bool isIntTc0 = int.TryParse(tc[0], out hour);
                        bool isIntTc1 = int.TryParse(tc[1], out min);
                        bool isIntTc2 = int.TryParse(tc[2], out sec);
                        if (!isIntTc0 || !isIntTc1 || !isIntTc2)
                        {
                            MessageBox.Show("タイムコード形式が不正な行があり、ログ読み込みを中断します。\r" + line);
                            return false;
                        }
                        else
                        {
                            rec.TimeStamp = hour * 3600 + min * 60 + sec;
                        }
                        rec.Transcript = fields[1];
                        AppModel.Records.Add(rec);
                        rec.Renew();
                    }

                }

                //foreach(var rec in AppModel.Records.Records)
                //{
                //    Console.WriteLine("tc:"+rec.TimeStamp + " text:"+rec.Transcript);
                //}
                AppModel.MainWindow.ListBox_Records.DataContext = AppModel.Records.Records;
                AppModel.MainWindow.MI_Replace.IsEnabled = true;
            }
            AppModel.CurrentLogFilePath = logPath;
            return true;

        }



        /// <summary>
        /// srt形式字幕ファイルが存在すれば読み込む
        /// </summary>
        /// <param name="moviePath">動画のフルパス</param>
        /// <returns>読み込み成功したらtrue</returns>
        /// srtファイル形式（UTF-8テキスト）
        /// 1
        /// 00:30:22,123 --> 00:30:30,456
        /// (字幕テキスト1)
        /// 
        /// 2
        /// 01:10:34,156 --> 01:10:38,345
        /// (字幕テキスト2)
        /// 
        public static bool SearchSRTFile(string moviePath)
        {
            string logPath = Path.Combine(Path.GetDirectoryName(moviePath), Path.GetFileNameWithoutExtension(moviePath) + ".srt");
            Console.WriteLine("Search:" + logPath);
            if (!File.Exists(logPath))
            {
                //見つからなければfalseを返して終了
                Console.WriteLine(".srtファイル無し");
                return false;
            }
            return LoadSRTFile(logPath);
        }

        /// <summary>
        /// 指定のログファイルを読み込む
        /// </summary>
        /// <param name="logPath"></param>
        /// <returns>読み込み成功したらtrue</returns>
        public static bool LoadSRTFile(string logPath)
        {
            Console.WriteLine("srtファイル読み込み開始");

            //改行コードがCRLFでないと正しくパースできないので、事前にチェックして変換、上書きする
            if (ReplaceLF2CRLF(logPath) == false) return false; //falseなら中断

            //読み込み処理
            SubRipFile subs = SubRipFile.Load(logPath);

            foreach (SubRipEntry sub in subs.Entries)
            {
                //Console.WriteLine(sub.OrderId);         //通し番号
                //Console.WriteLine(sub.Duration.Start);  //INタイムコード
                //Console.WriteLine(sub.Duration.End);    //OUTタイムコード
                //Console.WriteLine(sub.Text);            //字幕テキスト
                //Console.WriteLine(sub.Duration.Start.ToString());

                Dogagan_Record rec = new Dogagan_Record();
                char[] delimiterChars = { ':', ',' };
                string[] tc = sub.Duration.Start.ToString().Split(delimiterChars);

                //タイムコードを変換＆整合性チェック
                int hour, min, sec;
                bool isIntTc0 = int.TryParse(tc[0], out hour);
                bool isIntTc1 = int.TryParse(tc[1], out min);
                bool isIntTc2 = int.TryParse(tc[2], out sec);
                if (!isIntTc0 || !isIntTc1 || !isIntTc2)
                {
                    MessageBox.Show("タイムコード形式が不正な行があり、ログ読み込みを中断します。\r" + sub.ToString());
                    return false;
                }
                else
                {
                    rec.TimeStamp = hour * 3600 + min * 60 + sec;
                }
                rec.Transcript = sub.Text;
                AppModel.Records.Add(rec);
                rec.Renew();

            }

            AppModel.MainWindow.ListBox_Records.DataContext = AppModel.Records.Records;
            AppModel.MainWindow.MI_Replace.IsEnabled = true;

            AppModel.CurrentLogFilePath = logPath;
            return true;
        }

        /// <summary>
        /// テキストファイルの改行コードをCRLFに変換する
        /// </summary>
        /// <param name="logPath"></param>
        public static bool ReplaceLF2CRLF(string filePath)
        {
            string text = "";
            using (var reader = new StreamReader(filePath))
            {
                text = reader.ReadToEnd();
            }

            if (text.Contains("\r\n"))
            {
                Console.WriteLine("改行コードがCRLF"); //問題なし
                return true;
            } else
            {
                Console.WriteLine("改行コードがCRLFではない。要変換");
                string fname = Path.GetFileName(filePath);
                MessageBoxResult result = MessageBox.Show("SRTファイル「" + fname + "」が見つかりましたが改行コードをCRLFに変換する必要があります。変換して上書きしてよろしいですか？", "書き換え確認", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                switch (result)
                {
                    case MessageBoxResult.OK:
                        ;
                        File.WriteAllText(filePath, text.Replace("\n", "\r\n"));
                        return true;
                    default:
                        return false;
                }
            }
        }
    }
}

