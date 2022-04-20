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
        private string _theme = BaseModel.Theme == "Light" ? "WhiteBalanceSunny" : "WeatherNight";
        private Settings _settings = new();
        private SettingsAbout _about = new();

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
                    About.IsUpdate = DataProjectInfo.IsUpdate ? "Download" : "Reload";
                    About.LatestVersion = DataProjectInfo.LatestVersion;
                }
            });
        public ICommand CreateCurrentVersionCommand =>
            new RelayCommand((obj) =>
            {
                BaseModel.IsWorking = true;
                Task.Run(() => {
                    bool status = ProjectInfoService.CreateCurrentVersion();
                    string mess = status ? "File upload was successful." : "Something went wrong...";
                    Message.Enqueue(mess);
                    BaseModel.IsWorking = false;
                    Main.Notifications.Add(new()
                    {
                        Title = "Update",
                        Message = mess
                    });
                });
            }, (obj) => About.Admin & !BaseModel.IsWorking);

        public ICommand ApplyCommand =>
            new RelayCommand((obj) =>
            {
                Settings settings = obj as Settings;

                Task.Run(() => {
                    if (Net.Get.Currency(settings.CurrencyApi) == 0)
                    {
                        MessageBox.Show(
                            "The \"CurrencyApi\" you provided is not working!", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Stop);
                    }
                    string steamApi = Services.BaseService.StatusSteam();
                    if (steamApi == "error")
                    {
                        MessageBox.Show(
                            "The \"SteamApiKey\" you provided is not working!", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Stop);
                    }
                    else if (string.IsNullOrEmpty(steamApi))
                        Main.Notifications.Add(new()
                        {
                            Title = "Steam Account",
                            Message = "Failed to get your API Key!\nSome features will not be available to you."
                        });
                });

                Theme = BaseModel.Theme == "Light" ? "WhiteBalanceSunny" : "WeatherNight";

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
            }, (obj) => !BaseModel.IsParsing & !BaseModel.IsWorking & !BaseModel.IsBrowser);

        public ICommand ThemeCommand =>
            new RelayCommand((obj) =>
            {
                SettingsProperties.Default.SetHours = false;
                App app = (App)Application.Current;
                if (BaseModel.Theme == "Light")
                {
                    Theme = "WeatherNight";
                    app.ChangeTheme(new("/Themes/Dark.xaml", UriKind.RelativeOrAbsolute));
                    BaseModel.Theme = "Dark";
                }
                else if (BaseModel.Theme == "Dark")
                {
                    Theme = "WhiteBalanceSunny";
                    app.ChangeTheme(new("/Themes/Light.xaml", UriKind.RelativeOrAbsolute));
                    BaseModel.Theme = "Light";
                }
                SettingsProperties.Default.Theme = BaseModel.Theme;
                SettingsProperties.Default.Save();
            });
    }
}