using System;
using System.Collections.Generic;
using System.Text;

namespace PLSE_FoxPro.Models
{
    public static class ResolutionHelper
    {
        public static void UnwrapDBStringToCollection(string obj, ICollection<NumerableContentWrapper> collection)
        {

        }
        /// <summary>
        /// Входит ли постановление в перечень выполняемых на платной основе
        /// </summary>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public static bool IsPayableResolution(Resolution resolution)
        {
                return resolution.TypeCase switch
                {
                    CaseType {Type: "гражданское" } => true,
                    CaseType {Type: "арбитражное" } => true,
                    CaseType {Type: "административное судопроизводство" } => true,
                    _ => false,
                };
        }
        /// <summary>
        /// Возвращает номер статьи и кодекс по которым дается подписка
        /// </summary>
        /// <param name="resolution"></param>
        /// <returns>Строку или null, если подписка не требуется/тип дела неизвестен</returns>
        public static string SubscriptionArticle(Resolution resolution)
        {
            return resolution.TypeCase switch
            {
                CaseType { Type: "гражданское" } => "85 ГПК РФ",
                CaseType { Type: "арбитражное" } => "55 АПК РФ",
                CaseType { Type: "административное судопроизводство" } => "49 КАС РФ",
                CaseType { Type: "уголовное" } => "57 УПК РФ",
                CaseType { Type: "проверка КУCП" } => "57 УПК РФ",
                CaseType { Type: "административное правонарушение" } => "25.9 КоАП РФ",
                _ => null,

            };
        }
        /// <summary>
        /// Возвращает номер статьи и кодекс по которым эксперт предупреждается об ответственности
        /// </summary>
        /// <param name="resolution"></param>
        /// <returns>Строку или null, если предупреждение не требуется/тип дела неизвестен</returns>
        public static string WarningArticle(Resolution resolution)
        {
            return resolution.TypeCase switch
            {
                CaseType { Type: "гражданское" } => "307 УК РФ",
                CaseType { Type: "арбитражное" } => "307 УК РФ",
                CaseType { Type: "административное судопроизводство" } => "307 УК РФ",
                CaseType { Type: "уголовное" } => "307 УК РФ",
                CaseType { Type: "проверка КУCП" } => "307 УК РФ",
                CaseType { Type: "административное правонарушение" } => "17.9 КоАП РФ",
                _ => null,

            };
        }
    }
}
