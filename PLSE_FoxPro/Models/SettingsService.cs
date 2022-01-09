using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace PLSE_FoxPro.Models
{
    interface ISettingsService
    {
        void SetValue<T>(string key, T value);
        /// <summary>
        /// Читает значение из текущего <see cref="IServiceProvider"/> и приводит его к типу <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Ключ, ассоциированный с требуемым параметром настроек</param>
        /// <returns></returns>
        [Pure]
        T GetValue<T>(string key);
    }
    internal class SettingsService : ISettingsService
    {
        public T GetValue<T>(string key)
        {
            throw new NotImplementedException();
        }

        public void SetValue<T>(string key, T value)
        {
            throw new NotImplementedException();
        }
    }
}
