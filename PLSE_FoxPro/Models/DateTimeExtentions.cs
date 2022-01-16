using System;
using System.Collections.Generic;
using System.Text;

namespace PLSE_FoxPro.Models
{
    public static class DateTimeExtentions
    {
        /// <summary>
        /// Переводит разницу между текущей датой в строковое представление, выраженное в стиле 'дней/месяцем/лет назад'
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string TimeAgo(this DateTime? date)
        {
            if (date == null) return null;
            var days = (DateTime.Now - date.Value).Days;
            if (days < 0) return "в будущем";
            if (days < 30) return $"{days} {DeclinationByLastDigit(days, "день", "дня", "дней")} назад";
            if (days < 366) return $"{days / 30} {DeclinationByLastDigit(days / 30, "месяц", "месяца", "месяцев")} назад";
            return $"{days / 365} {DeclinationByLastDigit(days / 365, "год", "года", "лет")} назад";
        }

        /// <summary>
        /// Возвращает один из своих параметров в зависимости от завершающих цифр параметра <paramref name="val"/>
        /// </summary>
        /// <example></example>
        /// <param name="val"></param>
        /// <param name="undecl">Если оканчивается на 1</param>
        /// <param name="single">Если оканчивается на 2-4, но не 12-14</param>
        /// <param name="plural">В остальных случаях</param>
        /// <returns><paramref name="undecl"/> если оканчивается на 1, <paramref name="single"/> - оканчивается на 2-4, кроме 12-14,
        /// <paramref name="plural"/> - остальные случаи</returns>
        public static string DeclinationByLastDigit(int val, string undecl, string single, string plural)
        {
            if (val % 10 == 1) return undecl;
            if (val % 100 > 4 && val % 100 < 21) return plural;
            if (val % 10 > 1 && val % 10 < 5) return single;
            else return plural;
        }
    }
}
