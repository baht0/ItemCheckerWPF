using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    public class SettingViewModel : ObservableObject
    {
        private string _steamApiKey;
        private Settings _settings;
        //profile
        ObservableCollection<string> _profiles;
        private string _currentProfile;
        private string _selectedProfile;
        //about
        private string _currentVersion;
        private string _latestVersion;
        private string _isUpdate;

        public string SteamApiKey
        {
            get
            {
                return _steamApiKey;
            }
            set
            {
                _steamApiKey = value;
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
            SteamApiKey = GeneralProperties.Default.SteamApiKey;

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
                NotEnoughBalance = GeneralProperties.Default.NotEnoughBalance,
                CancelOrder = GeneralProperties.Default.CancelOrder,

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
        public ICommand GetSteamApiCommand
        {
            get
            {
                return new RelayCommand((obj) =>
                {
                    SteamApiKey = Account.GetSteamApiKey();
                }, (obj) => !Main.IsLoading);
            }
        }
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
            GeneralProperties.Default.SteamApiKey = SteamApiKey;
            GeneralProperties.Default.CurrencyApiKey = settings.CurrencyApi;
            GeneralProperties.Default.Currency = settings.Currency;
            GeneralProperties.Default.ExitChrome = settings.ExitChrome;
            GeneralProperties.Default.Guard = settings.Guard;
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
    }
}