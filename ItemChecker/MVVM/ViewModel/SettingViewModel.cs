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
using ItemChecker.MVVM.View;
using System.Linq;

namespace ItemChecker.MVVM.ViewModel
{
    public class SettingViewModel : ObservableObject
    {
        private string _theme = ProjectInfo.Theme == "Light" ? "WhiteBalanceSunny" : "WeatherNight";

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
        public string Theme
        {
            get
            {
                return _theme;
            }
            set
            {
                _theme = value;
                OnPropertyChanged();
            }
        }
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
        private Settings _settings = new();
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
        private SettingsAbout _about = new();

        public SettingViewModel()
        {
            About.LatestVersion = DataProjectInfo.LatestVersion;
            if (DataProjectInfo.IsUpdate)
                Message.Enqueue("Update available!");
        }
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
        public ICommand ResetSteamApiCommand =>
            new RelayCommand((obj) =>
            {
                SteamAccount.ApiKey = string.Empty;
            });
        public ICommand CopyIdCommand =>
            new RelayCommand((obj) =>
            {
                Clipboard.SetText(SteamAccount.Id64);
            });
        public ICommand LogoutCommand =>
            new RelayCommand((obj) =>
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to logout?", "Question",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                    return;

                MainProperties.Default.SteamLoginSecure = string.Empty;
                MainProperties.Default.SteamCurrencyId = 0;
                MainProperties.Default.SessionBuff = string.Empty;
                MainProperties.Default.Save();
                MainProperties.Default.Save();

                string profilesDir = ProjectInfo.DocumentPath + "profile";
                if (!Directory.Exists(profilesDir))
                    Directory.Delete(profilesDir);

                Application.Current.Shutdown();
            });
        public ICommand OpenMarketCommand =>
            new RelayCommand((obj) =>
            {
                Edit.OpenUrl("https://help.steampowered.com/en/faqs/view/71D3-35C2-AD96-AA3A");
            }, (obj) => SteamAccount.StatusMarket == "Disabled");
        //base
        public ICommand ResetBaseCommand =>
            new RelayCommand((obj) =>
            {
                var id = Convert.ToInt32(obj);
                switch (id)
                {
                    case 2:
                        foreach (var item in SteamBase.ItemList.Where(x => x.Csm.Updated.AddMinutes(30) > DateTime.Now).ToList())
                            item.Csm.Updated = DateTime.MinValue;
                        break;
                    case 3:
                        foreach (var item in SteamBase.ItemList.Where(x => x.Lfm.Updated.AddMinutes(30) > DateTime.Now).ToList())
                            item.Lfm.Updated = DateTime.MinValue;
                        break;
                    case 4:
                        foreach (var item in SteamBase.ItemList.Where(x => x.Buff.Updated.AddMinutes(30) > DateTime.Now).ToList())
                            item.Buff.Updated = DateTime.MinValue;
                        break;
                }
            });
        //about
        public ICommand UpdateCommand =>
            new RelayCommand((obj) =>
            {
                if (DataProjectInfo.IsUpdate)
                {
                    MessageBoxResult result = MessageBox.Show($"Want to upgrade from {DataProjectInfo.CurrentVersion} to {DataProjectInfo.LatestVersion}?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                        ProjectInfoService.Update();
                }
                else
                {
                    ProjectInfoService.CheckUpdate();
                    About.IsUpdate = DataProjectInfo.IsUpdate ? "Download" : "Reload";
                    About.LatestVersion = DataProjectInfo.LatestVersion;
                }
            });
        public ICommand CreateCurrentVersionCommand =>
            new RelayCommand((obj) =>
            {
                BaseModel.IsWorking = true;
                Task.Run(() => {
                    bool status = ProjectInfoService.UploadCurrentVersion();
                    string mess = status ? $"File upload was successful.\nVersion: {DataProjectInfo.CurrentVersion}" : "Something went wrong...";
                    BaseModel.IsWorking = false;
                    Main.Notifications.Add(new()
                    {
                        Title = "Load Update.",
                        Message = mess
                    });
                });
            }, (obj) => About.Admin & !BaseModel.IsWorking);

        public ICommand ApplyCommand =>
            new RelayCommand((obj) =>
            {
                Settings settings = obj as Settings;

                Theme = ProjectInfo.Theme == "Light" ? "WhiteBalanceSunny" : "WeatherNight";

                SettingsProperties.Default.SetHours = settings.SetHours;
                SettingsProperties.Default.TurnOn = settings.TurnOn;
                SettingsProperties.Default.TurnOff = settings.TurnOff;

                SettingsProperties.Default.MinPrecent = settings.MinPrecent;
                SettingsProperties.Default.ServiceId = settings.ServiceId;

                SettingsProperties.Default.UseLocalDb = settings.UseLocalDb;

                SettingsProperties.Default.Save();
            }, (obj) => !BaseModel.IsWorking);

        public ICommand ThemeCommand =>
            new RelayCommand((obj) =>
            {
                SettingsProperties.Default.SetHours = false;
                App app = (App)Application.Current;
                if (ProjectInfo.Theme == "Light")
                {
                    Theme = "WeatherNight";
                    app.ChangeTheme(new("/Themes/Dark.xaml", UriKind.RelativeOrAbsolute));
                    ProjectInfo.Theme = "Dark";
                }
                else if (ProjectInfo.Theme == "Dark")
                {
                    Theme = "WhiteBalanceSunny";
                    app.ChangeTheme(new("/Themes/Light.xaml", UriKind.RelativeOrAbsolute));
                    ProjectInfo.Theme = "Light";
                }
                SettingsProperties.Default.Theme = ProjectInfo.Theme;
                SettingsProperties.Default.Save();
            });
    }
}