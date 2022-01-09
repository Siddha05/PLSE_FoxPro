using System;
using System.Text;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Controls;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PLSE_FoxPro.Models;

namespace PLSE_FoxPro.ViewModels
{
    internal class SettingsVM : ObservableValidator
    {
        #region Fields
        RelayCommand _openfolder;
        string _save_folder;
        #endregion

        #region Properties
        [Required]
        public string SaveFolder
        {
            get => _save_folder;
            set => SetProperty(ref _save_folder, value, true);
        }    
        public bool IsLastLoginSave { get; set; }
        public bool IsExpertiseScan { get; set; }
        public bool IsExpertScan { get; set; }
        public int LeftDaysFactor { get; set; }
        public int DaysOnRequestFactor { get; set; }
        public bool IsShowNotification { get; set; }
        public bool IsShowNearEvent { get; set; }
        #endregion

        #region Commands
        public RelayCommand OpenFolder
        {
            get
            {
                return _openfolder != null ? _openfolder : _openfolder = new RelayCommand(() =>
                {
                    System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
                    if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        SaveFolder = folderBrowser.SelectedPath;
                    }
                });
            }
        }
        public RelayCommand<Page> Save
        {
            get
            {
                return new RelayCommand<Page>(n =>
                {
                    //FrameworkElement el = n as FrameworkElement;
                    if (Validation.GetHasError(n))
                    {
                        MessageBox.Show("Ошибки ввода на странице. Изправьте прежде чем сохранить");
                    }
                    else
                    {
                        Properties.Settings.Default.IsExpertiseScan = IsExpertiseScan;
                        Properties.Settings.Default.IsLastLoginSave = IsLastLoginSave;
                        Properties.Settings.Default.IsExpertSpecialityScan = IsExpertScan;
                        Properties.Settings.Default.InicalFolderPath = SaveFolder;
                        Properties.Settings.Default.WarnLeftDaysExpTreshold = LeftDaysFactor;
                        Properties.Settings.Default.WarnDaysOnRequestTreshold = DaysOnRequestFactor;
                        Properties.Settings.Default.IsShowNotification = IsShowNotification;
                        Properties.Settings.Default.IsShowNearEvent = IsShowNearEvent;
                        Properties.Settings.Default.Save();
                        App.SendMessage("Настройки успешно сохранены");
                        App.Services.GetService<IPagesService>().RemovePage();
                    }
                });
            }
        }
        public RelayCommand Cancel => new RelayCommand(() => App.Services.GetService<IPagesService>().RemovePage());
        #endregion
        
        public SettingsVM()
        {
            IsLastLoginSave = Properties.Settings.Default.IsLastLoginSave;
            IsExpertiseScan = Properties.Settings.Default.IsExpertiseScan;
            IsExpertScan = Properties.Settings.Default.IsExpertSpecialityScan;
            SaveFolder = Properties.Settings.Default.InicalFolderPath;
            LeftDaysFactor = Properties.Settings.Default.WarnLeftDaysExpTreshold;
            DaysOnRequestFactor = Properties.Settings.Default.WarnDaysOnRequestTreshold;
            IsShowNotification = Properties.Settings.Default.IsShowNotification;
            IsShowNearEvent = Properties.Settings.Default.IsShowNearEvent;
        }
    }
}
