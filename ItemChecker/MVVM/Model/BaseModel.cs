using ItemChecker.Core;
using ItemChecker.Net;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace ItemChecker.MVVM.Model
{
    public class BaseModel : ObservableObject
    {
        public static List<string> ServicesShort => new()
                    {
                        "SteamMarket",
                        "Cs.Money",
                        "Loot.Farm",
                        "Buff163"
                    };
        public static List<string> Services => new()
                    {
                        "SteamMarket(A)",
                        "SteamMarket",
                        "Cs.Money(T)",
                        "Loot.Farm",
                        "Buff163(A)",
                        "Buff163"
                    };
        public static List<string> Parameters => new()
                {
                    "Float", "Sticker", "Doppler"
                };

        public string CurrencySteamSymbol => SteamAccount.Currency.Symbol;
        public List<string> CurrencyList => Currencies.Allow.Select(x => x.Name).ToList();
        public DataCurrency CurrentCurrency { get; set; } = Currencies.Allow.FirstOrDefault();
        public int CurrencyId
        {
            get
            {
                return _currencyId;
            }
            set
            {
                _currencyId = value;
                OnPropertyChanged();
            }
        }
        int _currencyId = 0;

        public static void ErrorLog(Exception exp, bool isShow)
        {
            try
            {
                string info = $"v.{DataProjectInfo.CurrentVersion} [{DateTime.Now}] {exp.Message}\n{exp.StackTrace}\n";
                string file = String.IsNullOrEmpty(SteamAccount.AccountName) ? "NoAccountName.txt" : $"{SteamAccount.AccountName}.txt";
                JObject json = DropboxRequest.Post.ListFolder($"ErrorLogs");
                JArray usersLog = JArray.Parse(json["entries"].ToString());
                if (usersLog.Any(x => x["name"].ToString() == file))
                {
                    string read = DropboxRequest.Get.Read($"ErrorLogs/{file}");
                    info = string.Format("{0}{1}", info, read);
                    DropboxRequest.Post.Delete($"ErrorLogs/{file}");
                }
                DropboxRequest.Post.Upload($"ErrorLogs/{file}", info);
            }
            finally
            {
                if (isShow)
                    MessageBox.Show(exp.Message, "Something went wrong :(", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    public class BaseMainTable<T> : ObservableObject
    {
        public List<T> Items
        {
            get
            {
                return _items;
            }
            set
            {
                _items = value;
                GridView = CollectionViewSource.GetDefaultView(value);
            }
        }
        List<T> _items = new();
        public T SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }
        T _selectedItem;
        public ICollectionView GridView
        {
            get
            {
                return _gridView;
            }
            set
            {
                _gridView = value;
                OnPropertyChanged();
            }
        }
        ICollectionView _gridView;
        public int Count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = value;
                OnPropertyChanged();
            }
        }
        int _count = 0;
    }
    public class BaseTable<T> : ObservableObject
    {
        public ObservableCollection<T> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<T> _items = new();
        public T SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }
        T _selectedItem;
    }
}
