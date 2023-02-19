using ItemChecker.Core;
using ItemChecker.Net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ItemChecker.MVVM.Model
{
    public class InventoryConfig : ObservableObject
    {
        public bool AllAvailable { get; set; }
        public bool SelectedOnly { get; set; } = true;
        public List<string> SellingPrice => new()
                {
                    "LowestSellOrder",
                    "HighestBuyOrder",
                    "Custom",
                };
        public int SellingPriceId { get; set; }
        public decimal Price { get; set; }

        public List<string> Tasks => new()
                {
                    "TradeOffers",
                    "QuickSell",
                };
        public int TaskId
        {
            get
            {
                return _taskId;
            }
            set
            {
                _taskId = value;
                OnPropertyChanged();
            }
        }
        int _taskId;
    }
    public class ToolInventory : InventoryConfig
    {
        public static CancellationTokenSource CTSource { get; set; } = new();
        public CancellationToken CToken { get; set; } = CTSource.Token;

        public ObservableCollection<DataInventory> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<DataInventory> _items = new();
        public DataInventory SelectedItem
        {
            get
            {
                if (_selectedItem == null)
                    return Items.FirstOrDefault();
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }
        DataInventory _selectedItem = new();
        public decimal SumOfItems
        {
            get
            {
                return _sumOfItems;
            }
            set
            {
                _sumOfItems = value;
                OnPropertyChanged();
            }
        }
        decimal _sumOfItems;
        public bool IsService
        {
            get
            {
                return _isService;
            }
            set
            {
                _isService = value;
                OnPropertyChanged();
            }
        }
        bool _isService;
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }
        int _progress = 0;
        public int MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged();
            }
        }
        int _maxProgress = 0;
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }
        bool _isBusy;

        public void UpdateInventory()
        {
            try
            {
                IsBusy = true;
                var items = InventoryService.CheckInventory();
                Items = new(items);
                SumOfItems = InventoryService.GetSumOfItems(items);
                SelectedItem = Items.FirstOrDefault();
            }
            catch (Exception ex)
            {
                BaseModel.ErrorLog(ex, true);
            }
            finally
            {
                IsBusy = false;
                TaskId = 0;
                Main.Message.Enqueue("Steam Inventory updated.");
            }
        }

        public void StartTask()
        {
            if (!IsService)
            {
                IsService = true;
                CTSource = new();
                CToken = CTSource.Token;

                switch (TaskId)
                {
                    case 0:
                        Task.Run(TradeOffer);
                        break;
                    case 1:
                        Task.Run(QuickSell);
                        break;
                }
            }
            else
            {
                CTSource.Cancel();
                IsService = false;
            }
        }
        void TradeOffer()
        {
            try
            {
                var trades = InventoryService.CheckOffer();
                while (trades.Any())
                {
                    Progress = 0;
                    MaxProgress = trades.Count;
                    foreach (var offer in trades)
                    {
                        try
                        {
                            SteamRequest.Post.AcceptTrade(offer.TradeOfferId, offer.PartnerId);
                        }
                        catch (Exception exp)
                        {
                            BaseModel.ErrorLog(exp, false);
                        }
                        finally
                        {
                            Progress++;
                            Thread.Sleep(1000);
                        }
                        if (CToken.IsCancellationRequested)
                            break;
                    }
                    trades = InventoryService.CheckOffer();
                }
            }
            catch (Exception exp)
            {
                CTSource.Cancel();
                BaseModel.ErrorLog(exp, true);
            }
            finally
            {
                IsService = false;
                Main.Message.Enqueue("Accept trades has finished.");
            }
        }
        void QuickSell()
        {
            try
            {
                var items = InventoryService.CheckInventory();
                Items = new(items);

                items = SelectedOnly ? items.Where(x => x.ItemName == SelectedItem.ItemName).ToList() : items;
                Progress = 0;
                MaxProgress = items.Count;

                foreach (var item in items)
                {
                    try
                    {
                        InventoryService.SellItem(item, this);
                    }
                    catch (Exception exp)
                    {
                        BaseModel.ErrorLog(exp, false);
                    }
                    finally
                    {
                        Progress++;
                        Thread.Sleep(1500);
                    }
                    if (CToken.IsCancellationRequested)
                        break;
                }
            }
            catch (Exception exp)
            {
                CTSource.Cancel();
                BaseModel.ErrorLog(exp, true);
            }
            finally
            {
                SelectedItem = Items.Any() ? Items.FirstOrDefault() : new();
                IsService = false;
                Main.Message.Enqueue("Quick sell items has finished.");
            }
        }
    }
    public class InventoryService
    {
        static List<DataInventory> inventory { get; set; } = new();
        static decimal sumOfItems { get; set; }

        public static List<DataInventory> CheckInventory()
        {
            var json = SteamRequest.Get.Request("http://steamcommunity.com/my/inventory/json/730/2");
            JObject rgInventory = (JObject)JObject.Parse(json)["rgInventory"];
            JObject rgDescriptions = (JObject)JObject.Parse(json)["rgDescriptions"];

            List<DataInventory> inventory = new();
            foreach (var jObject in rgInventory)
            {
                string assetid = jObject.Value["id"].ToString();
                string classid = jObject.Value["classid"].ToString();
                string instanceid = jObject.Value["instanceid"].ToString();

                JObject jsonItem = (JObject)rgDescriptions[$"{classid}_{instanceid}"];

                string name = jsonItem["market_name"].ToString();
                bool marketable = (int)jsonItem["marketable"] != 0;
                if (!marketable) continue;
                bool tradable = (int)jsonItem["tradable"] != 0;
                bool nameTag = jsonItem.ContainsKey("fraudwarnings");
                bool stickers = jsonItem["descriptions"].ToString().Contains("sticker_info");
                DateTime tradeLock = jsonItem.ContainsKey("cache_expiration") ? (DateTime)jsonItem["cache_expiration"] : new();

                DataInventory item = inventory.FirstOrDefault(x => x.ItemName == name);
                item ??= new()
                {
                    ItemName = name
                };

                item.Data.Add(new()
                {
                    AssetId = assetid,
                    ClassId = classid,
                    InstanceId = instanceid,
                    Tradable = tradable,
                    TradeLock = tradeLock,
                    Marketable = marketable,
                    Stickers = stickers,
                    NameTag = nameTag,
                });
                item.Data.OrderBy(d => d.TradeLock);

                if (!inventory.Any(x => x.ItemName == name))
                    inventory.Add(item);
            }
            return inventory;
        }
        public static decimal GetSumOfItems(List<DataInventory> items = null)
        {
            items ??= CheckInventory();

            bool same = inventory.Count == items.Count
                && inventory.Select(x => x.ItemName).ToHashSet().SetEquals(items.Select(x => x.ItemName))
                && inventory.Select(x => x.Data.Count).ToHashSet().SetEquals(items.Select(x => x.Data.Count));

            if (!same)
            {
                sumOfItems = 0;
                inventory = items;
                foreach (var item in items)
                {
                    if (item.Data.Any(x => x.Marketable))
                    {
                        var baseItem = ItemsBase.List.FirstOrDefault(x => x.ItemName == item.ItemName);
                        var json = SteamRequest.Get.ItemOrdersHistogram(baseItem.ItemName, baseItem.Steam.Id, 1);
                        var price = Convert.ToDecimal(json["highest_buy_order"]) / 100;
                        sumOfItems += price * item.Data.Count;
                    }
                }
            }
            return sumOfItems;
        }
        public static void SellItem(DataInventory inventoryItem, ToolInventory config)
        {
            ItemBaseService.UpdateSteamItem(inventoryItem.ItemName, SteamAccount.Currency.Id);
            var baseItem = ItemsBase.List.FirstOrDefault(x => x.ItemName == inventoryItem.ItemName).Steam;

            foreach (var item in inventoryItem.Data)
            {
                if (!item.Marketable || item.Stickers || item.NameTag)
                    return;

                decimal price = 0;
                switch (config.SellingPriceId)
                {
                    case 0:
                        price = baseItem.LowestSellOrder;
                        break;
                    case 1:
                        price = baseItem.HighestBuyOrder;
                        break;
                    case 2:
                        price = config.Price;
                        break;
                }
                int sellPrice = (int)((price * 100 - 0.01m) * Calculator.CommissionSteam);
                SteamRequest.Post.SellItem(item.AssetId, sellPrice);
            }
        }

        public static List<DataTradeOffer> CheckOffer()
        {
            var offers = new List<DataTradeOffer>();
            var json = SteamRequest.Get.TradeOffers();
            var trades = (JArray)json["response"]["trade_offers_received"];
            foreach (var trade in trades)
            {
                if (trade["trade_offer_state"].ToString() == "2")
                {
                    offers.Add(new()
                    {
                        TradeOfferId = trade["tradeofferid"].ToString(),
                        PartnerId = trade["accountid_other"].ToString()
                    });
                }
                else
                    continue;
            }
            return offers;
        }
    }
}
