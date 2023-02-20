using ItemChecker.Core;
using ItemChecker.Support;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ItemChecker.MVVM.Model
{
    public class ParserConfig : ObservableObject
    {
        public DateTime CheckedTime { get; set; } = DateTime.Now;
        public int MinPrice { get; set; }
        public int MaxPrice
        {
            get
            {
                return _maxPrice;
            }
            set
            {
                _maxPrice = value;
                OnPropertyChanged();
            }
        }
        int _maxPrice;

        public bool All
        {
            get
            {
                return _all;
            }
            set
            {
                _all = value;
                OnPropertyChanged();
            }
        }
        bool _all;
        public bool NotWeapon { get; set; }
        public bool Normal { get; set; }
        public bool Souvenir { get; set; }
        public bool Stattrak { get; set; }
        public bool KnifeGlove { get; set; }
        public bool KnifeGloveStattrak { get; set; }

        public int ServiceOne { get; set; }
        public int ServiceTwo { get; set; }

        public void SetMaxPrice(int serviceId)
        {
            All = false;
            switch (serviceId)
            {
                case 0 or 1:
                    var steamUsd = Currency.ConverterToUsd(SteamAccount.Balance, SteamAccount.Currency.Id);
                    MaxPrice = (int)Math.Ceiling(steamUsd);
                    break;
                case 2:
                    MaxPrice = (int)Math.Ceiling(ServiceAccount.Csm.Balance);
                    break;
                case 3:
                    MaxPrice = (int)Math.Ceiling(ServiceAccount.Lfm.Balance);
                    break;
                case 4 or 5:
                    MaxPrice = (int)Math.Ceiling(ServiceAccount.Buff.Balance);
                    All = true;
                    break;
            }
        }
        protected ParserConfig Config => (ParserConfig)this.MemberwiseClone();
    }
    public class ToolParser : ParserConfig
    {
        //timer
        public static CancellationTokenSource CTSource { get; set; } = new();
        public CancellationToken CToken { get; set; } = CTSource.Token;
        public System.Timers.Timer Timer { get; set; } = new(1000);

        //info
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                OnPropertyChanged();
            }
        }
        bool _isVisible;
        public string Service1
        {
            get
            {
                return _service1;
            }
            set
            {
                _service1 = value;
                OnPropertyChanged();
            }
        }
        string _service1 = "Service1";
        public string Service2
        {
            get
            {
                return _service2;
            }
            set
            {
                _service2 = value;
                OnPropertyChanged();
            }
        }
        string _service2 = "Service2";
        public string TableToolTip
        {
            get
            {
                return _tableToolTip;
            }
            set
            {
                _tableToolTip = value;
                OnPropertyChanged();
            }
        }
        string _tableToolTip;
        public DateTime DateTime
        {
            get
            {
                return _dateTime;
            }
            set
            {
                _dateTime = value;
                OnPropertyChanged();
            }
        }
        DateTime _dateTime = DateTime.MinValue;

        //progress
        public bool IsParser
        {
            get
            {
                return _isParser;
            }
            set
            {
                _isParser = value;
                OnPropertyChanged();
            }
        }
        bool _isParser;
        public int CountList
        {
            get { return _countList; }
            set
            {
                _countList = value;
                OnPropertyChanged();
            }
        }
        int _countList = 0;
        public int CurrentProgress
        {
            get { return _currentProgress; }
            set
            {
                _currentProgress = value;
                OnPropertyChanged();
            }
        }
        int _currentProgress;
        public int MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged();
            }
        }
        int _maxProgress;
        public string StatusStr
        {
            get { return _statusStr; }
            set
            {
                _statusStr = value;
                OnPropertyChanged();
            }
        }
        string _statusStr;
        public bool TimerOn
        {
            get { return _timerOn; }
            set
            {
                _timerOn = value;
                OnPropertyChanged();
            }
        }
        bool _timerOn;
        public bool IsStoped { get; set; }

        public static List<DataParser> Items { get; set; } = new();
        public void Start(List<DataParser> list = null)
        {
            if (!IsParser)
            {
                Items = list ?? new();
                CheckedTime = DateTime.Now;
                CTSource = new();
                CToken = CTSource.Token;
                View();
                Task.Run(MainTask);
            }
            else if (StatusStr != "Create List...")
            {
                CTSource.Cancel();
                IsParser = false;
                IsStoped = true;
                StatusStr = string.Empty;
            }
        }
        public List<DataParser> Import(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) && !File.Exists(filePath))
                return new();

            var text = File.ReadAllText(filePath);
            var json = JObject.Parse(text);
            var data = JsonConvert.DeserializeObject<List<DataParser>>(json["Items"].ToString());

            var config = json["ParserCheckConfig"].ToObject<ParserConfig>();

            View();
            IsParser = false;
            TimerOn = false;
            return data;
        }

        void MainTask()
        {
            try
            {
                StatusStr = "Create List...";
                switch (ServiceOne)
                {
                    case 0 or 1:
                        ItemBaseService.UpdateSteam();
                        break;
                    case 2:
                        ItemBaseService.UpdateCsm(Config);
                        break;
                    case 3:
                        ItemBaseService.UpdateLfm();
                        break;
                    case 4 or 5:
                        ItemBaseService.UpdateBuff(Config, 1);
                        break;
                }
                var checkList = new List<string>();
                switch (ServiceTwo)
                {
                    case 3:
                        ItemBaseService.UpdateLfm();
                        checkList = ApplyConfig(Items);
                        break;
                    case 4 or 5:
                        checkList = ApplyConfig(Items);
                        if (ItemsBase.List.Count / 80 < checkList.Count)
                            ItemBaseService.UpdateBuff(Config, 2);
                        break;
                    default:
                        checkList = ApplyConfig(Items);
                        break;
                }
                CountList = checkList.Count;
                CurrentProgress = 0;
                MaxProgress = checkList.Count;

                if (checkList.Any())
                    CheckList(checkList);
                else
                {
                    MessageBox.Show("Nothing found. Adjust the parameters.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Main.Notifications.Add(new()
                {
                    Title = "Parser",
                    Message = "The Parser has finished."
                });
            }
            catch (Exception exp)
            {
                CTSource.Cancel();
                BaseModel.ErrorLog(exp, true);
            }
            finally
            {
                if (Items.Any())
                    Export();
                TimerOn = false;
                IsParser = false;
                DataGridParse.CanBeUpdated = true;
            }
        }
        void View()
        {
            StatusStr = "Preparation...";
            TimerOn = true;
            IsParser = true;
            IsStoped = false;
            DateTime = CheckedTime;
            TableToolTip = ServiceOne == 0 || ServiceOne == 1 ? "Press 'Insert' to add to queue\nPress 'F' to add to Reserve\nPress 'F+Ctrl' to remove from Reserve" : null;
            Service1 = BaseModel.Services[ServiceOne];
            Service2 = BaseModel.Services[ServiceTwo];
            IsVisible = true;
        }
        List<string> ApplyConfig(List<DataParser> checkedList)
        {
            List<string> list = new();
            foreach (var item in ItemsBase.List)
            {
                string itemName = item.ItemName;
                //standart
                if (list.Any(x => x == itemName) || item.Steam.Id == 0)
                    continue;
                if (itemName.Contains("Doppler") || SteamAccount.Orders.Any(x => x.ItemName == itemName) || checkedList.Any(x => x.ItemName == itemName))
                    continue;
                //have
                if ((ServiceOne == 1 && !item.Steam.IsHave) || (ServiceOne == 2 && !item.Csm.Inventory.Any()) || (ServiceOne == 3 && !item.Lfm.IsHave) || (ServiceOne == 4 && !item.Buff.IsHave) || (ServiceOne == 5 && !item.Buff.IsHave))
                    continue;
                //Unavailable
                if (ServiceTwo == 2 && (item.Csm.Status == ItemStatus.Unavailable || item.Csm.Status == ItemStatus.Overstock))
                    continue;
                //Overstock
                if (ServiceTwo == 3 && (item.Lfm.Status == ItemStatus.Unavailable || item.Lfm.Status == ItemStatus.Overstock))
                    continue;
                //Price
                if (MinPrice != 0)
                {
                    if (ServiceOne < 2 && (item.Steam.AvgPrice == 0 || item.Steam.AvgPrice < MinPrice))
                        continue;
                    else if (ServiceOne == 2 && ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Csm.Inventory.Select(x => x.Price).DefaultIfEmpty().Min() < MinPrice)
                        continue;
                    else if (ServiceOne == 3 && item.Lfm.Price < MinPrice)
                        continue;
                    else if (ServiceOne == 4 && item.Buff.BuyOrder < MinPrice)
                        continue;
                    else if (ServiceOne == 5 && item.Buff.Price < MinPrice)
                        continue;
                }
                if (MaxPrice != 0)
                {
                    if (ServiceOne < 2 && (item.Steam.AvgPrice == 0 || item.Steam.AvgPrice > MaxPrice))
                        continue;
                    else if (ServiceOne == 2 && ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Csm.Inventory.Select(x => x.Price).DefaultIfEmpty().Min() > MaxPrice)
                        continue;
                    else if (ServiceOne == 3 && item.Lfm.Price > MaxPrice)
                        continue;
                    else if (ServiceOne == 4 && item.Buff.BuyOrder > MaxPrice)
                        continue;
                    else if (ServiceOne == 5 && item.Buff.Price > MaxPrice)
                        continue;
                }
                //add
                if (NotWeapon && item.Type != Type.Weapon && item.Type != Type.Knife && item.Type != Type.Gloves)
                    list.Add(itemName);
                else if (Normal && item.Type == Type.Weapon && !itemName.Contains("Souvenir") && !itemName.Contains("StatTrak™"))
                    list.Add(itemName);
                else if (Souvenir && item.Type == Type.Weapon && itemName.Contains("Souvenir"))
                    list.Add(itemName);
                else if (Stattrak && item.Type == Type.Weapon && itemName.Contains("StatTrak™"))
                    list.Add(itemName);
                else if (KnifeGlove && (item.Type == Type.Knife | item.Type == Type.Gloves))
                    list.Add(itemName);
                else if (KnifeGloveStattrak && (item.Type == Type.Knife | item.Type == Type.Gloves) && itemName.Contains("StatTrak™"))
                    list.Add(itemName);
                else if (!Normal && !Souvenir && !Stattrak && !KnifeGlove && !KnifeGloveStattrak)
                    list.Add(itemName);
            }
            return list;
        }
        void CheckList(List<string> checkList)
        {
            var start = DateTime.Now;
            Timer.Elapsed += (sender, e) => TimerTick(checkList.Count, start);
            Timer.Enabled = true;

            foreach (string itemName in checkList)
            {
                try
                {
                    var data = CheckItem(itemName, ServiceOne, ServiceTwo);
                    Items.Add(data);
                }
                catch (Exception exp)
                {
                    if (!exp.Message.Contains("429"))
                        BaseModel.ErrorLog(exp, true);
                    else
                        MessageBox.Show(exp.Message + "\n\nTo continue, you need to change the IP address.", "Parser stoped!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    IsStoped = true;
                    CTSource.Cancel();
                }
                finally
                {
                    CurrentProgress++;
                    Thread.Sleep(100);
                }
                if (CToken.IsCancellationRequested)
                    break;
            }
            Thread.Sleep(1000);
            Timer.Enabled = false;
            Timer.Elapsed -= (sender, e) => TimerTick(checkList.Count, start);
        }
        DataParser CheckItem(string itemName, int serOneId, int serTwoId)
        {
            var data = new DataParser()
            {
                ItemName = itemName,
            };
            switch (serOneId)
            {
                case 0:
                    {
                        ItemBaseService.UpdateSteamItem(itemName);
                        var Item = ItemsBase.List.FirstOrDefault(x => x.ItemName == data.ItemName).Steam;
                        data.Purchase = Item.HighestBuyOrder;
                        data.Have = data.Purchase > 0;
                        break;
                    }
                case 1:
                    {
                        ItemBaseService.UpdateSteamItem(itemName);
                        var Item = ItemsBase.List.FirstOrDefault(x => x.ItemName == data.ItemName).Steam;
                        data.Purchase = Item.LowestSellOrder;
                        data.Have = data.Purchase > 0;
                        break;
                    }
                case 2:
                    {
                        data.Purchase = ItemsBase.List.FirstOrDefault(x => x.ItemName == data.ItemName).Csm.Inventory.Select(x => x.Price).DefaultIfEmpty().Min();
                        break;
                    }
                case 3:
                    {
                        var price = ItemsBase.List.FirstOrDefault(x => x.ItemName == data.ItemName).Lfm.Price;
                        data.Purchase = Math.Round(price * 1.03m, 2);
                        break;
                    }
                case 4:
                    {
                        var buff = ItemsBase.List.FirstOrDefault(x => x.ItemName == data.ItemName).Buff;
                        data.Purchase = buff.BuyOrder;
                        break;
                    }
                case 5:
                    {
                        var buff = ItemsBase.List.FirstOrDefault(x => x.ItemName == data.ItemName).Buff;
                        data.Purchase = buff.Price;
                        break;
                    }
            }
            switch (serTwoId)
            {
                case 0:
                    {
                        ItemBaseService.UpdateSteamItem(itemName);
                        var Item = ItemsBase.List.FirstOrDefault(x => x.ItemName == data.ItemName).Steam;
                        data.Price = Item.HighestBuyOrder;
                        data.Get = Math.Round(data.Price * Calculator.CommissionSteam, 2);
                        break;
                    }
                case 1:
                    {
                        ItemBaseService.UpdateSteamItem(itemName);
                        var Item = ItemsBase.List.FirstOrDefault(x => x.ItemName == data.ItemName).Steam;
                        data.Price = Item.LowestSellOrder;
                        data.Get = Math.Round(data.Price * Calculator.CommissionSteam, 2);
                        break;
                    }
                case 2:
                    {
                        ItemBaseService.UpdateCsmItem(itemName, false);
                        var Item = ItemsBase.List.FirstOrDefault(x => x.ItemName == data.ItemName).Csm;
                        data.Price = Item.Price;
                        data.Get = Math.Round(data.Price * Calculator.CommissionCsm, 2);
                        break;
                    }
                case 3:
                    {
                        var Item = ItemsBase.List.FirstOrDefault(x => x.ItemName == data.ItemName).Lfm;
                        data.Price = Item.Price;
                        data.Get = Math.Round(data.Price * Calculator.CommissionLf, 2);
                        break;
                    }
                case 4:
                    {
                        ItemBaseService.UpdateBuffItem(itemName);
                        var Item = ItemsBase.List.FirstOrDefault(x => x.ItemName == data.ItemName).Buff;
                        data.Price = Item.BuyOrder;
                        data.Get = Math.Round(data.Price * Calculator.CommissionBuff, 2);
                        break;
                    }
                case 5:
                    {
                        ItemBaseService.UpdateBuffItem(itemName);
                        var Item = ItemsBase.List.FirstOrDefault(x => x.ItemName == data.ItemName).Buff;
                        data.Price = Item.Price;
                        data.Get = Math.Round(data.Price * Calculator.CommissionBuff, 2);
                        break;
                    }
            }

            data.Precent = Edit.Precent(data.Purchase, data.Get);
            data.Difference = Edit.Difference(data.Get, data.Purchase);

            return data;
        }
        void TimerTick(int itemCount, DateTime timeStart)
        {
            if (CToken.IsCancellationRequested)
                Timer.Enabled = false;
            StatusStr = Edit.calcTimeLeft(timeStart, itemCount, CurrentProgress);
        }

        void Export()
        {
            string items = JsonConvert.SerializeObject(Items, Formatting.Indented);
            JObject json = new(
                new JProperty("Size", Items.Count),
                new JProperty("ParserCheckConfig", JObject.FromObject(Config)),
                new JProperty("Items", JArray.Parse(items)));

            string path = ProjectInfo.DocumentPath + "extract\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            File.WriteAllText(path + $"Parser_{CheckedTime:dd.MM_hh.mm}_{ServiceOne}{ServiceTwo}.json", json.ToString());
        }
    }
}
