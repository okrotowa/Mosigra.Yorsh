
//1"IQ"iq_bg_DF.jpg"
//2"Выпить"выпить_bg_DF.jpg"
//3"Координация"координация_bg_DF.jpg"
//4"Неприятное"неприятное_bg_DF.jpg"
//5"Обмен"обмен_bg_DF.jpg"
//6"Приколы"приколы_bg_DF.jpg"
//7"Рисунок"рисунок_bg_DF.jpg"
//8"Скороговорка"скороговорка_bg_DF.jpg"
//9"СМС"смс_bg_DF.jpg"
//10"Тост"тост_bg_DF.jpg"
//11"Физическое"физическое_bg_DF.jpg"
//12"Фото"фото_bg_DF.jpg"
//13"медведь"медведь_bg_DF.jpg"
//14"IQ"iq_bg_NY.jpg"
//15"Выпить"выпить_bg_NY.jpg"
//16"Координация"координация_bg_NY.jpg"
//17"Неприятное"неприятное_bg_NY.jpg"
//18"Обмен"обмен_bg_NY.jpg"
//19"Приколы"приколы_bg_NY.jpg"
//20"Рисунок"рисунок_bg_NY.jpg"
//21"Скороговорка"скороговорка_bg_NY.jpg"
//22"СМС"смс_bg_NY.jpg"
//23"Тост"тост_bg_NY.jpg"
//24"Физическое"физическое_bg_NY.jpg"
//25"Фото"фото_bg_NY.jpg"
//26"медведь"медведь_bg_NY.jpg"

using System.Collections.Generic;
using System.IO;

namespace Yorsh.Transliteration
{
    static class Program
    {
        static void Main(string[] args)
        {
            IList<string> _list = new string[]
            {
             "iq_bg_DF.jpg",
            "выпить_bg_DF.jpg",
            "координация_bg_DF.jpg",
            "неприятное_bg_DF.jpg",
            "обмен_bg_DF.jpg",
            "приколы_bg_DF.jpg",
            "рисунок_bg_DF.jpg",
            "скороговорка_bg_DF.jpg",
            "смс_bg_DF.jpg",
            "тост_bg_DF.jpg",
            "физическое_bg_DF.jpg",
            "фото_bg_DF.jpg",
            "медведь_bg_DF.jpg",
            "iq_bg_NY.jpg",
            "выпить_bg_NY.jpg",
            "координация_bg_NY.jpg",
            "неприятное_bg_NY.jpg",
            "обмен_bg_NY.jpg",
            "приколы_bg_NY.jpg",
            "рисунок_bg_NY.jpg",
            "скороговорка_bg_NY.jpg",
            "смс_bg_NY.jpg",
            "тост_bg_NY.jpg",
            "физическое_bg_NY.jpg",
            "фото_bg_NY.jpg",
            "медведь_bg_NY.jpg",
            };
            using (var stream = new StreamWriter(File.Create("file.txt")))
            {
                foreach (var item in _list)
                {
                    var newItem = UnidecodeSharpFork.Unidecoder.Unidecode(item);
                    stream.WriteLine(newItem);
                }
                stream.Close();
            }
        }
    }
}
