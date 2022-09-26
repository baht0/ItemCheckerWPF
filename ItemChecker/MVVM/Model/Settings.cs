using ItemChecker.Core;
using ItemChecker.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class Settings
    {
        //general
        public int MinPrecent { get; set; } = SettingsProperties.Default.MinPrecent;
        public List<string> ServicesList { get; set; } = Main.Services;
        public int ServiceId { get; set; } = SettingsProperties.Default.ServiceId;
        public bool SetHours { get; set; } = SettingsProperties.Default.SetHours;
        public DateTime TurnOn { get; set; } = SettingsProperties.Default.TurnOn;
        public DateTime TurnOff { get; set; } = SettingsProperties.Default.TurnOff;

        //steam
        public string SteamApiKey { get; set; } = SteamAccount.ApiKey;
        public string UserName { get; set; } = SteamAccount.UserName;
        public string AccountName { get; set; } = SteamAccount.AccountName;
        public string SteamId { get; set; } = SteamAccount.Id64;
        public string SteamMarket { get; set; } = SteamAccount.StatusMarket;
        public string Currency { get; set; } = SteamBase.CurrencyList.FirstOrDefault(x => x.Id == SteamAccount.CurrencyId).Name;

        //Base
        public bool UseLocalDb { get; set; } = SettingsProperties.Default.UseLocalDb;
        public int StmCount { get; set; } = SteamBase.ItemList.Count;
        public DateTime StmUpdated { get; set; } = SteamBase.Updated;
        public int CsmCount { get; set; } = SteamBase.ItemList.Where(x => x.Csm.Id != 0).Count();
        public int CsmUpdated { get; set; } = (int)(DateTime.Now - SteamBase.ItemList.Select(x => x.Csm.Updated).Max()).TotalMinutes;
        public int LfmCount { get; set; } = SteamBase.ItemList.Where(x => x.Lfm.Price != 0).Count();
        public int LfmUpdated { get; set; } = (int)(DateTime.Now - SteamBase.ItemList.Select(x => x.Lfm.Updated).Max()).TotalMinutes;
        public int BuffCount { get; set; } = SteamBase.ItemList.Where(x => x.Buff.Id != 0).Count();
        public int BuffUpdated { get; set; } = (int)(DateTime.Now - SteamBase.ItemList.Select(x => x.Buff.Updated).Max()).TotalMinutes;
    }
    public class SettingsAbout : ObservableObject
    {
        private string _currentVersion = DataProjectInfo.CurrentVersion;
        private string _latestVersion = DataProjectInfo.LatestVersion;
        private string _isUpdate = DataProjectInfo.IsUpdate ? "Download" : "Reload";
        private bool _admin = SteamAccount.AccountName == "bahtiarov116";

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
        public bool Admin
        {
            get
            {
                return _admin;
            }
            set
            {
                _admin = value;
                OnPropertyChanged();
            }
        }
    }
}
