using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PLSE_FoxPro.Models
{
    public static class StringExtentions
    {
        /// <summary>
        /// Переворачивает строку
        /// </summary>
        /// <returns>Новая перевернутая строка</returns>
        static public string StrReverse(this string str) => new string(str?.Reverse().ToArray());
        /// <summary>
        /// Ищет первую букву в исходной строке и переводит ее в верхний регистр
        /// </summary>
        /// <param name="str">Исходная строка</param>
        /// <returns>Если буква не обнаружена или исходная строка пустая, возвращается исходная строка</returns>
        static public string ToUpperFirstLetter(this string str)
        {
            if (String.IsNullOrWhiteSpace(str)) return str;
            for (int i = 0; i < str.Length; i++)
            {
                if (Char.IsLetter(str[i])) return str.Remove(i, 1).Insert(i, str.Substring(i, 1).ToUpper());
            }
            return str;
        }
        /// <summary>
        /// Переводит в верхний регистр первую букву русского алфавита. Если первый символ не русская буква, вернет неизменную строку.
        /// </summary>
        /// <remarks>В пять раз выделяет меньше памяти чем вариант Remove.Insert,Substring.ToUpper</remarks>
        /// <param name="s1"></param>
        /// <returns></returns>
        static public string ToUpperFirstRu(this string s1)
        {
            if (string.IsNullOrWhiteSpace(s1)) return s1;
            var f = s1.GetType().GetField("_firstChar", BindingFlags.Instance | BindingFlags.NonPublic); //if f is null try m_firstChar
            char c = (char)f.GetValue(s1);
            if (c == 1105) f.SetValue(s1, 'Ё');
            else
            {
                if (c > 1071 && c < 1104)
                {
                    f.SetValue(s1, (char)(c - 32));
                }
            }
            return s1;
        }
        static public bool ContainWithComparison(this string source, string str, StringComparison comparison)
        {
            if (str == null) throw new ArgumentException("Искомая строка не может быть null");
            if (!Enum.IsDefined(typeof(StringComparison), comparison)) throw new ArgumentException("Неопределенный метод ставнения");
            if (source == null) return false;
            return source.IndexOf(str, comparison) >= 0;
        }
        
        /// <summary>
        /// Возвращает только цифровые символы из исходной строки
        /// </summary>
        /// <returns>Строка, содержащая только цифры или пустая строка</returns>
        public static string OnlyDigits(this string s) => new string(s?.Where(n => Char.IsDigit(n)).ToArray());
        /// <summary>
        /// Убирает все пробелы из исходной строки
        /// </summary>
        /// <returns>Строка без пробелов</returns>
        public static string SpaceFree(this string s) => s?.Replace(" ", "");
        public static string BeforeFirstDot(this string s)
        {
            int posdot = s.IndexOf('.');
            if (posdot < 0) return s;
            else return s.Substring(0, posdot + 2);
        }
        /// <summary>
        /// Форматирует строку как имя собственное
        /// </summary>
        /// <remarks>Только для русских букв</remarks>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToProperNoun(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            return s.Trim().ToLower().ToUpperFirstRu();
        }
     
    }
}
