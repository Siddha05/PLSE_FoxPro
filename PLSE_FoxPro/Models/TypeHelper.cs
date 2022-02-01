using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace PLSE_FoxPro.Models
{
    public static class TypeHelper
    {
        /// <summary>
        /// Возвращает все открытые экземплярные свойства типа
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IEnumerable<PropertyInfo> GetPublicProperties(Type type) => 
                                GetProperties(type, BindingFlags.Instance | BindingFlags.Public);
        /// <summary>
        /// Возвращает все экземплярные свойства типа
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IEnumerable<PropertyInfo> GetAllProperties(Type type) =>
                                GetProperties(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        /// <summary>
        /// Возвращает все открытые экземплярные свойства типа имеющие setter
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IEnumerable<PropertyInfo> GetPublicPropertiesWithSetter(Type type)
        {
            foreach (var item in GetProperties(type, BindingFlags.Instance | BindingFlags.Public))
            {
                if (item.CanWrite) yield return item;
            }
        }
        private static IEnumerable<PropertyInfo> GetProperties(Type type, BindingFlags flags)
        {
            if (type == null) throw new ArgumentNullException($"Argument {nameof(type)} was null");
            foreach (var item in type.GetProperties(flags))
            {
                yield return item;
            }
        }

        /// <summary>
        /// Декларирует ли <paramref name="member"/> аттрибут типа <paramref name="type"/>?
        /// </summary>
        /// <param name="member">Член чля которого определяется наличие аттрибута</param>
        /// <param name="type">Тип определяемого аттрибута</param>
        /// <returns>True если определяет, иначе false</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsDefineAttribute(MemberInfo member, Type type)
        {
            if (member == null) throw new ArgumentNullException($"{nameof(member)} was null");
            if (type == null) throw new ArgumentNullException($"{nameof(type)} was null");
            return member.IsDefined(type);
        }
    }
}
