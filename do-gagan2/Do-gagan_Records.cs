using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
        /// <summary>
        /// 全レコードをタブ区切り形式テキストを生成して返す
        /// </summary>
        /// <returns>タグ区切りテキスト</returns>
        public string ToString(bool WithSpeakerLabel = false, bool WithConfidence = false, FileFormatVersion version = FileFormatVersion.Type2)
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
                        result += span.ToString(@"hh\:mm\:ss") + "\t" + r.Transcript;
                        break;

                    default:
                        //動画眼2.x形式（タイムスタンプそのまま、話者、信頼度フィールド対応）
                        result += r.TimeStamp + "\t";
                        if (WithSpeakerLabel)
                        {
                            //話者ラベル、「」あり
                            result += "\t" + r.Speaker + " 「" + r.Transcript + "」";
                        }
                        else
                        {
                            result += "\t" + r.Transcript;
                        }
                        if (WithConfidence)
                        {
                            //信頼度あり
                            result += "\t" + r.Confidence;
                        }
                        break;
                }
                result += "\r";
            }
            return result;
        }


    }

    /// <summary>
    /// 動画眼形式データの個別レコード
    /// </summary>
    public class Dogagan_Record: INotifyPropertyChanged
    {
        public float TimeStamp { get; set; }
        public string Speaker { get; set; }
        public string Transcript { get; set; }
        public double? Confidence { get; set; }

        //データバインディングの更新に必要
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Renew()
        {
            OnPropertyChanged("TimeStamp");
            OnPropertyChanged("Transcript");
        }

    }

    public enum FileFormatVersion
    {
        Type1,
        Type2
    }

}
