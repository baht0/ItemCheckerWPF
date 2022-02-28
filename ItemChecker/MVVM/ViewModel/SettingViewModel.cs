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

namespace ItemChecker.MVVM.ViewModel
{
    public class SettingViewModel : ObservableObject
    {
        SnackbarMessageQueue _message = new();
        private string _theme = BaseModel.Theme == "Light" ? "WeatherNight" : "WhiteBalanceSunny";
        private Settings _settings = new();
        //about
        private string _currentVersion = DataProjectInfo.CurrentVersion;
        private string _latestVersion = DataProjectInfo.LatestVersion;
        private string _isUpdate = DataProjectInfo.IsUpdate ? "Download" : "Reload";

        public SnackbarMessageQueue Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }
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
        //about
        public string CurrentVersion
        {
            get
            {
                return _currentVersion;
            }
            set
            {
                _currentVersion = value;
                OnPropertyChanged();
            }
        }
        public string LatestVersion
        {
            get
            {
                return _latestVersion;
            }
            set
            {
                _latestVersion = value;
                OnPropertyChanged();
            }
        }
        public string IsUpdate
        {
            get
            {
                return _isUpdate;
            }
            set
            {
                _isUpdate = value;
                OnPropertyChanged();
            }
        }

        public SettingViewModel()
        {
            Task.Run(() => ProjectInfoService.CheckUpdate());
            LatestVersion = DataProjectInfo.LatestVersion;
            if (DataProjectInfo.IsUpdate)
                Message.Enqueue("Update available!");
        }
        public ICommand GetCurrencyApiCommand
        {
            get
            {
                return new RelayCommand((obj) =>
                {
                    Edit.openUrl("https://free.currencyconverterapi.com/free-api-key");
                    Edit.openUrl("https://openexchangerates.org/signup/free");
                });
            }
        }
        //order
        public ICommand GetSteamApiCommand =>
            new RelayCommand((obj) =>
            {
                Edit.openUrl("https://steamcommunity.com/dev/apikey");
            });
        public ICommand CopyIdCommand =>
            new RelayCommand((obj) =>
            {
                Clipboard.SetText(SteamAccount.Id64);
            });
        public ICommand LogoutCommand =>
            new RelayCommand((obj) =>
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to logout?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                    return;
                SettingsProperties.Default.SteamLoginSecure = string.Empty;

                string profilesDir = BaseModel.AppPath + "Profile";
                if (!Directory.Exists(profilesDir))
                    Directory.Delete(profilesDir);

                Application.Current.Shutdown();
            });
        public ICommand OpenMarketCommand =>
            new RelayCommand((obj) =>
            {
                Edit.openUrl("https://help.steampowered.com/en/faqs/view/71D3-35C2-AD96-AA3A");
            }, (obj) => SteamAccount.SteamMarket == "Disabled");
        //about
        public ICommand WhatIsNewCommand => 
            new RelayCommand((obj) =>
            {
                WhatsNewWindow window = new();
                window.ShowDialog();
            });
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
                    IsUpdate = DataProjectInfo.IsUpdate ? "Download" : "Reload";
                }
            });
        public ICommand CreateCurrentVersionCommand =>
            new RelayCommand((obj) =>
            {
                BaseModel.IsWorking = true;
                Task.Run(() => {
                    bool status = ProjectInfoService.CreateCurrentVersion();
                    string mess = status ? "Success" : "Something went wrong...";
                    Message.Enqueue(mess);
                    BaseModel.IsWorking = false;
                });
            }, (obj) => SteamAccount.AccountName == "bahtiarov116" & !BaseModel.IsWorking);

        public ICommand ApplyCommand =>
            new RelayCommand((obj) =>
            {
                Settings settings = obj as Settings;
                SaveConfig(settings);
            }, (obj) => !BaseModel.IsParsing & !BaseModel.IsWorking & !BaseModel.IsBrowser);
        void SaveConfig(Settings settings)
        {
            SettingsProperties.Default.CurrencyApiKey = settings.CurrencyApi;
            SettingsProperties.Default.CurrencyId = settings.CurrencyId;
            SettingsProperties.Default.Quit = settings.Quit;
            SettingsProperties.Default.SetHours = settings.SetHours;
            SettingsProperties.Default.TurnOn = settings.TurnOn;
            SettingsProperties.Default.TurnOff = settings.TurnOff;

            SettingsProperties.Default.MinPrecent = settings.MinPrecent;
            SettingsProperties.Default.ServiceId = settings.ServiceId;
            SteamAccount.ApiKey = settings.SteamApiKey;

            FloatProperties.Default.maxFloatValue_FN = settings.FactoryNew;
            FloatProperties.Default.maxFloatValue_MW = settings.MinimalWear;
            FloatProperties.Default.maxFloatValue_FT = settings.FieldTested;
            FloatProperties.Default.maxFloatValue_WW = settings.WellWorn;
            FloatProperties.Default.maxFloatValue_BS = settings.BattleScarred;

            SettingsProperties.Default.Save();
        }

        public ICommand ThemeCommand =>
            new RelayCommand((obj) =>
            {
                App app = (App)Application.Current;
                if (BaseModel.Theme == "Light")
                {
                    Theme = "WhiteBalanceSunny";
                    app.ChangeTheme(new("/Themes/Dark.xaml", UriKind.RelativeOrAbsolute));
                    BaseModel.Theme = "Dark";
                }
                else if (BaseModel.Theme == "Dark")
                {
                    Theme = "WeatherNight";
                    app.ChangeTheme(new("/Themes/Light.xaml", UriKind.RelativeOrAbsolute));
                    BaseModel.Theme = "Light";
                }
            });
    }
}