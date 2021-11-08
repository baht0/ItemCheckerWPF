using ItemChecker.Net;
using ItemChecker.Properties;
using Newtonsoft.Json.Linq;

namespace ItemChecker.MVVM.Model
{
    public class ParserManualService : ParserService
    {
        public void Manual(string itemName)
        {
            ParserData response = CheckItems(itemName);
            ParserData.ParserItems.Add(response);
        }
        public void GetLF()
        {
            if (ParserProperties.Default.serviceOne != 2 & ParserProperties.Default.serviceTwo != 2)
                return;

            if (jArrayLF != null)
                jArrayLF.Clear();
            ItemsLF.Clear();
            string json = Get.Request("https://loot.farm/fullprice.json");
            jArrayLF = JArray.Parse(json);

            for (int i = 0; i < jArrayLF.Count; i++)
                ItemsLF.Add((string)jArrayLF[i]["name"]);
        }
    }
}
