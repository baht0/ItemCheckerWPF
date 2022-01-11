using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Properties;
using ItemChecker.Services;
using ItemChecker.Support;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        delegate void ThemeDeleg();
        event ThemeDeleg OnThemeDeleg;
        SnackbarMessageQueue _message = new();
        Main _mainInfo;        
        private ObservableCollection<string> _favoriteList = HomeProperties.Default.FavoriteList ?? (new());

        public SnackbarMessageQueue Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }
        public Main MainInfo
        {
            get { return _mainInfo; }
            set
            {
                _mainInfo = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<string> FavoriteList
        {
            get
            {
                return _favoriteList;
            }
            set
            {
                _favoriteList = value;
                OnPropertyChanged();
            }
        } //favorite

        public MainViewModel()
        {
            if (DataProjectInfo.IsUpdate)
                Message.Enqueue("Update available!");
            HomeProperties.Default.FavoriteList = HomeProperties.Default.FavoriteList ?? (new());
            UpdateInformation();
        }
        public ICommand OpenFolderCommand =>
            new RelayCommand((obj) =>
            {
                Edit.openUrl(BaseModel.AppPath);
            });
        public ICommand ExitCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    BaseModel.cts.Cancel();
                    BaseService.BrowserExit();
                }).Wait(5000);
                Application.Current.Shutdown();
            });
        //favorite
        public ICommand AddFavoriteCommand =>
            new RelayCommand((obj) =>
            {
                string itemName = string.Empty;
                if (obj is DataOrder)
                {
                    var item = obj as DataOrder;
                    itemName = item.ItemName;
                }
                else if (obj is DataParser)
                {
                    var item = obj as DataParser;
                    itemName = item.ItemName;
                }
                else if (obj is string)
                {
                    itemName = (string)obj;
                    if (string.IsNullOrEmpty(itemName))
                        return;
                }
                if (!HomeProperties.Default.FavoriteList.Contains(itemName))
                {
                    HomeProperties.Default.FavoriteList.Add(itemName);
                    HomeProperties.Default.Save();
                    FavoriteList = HomeProperties.Default.FavoriteList;
                    Message.Enqueue("Success. Item added to Favorites");
                }
            });
        void UpdateInformation()
        {
            Task.Run(() => {
                while (true)
                {
                    MainInfo = new();
                    if (SettingsProperties.Default.SetHours)
                    {
                        App app = (App)Application.Current;
                        Application.Current.Dispatcher.Invoke(() => { app.AutoChangeTheme(); });                        
                    }
                    System.Threading.Thread.Sleep(1500);
                }
            });
        }
    }
}