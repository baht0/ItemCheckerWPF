using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Windows.Input;
using System.Windows;
using System;
using ItemChecker.Support;
using OpenQA.Selenium.Support.Extensions;
using System.Threading.Tasks;

namespace ItemChecker.MVVM.ViewModel
{
    public class SettingViewModel : ObservableObject
    {
        private string _theme;
        private Settings _settings;
        //profile
        ObservableCollection<string> _profiles;
        private string _currentProfile;
        private string _selectedProfile;
        //about
        private string _currentVersion;
        private string _latestVersion;
        private string _isUpdate;

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
        //profile
        public ObservableCollection<string> Profiles
        {
            get
            {
                return _profiles;
            }
            set
            {
                _profiles = value;
                OnPropertyChanged();
            }
        }
        public string CurrentProfile
        {
            get
            {
                return _currentProfile;
            }
            set
            {
                _currentProfile = value;
                OnPropertyChanged();
            }
        }
        public string SelectedProfile
        {
            get
            {
                return _selectedProfile;
            }
            set
            {
                _selectedProfile = value;
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
            if (Main.Theme == "Light")
                Theme = "WeatherNight";
            if (Main.Theme == "Dark")
                Theme = "WhiteBalanceSunny";

            CheckProfiles();

            Settings = new Settings()
            {
                CurrencyApi = GeneralProperties.Default.CurrencyApiKey,
                CurrencyList = new ObservableCollection<string>()
                {
                    "USD ($)",
                    "RUB (₽)"
                },
                Currency = GeneralProperties.Default.Currency,
                ExitChrome = GeneralProperties.Default.ExitChrome,
                Guard = GeneralProperties.Default.Guard,
                SetHours = GeneralProperties.Default.SetHours,
                TurnOn = GeneralProperties.Default.TurnOn,
                TurnOff = GeneralProperties.Default.TurnOff,

                SteamApiKey = Account.ApiKey,
                NotEnoughBalance = GeneralProperties.Default.NotEnoughBalance,
                CancelOrder = GeneralProperties.Default.CancelOrder,
                SteamId = Account.Id64,
                Account = Account.AccountName,
                SteamMarket = Account.SteamMarket,

                FactoryNew = FloatProperties.Default.maxFloatValue_FN,
                MinimalWear = FloatProperties.Default.maxFloatValue_MW,
                FieldTested = FloatProperties.Default.maxFloatValue_FT,
                WellWorn = FloatProperties.Default.maxFloatValue_WW,
                BattleScarred = FloatProperties.Default.maxFloatValue_BS,

                CurrentProfile = GeneralProperties.Default.Profile
            };

            CurrentVersion = BaseModel.Version;
            LatestVersion = ProjectInfo.LatestVersion.Version;

            IsUpdate = "Reload";
            if (ProjectInfo.IsUpdate)
                IsUpdate = "Download";
        }

        void CheckProfiles()
        {
            List<string> dirProfiles = Directory.GetDirectories(BaseModel.AppPath + "\\Profiles").Select(d => new DirectoryInfo(d).Name).ToList();
            Profiles = new ObservableCollection<string>(dirProfiles);
        }
        public ICommand GetCurrencyApiCommand
        {
            get
            {
                return new RelayCommand((obj) =>
                {
                    Support.Edit.openUrl("https://free.currencyconverterapi.com/free-api-key");
                    Support.Edit.openUrl("https://openexchangerates.org/signup/free");
                });
            }
        }
        //steam
        public ICommand LogoutCommand =>
            new RelayCommand((obj) =>
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to logout?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                    return;

                BaseModel.cts.Cancel();
                Task.Run(() => {
                    BaseModel.Browser.Navigate().GoToUrl("https://steamcommunity.com/market/");
                    BaseModel.Browser.ExecuteJavaScript("Logout();");
                    BaseModel.BrowserExit();
                }).Wait(6000);
                Application.Current.Shutdown();
            }, (obj) => !Main.IsLoading & !Main.Timer.Enabled);
        public ICommand CopyIdCommand =>
            new RelayCommand((obj) =>
            {
                Clipboard.SetText(Account.Id64);
            });
        public ICommand OpenMarketCommand =>
            new RelayCommand((obj) =>
            {
                Edit.openUrl("https://help.steampowered.com/en/faqs/view/71D3-35C2-AD96-AA3A");
            }, (obj) => Account.SteamMarket == "Disabled");
        public ICommand GetSteamApiCommand =>
            new RelayCommand((obj) =>
            {
                Account.GetSteamApiKey();
                Settings.SteamApiKey = Account.ApiKey;
            }, (obj) => !Main.IsLoading);
        //profile
        public ICommand AddProfileCommand
        {
            get
            {
                return new RelayCommand((obj) =>
                {
                    if (!Directory.Exists(BaseModel.AppPath + $"\\Profiles\\{(string)obj}"))
                        Directory.CreateDirectory(BaseModel.AppPath + $"\\Profiles\\{(string)obj}");

                    CheckProfiles();
                });
            }
        }
        public ICommand RemoveProfileCommand
        {
            get
            {
                return new RelayCommand((obj) =>
                {
                    if (Directory.Exists(BaseModel.AppPath + $"\\Profiles\\{(string)obj}"))
                        Directory.Delete(BaseModel.AppPath + $"\\Profiles\\{(string)obj}");

                    CheckProfiles();

                }, (obj) => Profiles.Count != 1);
            }
        }
        //about
        public ICommand WhatIsNewCommand => 
            new RelayCommand((obj) =>
            {

            });
        public ICommand UpdateCommand =>
            new RelayCommand((obj) =>
            {
                if (ProjectInfo.IsUpdate)
                    ProjectInfoService.Update();
                else
                {
                    ProjectInfoService.CheckUpdate();
                    if (ProjectInfo.IsUpdate)
                        IsUpdate = "Download";
                }
            });
        public ICommand CreateCurrentVersionCommand =>
            new RelayCommand((obj) =>
            {
                ProjectInfoService.CreateCurrentVersion();
            }, (obj) => Account.AccountName == "bahtiarov116");

        public ICommand ApplyCommand =>
            new RelayCommand((obj) =>
            {
                Settings settings = obj as Settings;
                SaveConfig(settings);
            }, (obj) => !Main.IsLoading);
        void SaveConfig(Settings settings)
        {
            GeneralProperties.Default.CurrencyApiKey = settings.CurrencyApi;
            GeneralProperties.Default.Currency = settings.Currency;
            GeneralProperties.Default.ExitChrome = settings.ExitChrome;
            GeneralProperties.Default.Guard = settings.Guard;
            GeneralProperties.Default.SetHours = settings.SetHours;
            GeneralProperties.Default.TurnOn = settings.TurnOn;
            GeneralProperties.Default.TurnOff = settings.TurnOff;

            GeneralProperties.Default.NotEnoughBalance = settings.NotEnoughBalance;
            GeneralProperties.Default.CancelOrder = settings.CancelOrder;

            FloatProperties.Default.maxFloatValue_FN = settings.FactoryNew;
            FloatProperties.Default.maxFloatValue_MW = settings.MinimalWear;
            FloatProperties.Default.maxFloatValue_FT = settings.FieldTested;
            FloatProperties.Default.maxFloatValue_WW = settings.WellWorn;
            FloatProperties.Default.maxFloatValue_BS = settings.BattleScarred;

            GeneralProperties.Default.Profile = settings.CurrentProfile;

            GeneralProperties.Default.Save();
        }

        public ICommand ThemeCommand =>
            new RelayCommand((obj) =>
            {
                App app = (App)Application.Current;
                if (Main.Theme == "Light")
                {
                    Theme = "WhiteBalanceSunny";
                    app.ChangeTheme(new("/Themes/Dark.xaml", UriKind.RelativeOrAbsolute));
                    Main.Theme = "Dark";
                }
                else if (Main.Theme == "Dark")
                {
                    Theme = "WeatherNight";
                    app.ChangeTheme(new("/Themes/Light.xaml", UriKind.RelativeOrAbsolute));
                    Main.Theme = "Light";
                }
            });
    }
}