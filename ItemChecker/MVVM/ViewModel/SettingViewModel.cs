using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Properties;
using System.IO;
using System.Windows.Input;
using System.Windows;
using System;
using ItemChecker.Support;
using MaterialDesignThemes.Wpf;
using System.Threading.Tasks;
using System.Linq;
using ItemChecker.Net;

namespace ItemChecker.MVVM.ViewModel
{
    public class SettingViewModel : ObservableObject
    {
        public SnackbarMessageQueue Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }
        SnackbarMessageQueue _message = new();
        public Settings Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }
        Settings _settings = new();
        public SettingsAbout About
        {
            get
            {
                return _about;
            }
            set
            {
                _about = value;
                OnPropertyChanged();
            }
        }
        SettingsAbout _about = new();

        public ICommand GetCurrencyApiCommand
        {
            get
            {
                return new RelayCommand((obj) =>
                {
                    Edit.OpenUrl("https://free.currencyconverterapi.com/free-api-key");
                    Edit.OpenUrl("https://openexchangerates.org/signup/free");
                });
            }
        }
        //steam
        public ICommand LogoutCommand =>
            new RelayCommand((obj) =>
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to logout?", "Question",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                    return;

                string path = ProjectInfo.DocumentPath + "Net";
                if (Directory.Exists(path))
                    Directory.Delete(path, true);

                MainProperties.Default.SteamCurrencyId = 0;
                MainProperties.Default.Save();

                Application.Current.Shutdown();
            });
        public ICommand CopyApiCommand =>
            new RelayCommand((obj) =>
            {
                Clipboard.SetText(SteamRequest.ApiKey);
            });
        public ICommand CopyIdCommand =>
            new RelayCommand((obj) =>
            {
                Clipboard.SetText(SteamRequest.ID64);
            });
        //base
        public ICommand ResetBaseCommand =>
            new RelayCommand((obj) =>
            {
                var id = Convert.ToInt32(obj);
                switch (id)
                {
                    case 2:
                        foreach (var item in ItemsBase.List.Where(x => x.Csm.Updated.AddMinutes(30) > DateTime.Now).ToList())
                            item.Csm.Updated = DateTime.MinValue;
                        break;
                    case 3:
                        foreach (var item in ItemsBase.List.Where(x => x.Lfm.Updated.AddMinutes(30) > DateTime.Now).ToList())
                            item.Lfm.Updated = DateTime.MinValue;
                        break;
                    case 4:
                        foreach (var item in ItemsBase.List.Where(x => x.Buff.Updated.AddMinutes(30) > DateTime.Now).ToList())
                            item.Buff.Updated = DateTime.MinValue;
                        break;
                }
            });
        //about
        public ICommand CreateCurrentVersionCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    bool status = ProjectInfoService.UploadCurrentVersion();
                    string mess = status ? $"File upload was successful.\nVersion: {DataProjectInfo.CurrentVersion}" : "Something went wrong...";
                    Main.Notifications.Add(new()
                    {
                        Title = "Load Update.",
                        Message = mess
                    });
                });
            }, (obj) => About.Admin);

        public ICommand ApplyCommand =>
            new RelayCommand((obj) =>
            {
                Settings settings = obj as Settings;

                SettingsProperties.Default.UseLocalDb = settings.UseLocalDb;
                SettingsProperties.Default.Save();
            });
    }
}