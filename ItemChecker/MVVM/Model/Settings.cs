using ItemChecker.Core;
using ItemChecker.Net;
using ItemChecker.Properties;
using System;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class Settings
    {
        //steam
        public string SteamApiKey { get; set; } = SteamRequest.ApiKey;
        public string AccountName { get; set; } = SteamAccount.AccountName;
        public string SteamId { get; set; } = SteamRequest.ID64;
        public string Currency { get; set; } = SteamAccount.Currency.Name;

        //Base
        public bool UseLocalDb { get; set; } = SettingsProperties.Default.UseLocalDb;
        public int StmCount { get; set; } = ItemsBase.List.Count;
        public DateTime StmUpdated { get; set; } = ItemsBase.Updated;
        public int CsmCount { get; set; } = ItemsBase.List.Where(x => x.Csm.Id != 0).Count();
        public int CsmUpdated { get; set; } = (int)(DateTime.Now - ItemsBase.List.Select(x => x.Csm.Updated).Max()).TotalMinutes;
        public int LfmCount { get; set; } = ItemsBase.List.Where(x => x.Lfm.Price != 0).Count();
        public int LfmUpdated { get; set; } = (int)(DateTime.Now - ItemsBase.List.Select(x => x.Lfm.Updated).Max()).TotalMinutes;
        public int BuffCount { get; set; } = ItemsBase.List.Where(x => x.Buff.Id != 0).Count();
        public int BuffUpdated { get; set; } = (int)(DateTime.Now - ItemsBase.List.Select(x => x.Buff.Updated).Max()).TotalMinutes;
    }
    public class SettingsAbout : ObservableObject
    {
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
        string _currentVersion = DataProjectInfo.CurrentVersion;
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
        string _latestVersion = DataProjectInfo.LatestVersion;
        public bool Admin { get; set; } = SteamAccount.AccountName == "bahtiarov116";
    }
}