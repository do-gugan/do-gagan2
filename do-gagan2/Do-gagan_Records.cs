using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace do_gagan2
{
    class Do_gagan_Records
    {
        List<Dogagan_Record> Records = new List<Dogagan_Record>();

        /// <summary>
        /// 全レコードをタブ区切り形式テキストを生成して返す
        /// </summary>
        /// <returns>タグ区切りテキスト</returns>
        public string ToString(bool WithSpeakerLabel = false, bool WithConfidence = false, FileFormatVersion version = FileFormatVersion.Type2)
        {
            string result = "";
            Records.OrderBy(r => r.TimeStamp);
            foreach (var r in Records)
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
    public class Dogagan_Record
    {
        public float TimeStamp;
        public string Speaker;
        public string Transcript;
        public double? Confidence;
    }

    public enum FileFormatVersion
    {
        Type1,
        Type2
    }

}
