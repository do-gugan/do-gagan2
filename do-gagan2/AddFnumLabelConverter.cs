using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace do_gagan2
{
    public class AddFnumLabelConverter: IValueConverter
    {
        /// <summary>
        /// メイン画面のF1～5のソフトキー表示を設定にバインドする際、「F1: 」といったプレフィクスをつけるコンバーター
        /// </summary>
        /// <param name="value">キー設定文字列（設定から）</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">F番号</param>
        /// <param name="culture"></param>
        /// <returns></returns>

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {

            //見辛いので「$t」と「$c」は省略する
            value = value.ToString().Replace("$t", "").Replace("$c", "");
            return $"F{parameter}: {value}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
    }
}
