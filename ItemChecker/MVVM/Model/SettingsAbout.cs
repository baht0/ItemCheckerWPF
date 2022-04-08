using ItemChecker.Core;

namespace ItemChecker.MVVM.Model
{
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
