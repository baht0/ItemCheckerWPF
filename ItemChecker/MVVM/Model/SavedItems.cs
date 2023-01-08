using ItemChecker.Core;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class SavedItems : ObservableObject
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

        public bool IsReserve
        {
            get
            {
                return _isReserve;
            }
            set
            {
                _isReserve = value;
                ServiceId = 0;
                if (value)
                {
                    List = new(Reserve);
                    Services = Main.Services;
                }
                OnPropertyChanged();
            }
        }
        bool _isReserve;
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
                    Services = Model.Rare.Services;
                }
                OnPropertyChanged();
            }
        }
        bool _isRare;
        public List<string> Services
        {
            get { return _services; }
            set
            {
                _services = value;
                OnPropertyChanged();
            }
        }
        private List<string> _services = Main.Services;
        public int ServiceId
        {
            get { return _serviceId; }
            set
            {
                _serviceId = value;
                OnPropertyChanged();
            }
        }
        private int _serviceId;

        public static Reserve<DataItem> Reserve { get; set; } =  ReadFile("Reserve").ToObject<Reserve<DataItem>>();
        public static Rare<DataItem> Rare { get; set; } = ReadFile("Rare").ToObject<Rare<DataItem>>();

        public static JArray ReadFile(string listName)
        {
            string path = ProjectInfo.DocumentPath + "SavedList.json";

            if (!File.Exists(path))
            {
                File.Create(path);
                var json = new JObject(
                    new JProperty("Reserve", new JArray()),
                    new JProperty("Rare", new JArray()));
                File.WriteAllText(path, json.ToString());
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
    public class Reserve<T> : SavedList<T>
    {
        public new bool Add(T item)
        {
            if (IsAllow(item))
            {
                base.Add(item);
                Save();
                return true;
            }
            return false;
        }
        bool IsAllow(T item)
        {
            var currentItem = item as DataItem;
            var currentList = this as Reserve<DataItem>;
            var steamBase = ItemsBase.List.FirstOrDefault(x => x.ItemName == currentItem.ItemName);

            bool isAllow = item != null;
            if (isAllow)
                isAllow = steamBase != null;
            if (isAllow)
                isAllow = !currentList.Any(x => x.ItemName == currentItem.ItemName && x.ServiceId == currentItem.ServiceId);
            if (isAllow)
                isAllow = currentList.Select(x => x.ServiceId == currentItem.ServiceId).Count() < 200;

            return isAllow;                
        }
    }
    public class Rare<T> : SavedList<T>
    {
        public new bool Add(T item)
        {
            if (IsAllow(item))
            {
                base.Add(item);
                Save();
                return true;
            }
            return false;
        }
        bool IsAllow(T item)
        {
            var currentItem = item as DataItem;
            var currentList = this as Rare<DataItem>;

            var steamBase = ItemsBase.List.FirstOrDefault(x => x.ItemName == currentItem.ItemName);
            bool isAllow = steamBase != null;
            if (isAllow)
                isAllow = steamBase.Type == "Weapon" || steamBase.Type == "Knife" || steamBase.Type == "Gloves";
            if (isAllow)
                isAllow = !currentList.Any(x => x.ItemName == currentItem.ItemName && x.ServiceId == currentItem.ServiceId);
            if (isAllow && currentItem.ServiceId == 1)
                isAllow = steamBase.Type == "Weapon";
            if (isAllow && currentItem.ServiceId == 2)
                isAllow = currentItem.ItemName.Contains("Doppler");

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
                    new JProperty("Reserve", JArray.FromObject(SavedItems.Reserve)),
                    new JProperty("Rare", JArray.FromObject(SavedItems.Rare)));

            string path = ProjectInfo.DocumentPath + "SavedList.json";
            if (!File.Exists(path))
                File.Create(path);

            File.WriteAllText(path, json.ToString());
        }
    }
}