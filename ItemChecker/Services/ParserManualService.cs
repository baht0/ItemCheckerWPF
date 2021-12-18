using ItemChecker.Properties;
using System.Collections.Generic;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class ParserManualService : ParserService
    {
        public void Manual(string itemName)
        {
            DataParser data = CheckList(itemName);
            DataParser.ParserItems.Add(data);
        }
        //CheckList
        public List<string> SelectFile()
        {
            List<string> list = OpenFileDialog("txt");
            if (list.Any())
                list = clearPrices(list);

            return list;
        }
        public List<string> ApplyConfig(ParserList config)
        {
            List<string> list = new();

            foreach (string item in ParserProperties.Default.CheckList)
            {
                if (config.ServiceTwo == 1)
                {
                    if (ItemBase.Unavailable.Any(x => x.ItemName == item))
                        continue;
                }
                if (config.ServiceOne == 1)
                {
                    if (DataInventoryCsm.Inventory.Where(x => x.ItemName == item).Select(x => x.Price).Min() > Account.BalanceCsmUsd)
                        continue;
                }
                if (!config.OverstockM)
                {
                    if (ItemBase.Overstock.Any(x => x.ItemName == item) & config.ServiceTwo == 1)
                        continue;
                    else if (DataInventoryLf.Inventory.Where(x => x.ItemName == item).Select(x => x.IsOverstock).FirstOrDefault() & config.ServiceTwo == 2)
                        continue;
                }
                if (config.SouvenirM & item.Contains("Souvenir"))
                    list.Add(item);
                else if (config.StattrakM & item.Contains("StatTrak™"))
                    list.Add(item);
                else if (config.KnifeGloveM & item.Contains("★ "))
                    list.Add(item);
                else if (config.KnifeGloveStattrakM & item.Contains("★ StatTrak™"))
                    list.Add(item);
            }
            return list;
        }
    }
}