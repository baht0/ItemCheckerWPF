using ItemChecker.Properties;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class DataNotification
    {
        public bool IsRead { get; set; } = false;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
    }
    public class DataSavedList
    {
        public string ItemName { get; set; }
        public string ListName { get; set; }
        public int ServiceId { get; set; } = -1;

        public static List<DataSavedList> Items { get; set; } = ReadSavedList().ToObject<List<DataSavedList>>();

        static JArray ReadSavedList()
        {
            string path = ProjectInfo.DocumentPath + "SavedList.json";
            if (File.Exists(path))
                return JArray.Parse(File.ReadAllText(path));
            else
            {
                File.Create(path);
                return new();
            }
        }
        public static Boolean Add(string itemName, string listName, int serviceId = -1)
        {
            if (String.IsNullOrEmpty(itemName))
                return false;
            DataSavedList data = new()
            {
                ItemName = itemName,
                ListName = listName,
                ServiceId = serviceId
            };
            if (IsAllow(data))
            {
                Items.Add(data);
                Save();
                return true;
            }
            return false;
        }
        public static void Save()
        {
            string path = ProjectInfo.DocumentPath + "SavedList.json";
            JArray saved = JArray.FromObject(Items);

            File.WriteAllText(path, saved.ToString());
        }
        public static void Clear(string listName)
        {
            Items = Items.Where(x => x.ListName != listName).ToList();
            Save();
        }
        static Boolean IsAllow(DataSavedList item)
        {
            if (item != null && !Items.Any(x => x == item) && SteamBase.ItemList.Any(x => x.ItemName == item.ItemName) && !item.ItemName.Contains("Doppler"))
            {
                switch (item.ListName)
                {
                    case "favorite":
                        return Items.Select(x => x.ServiceId == item.ServiceId).Count() < 200;
                    case "rare":
                        string type = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName).Type;
                        return type == "Weapon" || type == "Knife" || type == "Gloves";
                    default:
                        return false;
                }
            }
            return false;
        }
    }
}
