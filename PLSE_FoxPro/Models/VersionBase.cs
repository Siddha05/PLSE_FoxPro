using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace PLSE_FoxPro.Models
{
    public enum Version
    {
        Unlnown = 0,
        New,
        Original,
        Edited
    }
    public abstract class VersionBase : ObservableValidator
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
            if (_version == Version.Original || _version == Version.Unlnown) _version = Version.Edited;
            _object_update = DateTime.Now;
            Debug.WriteLine($"Property {e.PropertyName} changed to {_version}", "VersionBase");
        }
        public void Validate() => ValidateAllProperties();
        public void ValidateProperty() => ValidateProperty();
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
