using ItemChecker.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using ItemChecker.Services;

namespace ItemChecker.MVVM.Model
{
    public class ParserCheckService : ParserService
    {
        public DataParser Check(string itemName, int serOneId, int serTwoId)
        {
            DataParser data = new()
            {
                ItemName = itemName,
            };

            ItemBaseService itemBaseService = new();
            switch (serOneId)
            {
                case 0:
                    {
                        itemBaseService.UpdateSteamItem(itemName);
                        var Item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Steam;
                        data.Purchase = Item.HighestBuyOrder;
                        data.Have = data.Purchase > 0;
                        break;
                    }
                case 1:
                    {
                        itemBaseService.UpdateSteamItem(itemName);
                        var Item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Steam;
                        data.Purchase = Item.LowestSellOrder;
                        data.Have = data.Purchase > 0;
                        break;
                    }
                case 2:
                    {
                        data.Purchase = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Csm.Inventory.Select(x => x.Price).DefaultIfEmpty().Min();
                        break;
                    }
                case 3:
                    {
                        var price = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Lfm.Price;
                        data.Purchase = Math.Round(price * 1.03m, 2);
                        break;
                    }
                case 4:
                    {
                        var buff = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Buff;
                        data.Purchase = buff.BuyOrder;
                        break;
                    }
                case 5:
                    {
                        var buff = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Buff;
                        data.Purchase = buff.Price;
                        break;
                    }
            }
            switch (serTwoId)
            {
                case 0:
                    {
                        itemBaseService.UpdateSteamItem(itemName);
                        var Item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Steam;
                        data.Price = Item.HighestBuyOrder;
                        data.Get = Math.Round(data.Price * Calculator.CommissionSteam, 2);
                        break;
                    }
                case 1:
                    {
                        itemBaseService.UpdateSteamItem(itemName);
                        var Item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Steam;
                        data.Price = Item.LowestSellOrder;
                        data.Get = Math.Round(data.Price * Calculator.CommissionSteam, 2);
                        break;
                    }
                case 2:
                    {
                        itemBaseService.UpdateCsmItem(itemName, false);
                        var Item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Csm;
                        data.Price = Item.Price;
                        data.Get = Math.Round(data.Price * Calculator.CommissionCsm, 2);
                        break;
                    }
                case 3:
                    {
                        var Item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Lfm;
                        data.Price = Item.Price;
                        data.Get = Math.Round(data.Price * Calculator.CommissionLf, 2);
                        break;
                    }
                case 4:
                    {
                        itemBaseService.UpdateBuffItem(itemName);
                        var Item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Buff;
                        data.Price = Item.BuyOrder;
                        data.Get = Math.Round(data.Price * Calculator.CommissionBuff, 2);
                        break;
                    }
                case 5:
                    {
                        itemBaseService.UpdateBuffItem(itemName);
                        var Item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Buff;
                        data.Price = Item.Price;
                        data.Get = Math.Round(data.Price * Calculator.CommissionBuff, 2);
                        break;
                    }
            }

            data.Precent = Edit.Precent(data.Purchase, data.Get);
            data.Difference = Edit.Difference(data.Get, data.Purchase);

            return data;
        }

        //List, 
        public static List<string> ApplyConfig(ParserCheckConfig config, List<DataParser> checkedList)
        {
            List<string> list = new();
            foreach (var item in SteamBase.ItemList)
            {
                string itemName = item.ItemName;
                //standart
                if (list.Any(x => x == itemName) || item.Steam.Id == 0)
                    continue;
                if (itemName.Contains("Doppler") || SteamMarket.Orders.Any(x => x.ItemName == itemName) || checkedList.Any(x => x.ItemName == itemName))
                    continue;
                //have
                if ((config.ServiceOne == 1 && !item.Steam.IsHave) || (config.ServiceOne == 2 && !item.Csm.Inventory.Any()) || (config.ServiceOne == 3 && !item.Lfm.IsHave) || (config.ServiceOne == 4 && !item.Buff.IsHave) || (config.ServiceOne == 5 && !item.Buff.IsHave))
                    continue;
                //only
                if (config.SelectedOnly != 0)
                {
                    if (config.SelectedOnly == 1 && !SteamMarket.Orders.Any(x => x.ItemName == itemName))
                        continue;
                    if (config.SelectedOnly == 2 && !ItemsList.Favorite.Any(x => x.ItemName == itemName))
                        continue;
                }
                //Unavailable
                if ((config.ServiceTwo == 2 && item.Csm.Unavailable) || (config.ServiceTwo == 3 && item.Lfm.Unavailable))
                    continue;
                //Overstock
                if ((config.ServiceTwo == 2 && item.Csm.Overstock) || (config.ServiceTwo == 3 && item.Lfm.Overstock))
                    continue;
                //Price
                if (config.MinPrice != 0)
                {
                    if (config.ServiceOne < 2 && item.Steam.AvgPrice != 0 && item.Steam.AvgPrice < config.MinPrice)
                        continue;
                    else if (config.ServiceOne == 2 && SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Csm.Inventory.Select(x => x.Price).DefaultIfEmpty().Min() < config.MinPrice)
                        continue;
                    else if (config.ServiceOne == 3 && item.Lfm.Price < config.MinPrice)
                        continue;
                    else if (config.ServiceOne == 4 && item.Buff.BuyOrder < config.MinPrice)
                        continue;
                    else if (config.ServiceOne == 5 && item.Buff.Price < config.MinPrice)
                        continue;
                }
                if (config.MaxPrice != 0)
                {
                    if (config.ServiceOne < 2 && item.Steam.AvgPrice != 0 && item.Steam.AvgPrice > config.MaxPrice)
                        continue;
                    else if (config.ServiceOne == 2 && SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Csm.Inventory.Select(x => x.Price).DefaultIfEmpty().Min() > config.MaxPrice)
                        continue;
                    else if (config.ServiceOne == 3 && item.Lfm.Price > config.MaxPrice)
                        continue;
                    else if (config.ServiceOne == 4 && item.Buff.BuyOrder > config.MaxPrice)
                        continue;
                    else if (config.ServiceOne == 5 && item.Buff.Price > config.MaxPrice)
                        continue;
                }
                //add
                if (config.NotWeapon && item.Type != "Weapon" && item.Type != "Knife" && item.Type != "Gloves")
                    list.Add(itemName);
                else if (config.Normal && item.Type == "Weapon" && !itemName.Contains("Souvenir") && !itemName.Contains("StatTrak™"))
                    list.Add(itemName);
                else if (config.Souvenir && item.Type == "Weapon" && itemName.Contains("Souvenir"))
                    list.Add(itemName);
                else if (config.Stattrak && item.Type == "Weapon" && itemName.Contains("StatTrak™"))
                    list.Add(itemName);
                else if (config.KnifeGlove && (item.Type == "Knife" | item.Type == "Gloves"))
                    list.Add(itemName);
                else if (config.KnifeGloveStattrak && (item.Type == "Knife" | item.Type == "Gloves") && itemName.Contains("StatTrak™"))
                    list.Add(itemName);
                else if (!config.Normal && !config.Souvenir && !config.Stattrak && !config.KnifeGlove && !config.KnifeGloveStattrak)
                    list.Add(itemName);
            }
            return list;
        }
    }
}