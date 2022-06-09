using ItemChecker.Support;
using System;
using System.Collections.Generic;
using System.Linq;

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

            if (serOneId < 2 || serTwoId < 2)
                UpdateSteamInfoItem(data.ItemName);
            switch (serOneId)
            {
                case 0 or 1:
                    {
                        var Item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Steam;
                        data.Price1 = Item.LowestSellOrder;
                        data.Price2 = Item.HighestBuyOrder;
                        data.Have = data.Price1 > 0;
                        break;
                    }
                case 2:
                    {
                        data.Price1 = DataInventoriesCsm.Items.Where(x => x.ItemName == data.ItemName).Select(x => x.Price).DefaultIfEmpty().Min();
                        data.Price2 = Math.Round(data.Price1 * Calculator.CommissionCsm, 2);
                        break;
                    }
                case 3:
                    {
                        data.Price1 = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Lfm.Price;
                        data.Price2 = Math.Round(data.Price1 * Calculator.CommissionLf, 2);
                        break;
                    }
                case 4:
                    {
                        BuffInfo buff = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Buff;
                        data.Price1 = buff.Price;
                        data.Price2 = buff.BuyOrder;
                        break;
                    }
            }
            switch (serTwoId)
            {
                case 0 or 1: //st
                    {
                        var Item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Steam;
                        data.Price3 = Item.LowestSellOrder;
                        data.Price4 = Math.Round(Item.HighestBuyOrder * Calculator.CommissionSteam, 2);
                        break;
                    }
                case 2:
                    {
                        var Item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Csm;
                        data.Price3 = Item.Price;
                        data.Price4 = Math.Round(data.Price3 * Calculator.CommissionCsm, 2);
                        break;
                    }
                case 3:
                    {
                        var LfItem = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Lfm;
                        data.Price3 = LfItem.Price;
                        data.Price4 = Math.Round(data.Price3 * Calculator.CommissionLf, 2);
                        break;
                    }
                case 4:
                    {
                        var buff = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Buff;
                        data.Price3 = buff.Price;
                        data.Price4 = Math.Round(buff.BuyOrder * Calculator.CommissionBuff, 2);
                        break;
                    }
            }

            if (serOneId == 0) //sta -> (any)
            {
                data.Precent = Edit.Precent(data.Price2, data.Price4);
                data.Difference = Edit.Difference(data.Price4, data.Price2);
            }
            else //(any) -> (any)
            {
                data.Precent = Edit.Precent(data.Price1, data.Price4);
                data.Difference = Edit.Difference(data.Price4, data.Price1);
            }
            return data;
        }

        //List, 
        public static List<string> ApplyConfig(ParserCheckConfig config)
        {
            List<string> list = new();
            foreach (var item in SteamBase.ItemList)
            {
                string itemName = item.ItemName;
                //standart
                if (list.Any(x => x == itemName) || ((config.ServiceOne < 2 | config.ServiceTwo < 2) && item.Steam.Id == 0))
                    continue;
                if (itemName.Contains("Doppler") || DataOrder.Orders.Any(x => x.ItemName == itemName))
                    continue;
                //have
                if ((config.ServiceOne == 2 && !DataInventoriesCsm.Items.Any(x => x.ItemName == itemName)) || (config.ServiceOne == 3 && !item.Lfm.IsHave) || (config.ServiceOne == 4 && !item.Buff.IsHave))
                    continue;
                if ((config.ServiceTwo == 2 && !item.Csm.IsHave) || (config.ServiceTwo == 3 && !item.Lfm.IsHave) || (config.ServiceTwo == 4 && item.Buff.BuyOrder <= 0))
                    continue;
                //only
                if (config.SelectedOnly != 0)
                {
                    if (config.SelectedOnly == 1 && !DataOrder.Orders.Any(x => x.ItemName == itemName))
                        continue;
                    if (config.SelectedOnly == 2 && !DataSavedList.Items.Any(x => x.ItemName == itemName))
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
                    if (config.ServiceOne < 2 && item.Steam.AvgPrice < config.MinPrice)
                        continue;
                    else if (config.ServiceOne == 2 && DataInventoriesCsm.Items.Where(x => x.ItemName == itemName).Select(x => x.Price).DefaultIfEmpty().Min() < config.MinPrice)
                        continue;
                    else if (config.ServiceOne == 3 && item.Lfm.Price < config.MinPrice)
                        continue;
                    else if (config.ServiceOne == 4 && item.Buff.Price < config.MinPrice)
                        continue;
                }
                if (config.MaxPrice != 0)
                {
                    if (config.ServiceOne < 2 && item.Steam.AvgPrice > config.MaxPrice)
                        continue;
                    else if (config.ServiceOne == 2 && DataInventoriesCsm.Items.Where(x => x.ItemName == itemName).Select(x => x.Price).DefaultIfEmpty().Min() > config.MaxPrice)
                        continue;
                    else if (config.ServiceOne == 3 && item.Lfm.Price > config.MaxPrice)
                        continue;
                    else if (config.ServiceOne == 4 && item.Buff.Price > config.MaxPrice)
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