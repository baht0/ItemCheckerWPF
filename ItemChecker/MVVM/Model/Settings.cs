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
        private string _currentVersion = DataProjectInfo.CurrentVersion;
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
        private string _latestVersion = DataProjectInfo.LatestVersion;
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
        private bool _admin = SteamAccount.AccountName == "bahtiarov116";
    }
}
