using ItemChecker.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class ItemsList : ObservableObject
    {
        public ObservableCollection<DataItem> List
        {
            get { return _list; }
            set
            {
                _list = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<DataItem> _list = new();
        public DataItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }
        private DataItem _selectedItem;

        bool _isFavorite;
        bool _isRare;
        public bool IsFavorite
        {
            get
            {
                return _isFavorite;
            }
            set
            {
                _isFavorite = value;
                ServiceId = 0;
                if (value)
                {
                    List = new(Favorite);
                    Services = Main.Services;
                }
                OnPropertyChanged();
            }
        }
        public bool IsRare
        {
            get
            {
                return _isRare;
            }
            set
            {
                _isRare = value;
                ServiceId = 0;
                if (value)
                {
                    List = new(Rare);
                    Services = new()
                    {
                        "Float",
                        "Sticker",
                        "Doppler (Soon)"
                    };
                }
                OnPropertyChanged();
            }
        }
        private List<string> _services = Main.Services;
        public List<string> Services
        {
            get { return _services; }
            set
            {
                _services = value;
                OnPropertyChanged();
            }
        }
        private int _serviceId;
        public int ServiceId
        {
            get { return _serviceId; }
            set
            {
                _serviceId = value;
                OnPropertyChanged();
            }
        }

        public static Favorite<DataItem> Favorite { get; set; } =  ReadFile("Favorite").ToObject<Favorite<DataItem>>();
        public static Rare<DataItem> Rare { get; set; } = ReadFile("Rare").ToObject<Rare<DataItem>>();

        public static JArray ReadFile(string listName)
        {
            string path = ProjectInfo.DocumentPath + "SavedList.json";

            if (!File.Exists(path))
            {
                File.Create(path);
                File.WriteAllText(path, "{}");
                return new();
            }
            JObject obj = JObject.Parse(File.ReadAllText(path));

            return obj[listName] as JArray;
        }
    }
    public class DataItem
    {
        public string ItemName { get; set; } = "Unknown";
        public int ServiceId { get; set; } = -1;

        public DataItem(string itemName, int serviceId)
        {
            this.ItemName = itemName;
            this.ServiceId = serviceId;
        }
    }
    public class Favorite<T> : SavedList<T>
    {
        public new Boolean Add(T item)
        {
            var currentItem = item as DataItem;
            if (IsAllow(currentItem))
            {
                base.Add(item);
                Save();
                return true;
            }
            return false;
        }
        Boolean IsAllow(DataItem item)
        {
            var currentList = this as Favorite<DataItem>;
            var steamBase = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName);

            bool isAllow = item != null;
            if (isAllow)
                isAllow = steamBase != null;
            if (isAllow)
                isAllow = !currentList.Any(x => x.ItemName == item.ItemName && x.ServiceId == item.ServiceId);
            if (isAllow)
                isAllow = currentList.Select(x => x.ServiceId == item.ServiceId).Count() < 200;

            return isAllow;                
        }
    }
    public class Rare<T> : SavedList<T>
    {
        public new Boolean Add(T item)
        {
            var currentItem = item as DataItem;
            if (IsAllow(currentItem))
            {
                base.Add(item);
                Save();
                return true;
            }
            return false;
        }
        Boolean IsAllow(DataItem item)
        {
            var currentList = this as Rare<DataItem>;
            var steamBase = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName);

            bool isAllow = item != null;

            if (isAllow)
                isAllow = steamBase != null && (steamBase.Type == "Weapon" || steamBase.Type == "Knife" || steamBase.Type == "Gloves");
            if (isAllow)
                isAllow = !currentList.Any(x => x.ItemName == item.ItemName && x.ServiceId == item.ServiceId);

            return isAllow;                
        }
    }
    public class SavedList<T> : List<T>
    {
        public new void Remove(T item)
        {
            base.Remove(item);
            Save();
        }
        public new void Clear()
        {
            base.Clear();
            Save();
        }
        protected void Save()
        {
            JObject json = new(
                    new JProperty("Favorite", JArray.FromObject(ItemsList.Favorite)),
                    new JProperty("Rare", JArray.FromObject(ItemsList.Rare)));

            string path = ProjectInfo.DocumentPath + "SavedList.json";
            if (!File.Exists(path))
                File.Create(path);

            File.WriteAllText(path, json.ToString());
        }
    }
}