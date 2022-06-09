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
        private string _theme = ProjectInfo.Theme == "Light" ? "WhiteBalanceSunny" : "WeatherNight";
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
                    Edit.OpenUrl("https://free.currencyconverterapi.com/free-api-key");
                    Edit.OpenUrl("https://openexchangerates.org/signup/free");
                });
            }
        }
        //order
        public ICommand GetSteamApiCommand =>
            new RelayCommand((obj) =>
            {
                Edit.OpenUrl("https://steamcommunity.com/dev/apikey");
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
                StartUpProperties.Default.SteamLoginSecure = string.Empty;
                StartUpProperties.Default.SteamCurrencyId = 0;
                StartUpProperties.Default.SessionBuff = string.Empty;
                StartUpProperties.Default.Remember = false;

                string profilesDir = ProjectInfo.AppPath + "Profile";
                if (!Directory.Exists(profilesDir))
                    Directory.Delete(profilesDir);

                Application.Current.Shutdown();
            });
        public ICommand OpenMarketCommand =>
            new RelayCommand((obj) =>
            {
                Edit.OpenUrl("https://help.steampowered.com/en/faqs/view/71D3-35C2-AD96-AA3A");
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
                    bool status = ProjectInfoService.UploadCurrentVersion();
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

                Theme = ProjectInfo.Theme == "Light" ? "WhiteBalanceSunny" : "WeatherNight";

                SettingsProperties.Default.SetHours = settings.SetHours;
                SettingsProperties.Default.TurnOn = settings.TurnOn;
                SettingsProperties.Default.TurnOff = settings.TurnOff;

                SettingsProperties.Default.MinPrecent = settings.MinPrecent;
                SettingsProperties.Default.ServiceId = settings.ServiceId;

                StartUpProperties.Default.Remember = settings.RememberMe;
                SteamAccount.ApiKey = settings.SteamApiKey;

                if (!settings.RememberMe)
                    StartUpProperties.Default.SteamLoginSecure = string.Empty;

                SettingsProperties.Default.Save();
                StartUpProperties.Default.Save();
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