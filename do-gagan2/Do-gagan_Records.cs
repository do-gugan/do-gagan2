using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualBasic;

namespace do_gagan2
{
    class Do_gagan_Records
    {
        private ObservableCollection<Dogagan_Record> _records = new ObservableCollection<Dogagan_Record>();

        public ObservableCollection<Dogagan_Record> Records { get { return _records; } }

        public void Clear()
        {
            _records.Clear();
        }

        public void Add(Dogagan_Record rec)
        {
            _records.Add(rec);
            Console.WriteLine("Speaker:"+rec.Speaker + ":"+rec.Transcript);
        }
        /// <summary>
        /// 全レコードをタブ区切り形式テキストを生成して返す
        /// </summary>
        /// <returns>タグ区切りテキスト</returns>
        public string ToString(bool WithSpeakerLabel = false, FileFormatVersion version = FileFormatVersion.Type2)
        {
            string result = "";
            _records.OrderBy(r => r.TimeStamp);
            foreach (var r in _records)
            {
                switch (version)
                {
                    case FileFormatVersion.Type1:
                        //動画眼1.x形式（タイムスタンプが00:00:00形式）
                        var span = new TimeSpan(0, 0, (int)r.TimeStamp);
                        result += span.ToString(@"hh\:mm\:ss") + "\t";
                        if (WithSpeakerLabel == true)
                        {
                            result += r.Speaker + " 「" + r.Transcript + "」";
                        }
                        else
                        {
                            result += r.Transcript;
                        }
                        break;

                    default:
                        //動画眼2.x形式（タイムスタンプそのまま、話者フィールド対応）
                        result += r.TimeStamp;
                        if (WithSpeakerLabel)
                        {
                            //話者ラベル、「」あり
                            result += "\t" + r.Speaker + " 「" + r.Transcript + "」";
                        }
                        else
                        {
                            result += "\t" + r.Transcript;
                        }
                        result += "\t" + r.Speaker;
                        break;
                }
                result += "\r\n";
            }
            return result;
        }

        public void Replace(string from, string to)
        {
            //_records.ToList().ForEach(r => r.Transcript.Replace(from, to));
            int replacedSum = 0;
            foreach(var rec in _records)
            {
                int replaced = rec.ReplaceAndRenew(from, to);
                replacedSum += replaced;
            }
            MessageBox.Show(replacedSum + "ヵ所置換しました");
            if (replacedSum > 0) { AppModel.IsCurrentFileDirty = true; }

        }

        public int GetMatchPlaces(string search)
        {
            if (String.IsNullOrEmpty(search))
            {
                return 0;
            } else
            {
                return _records.Sum(r => r.GetMatchPlaces(search));
            }
        }

        /// <summary>
        /// レコードを削除
        /// </summary>
        /// <param name="rec">削除したいレコード</param>
        public void Delete(Dogagan_Record item)
        {
            _records.Remove(item);
            AppModel.IsCurrentFileDirty = true;
        }      

    }

    #region 個別レコードオブジェクト

    /// <summary>
    /// 動画眼形式データの個別レコード
    /// </summary>
    public class Dogagan_Record: INotifyPropertyChanged
    {
        public float TimeStamp { get; set; }
        public string Speaker { get; set; }
        public string Transcript { get; set; }
        public double? Confidence { get; set; }
        public string TimeStampInHhMmSs {
            get {
                TimeSpan ts = new TimeSpan(0,0,(int)TimeStamp);
                
                return (ts.Hours * 60 + ts.Minutes).ToString("D2") + ":" + ts.Seconds.ToString("D2");
            }
        }
        public string SpeakerColor {
            get {
                switch(Speaker){
                    case "0":
                        return "#CCFFFF";
                    case "1":
                        return "#FFD5EC";
                    case "2":
                        return "#CCFFCC";
                    case "3":
                        return "#FFCCFF";
                    case "4":
                        return "#FFFFCC";
                    case "5":
                        return "#FFDBC9";
                    case "6":
                        return "#E6FFE9";
                    case "7":
                        return "#EAD9FF";
                    default:
                        return "#CCFFFF";
                }
            }
        }

        //データバインディングの更新に必要
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// レコード内にある検索語の数を返す
        /// </summary>
        /// <param name="search">検索語</param>
        /// <returns>Transcript内でマッチした回数</returns>
        public int GetMatchPlaces(string search)
        {
            //半角文字を全角に変換してから検索
            //string transcriptWide = Strings.StrConv(Transcript, VbStrConv.Wide, 0x411);
            //string searchWide = Strings.StrConv(search, VbStrConv.Wide, 0x411);
            //MatchCollection matche = Regex.Matches(transcriptWide, searchWide);
            MatchCollection matche = Regex.Matches(Transcript, search);
            return matche.Count;
        }
        public void Renew()
        {
            OnPropertyChanged("TimeStamp");
            OnPropertyChanged("Transcript");
            OnPropertyChanged("SpeakerColor");
        }
        public int ReplaceAndRenew(string from, string to)
        {
            int match = GetMatchPlaces(from);
            if (match > 0)
            {
                Transcript = Transcript.Replace(from, to);
                Renew();
            }
            return match;
        }

    }
    #endregion

    public enum FileFormatVersion
    {
        Type1,
        Type2
    }

}
