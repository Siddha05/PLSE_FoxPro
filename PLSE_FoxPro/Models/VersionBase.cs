using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace PLSE_FoxPro.Models
{
    public enum Version
    {
        Unknown = 0,
        New,
        Original,
        Edited
    }

    public abstract class ValidatorBase : ObservableValidator
    {
        /// <summary>
        /// Валидирует все открытие свойства объекта, помеченные аттрибутом ValidationAttribute
        /// </summary>
        public void Validate()
        {
            foreach (var item in TypeHelper.GetPublicPropertiesWithSetter(this.GetType()))
            {
                if (TypeHelper.IsDefineAttribute(item, typeof(ValidationAttribute))) ValidateProperty(item.GetValue(this), item.Name);
                if (IsInheritValidatorBase(item.PropertyType))
                {
                    var obj = (item.GetValue(this)) as ValidatorBase;
                    if (obj != null) obj.Validate();
                }
            }
        }
        /// <summary>
        /// Является ли <paramref name="type"/> наследником ValidatorBase?
        /// </summary>
        /// <param name="type"></param>
        /// <returns>True если наследует, иначе false</returns>
        /// <exception cref="ArgumentNullException"></exception>
        private bool IsInheritValidatorBase(Type type)
        {
            if (type == null) throw new ArgumentNullException();
            if (type == typeof(object)) return false;
            if (type.BaseType == typeof(ValidatorBase)) return true;
            else return IsInheritValidatorBase(type.BaseType);
        }
       
        /// <summary>
        /// Возвращает все открытые свойства
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public IEnumerable<PropertyInfo> GetOpenProperties()
        {
            foreach (var item in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                yield return item;
            }
        }
    }

    public abstract class VersionBase : ValidatorBase
    {
        #region Fields
        private Version _version;
        private DateTime _object_update;
        private int _id;
        #endregion

        #region Properties
        public DateTime ObjectModificationDate => _object_update;
        public Version Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }
        public int ID
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        #endregion

        #region Functions
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
             base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(Version)) return;
            if (_version == Version.Original || _version == Version.Unknown) _version = Version.Edited;
            _object_update = DateTime.Now;
            Debug.WriteLine($"Object {this.GetType().Name} changed to version {_version} (init property {e.PropertyName})", "VersionBase");
        }
        #endregion

        public VersionBase(int id, Version version)
        {
            _id = id;
            _version = version;
            _object_update = DateTime.UtcNow;
        }
        public VersionBase() : this(0, Version.New) { }
        
    }
}
