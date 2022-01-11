using ItemChecker.Net;
using Newtonsoft.Json.Linq;
namespace UpdateBase
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Update();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\r\n======================");
                Console.WriteLine(ex);
            }
            finally
            {
                Console.WriteLine("end: " + DateTime.Now);
                Thread.Sleep(TimeSpan.FromMinutes(10));
                Update();
            }
        }
        static void Update()
        {
            Console.WriteLine(DateTime.Now);
            List<Steam> steamList = GetSteamPriceList();

            string json = Get.Request("https://csm.auction/api/skins_base");
            JObject csm_base = JObject.Parse(json);

            json = Post.DropboxRead("SkinsBase.json");
            JArray items = JArray.Parse(json);
            JArray jArray = new();
            foreach (JObject item in items)
            {
                string name = item["Name"].ToString();
                int steamId = Convert.ToInt32(item["SteamId"]);
                int csmId = Convert.ToInt32(item["CsmId"]);

                decimal csmPrice = 0;
                if (csmId != 0)
                    csmPrice = Convert.ToDecimal(csm_base[csmId.ToString()]["a"]);

                jArray.Add(new JObject(
                    new JProperty("ItemName", name),
                    new JProperty("SteamId", steamId),
                    new JProperty("SteamPrice", steamList.Where(x => x.ItemName == name).Select(x => x.PriceSteam).FirstOrDefault()),
                    new JProperty("CsmId", csmId),
                    new JProperty("CsmPrice", csmPrice)
                ));
            }
            JObject skinsBase = new(
                   new JProperty("Updated", (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds),
                   new JProperty("Items", jArray)
                   );
            Console.WriteLine(skinsBase.ToString());

            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"\SkinsBase.txt", skinsBase.ToString());
        }
        static List<Steam> GetSteamPriceList()
        {
            List<Steam> list = new();
            int count = 20000;
            for (int i = 0; i < count; i += 100)
            {
                try
                {
                    string json = Get.Request("https://steamcommunity.com/market/search/render/?query=&appid=730&norender=1&search_descriptions=0&sort_column=price&sort_dir=desc&count=100&start=" + i);
                    count = Convert.ToInt32(JObject.Parse(json)["total_count"]);
                    JObject items = JObject.Parse(json);
                    foreach (JObject item in items["results"])
                    {
                        string name = item["name"].ToString();
                        decimal price = Convert.ToDecimal(item["sell_price"]) / 100;
                        list.Add(new Steam()
                        {
                            ItemName = name,
                            PriceSteam = price,
                        });
                    }
                }
                catch
                {
                    Console.WriteLine($"{i}/{count}_{DateTime.Now}" );
                    Thread.Sleep(TimeSpan.FromMinutes(5));
                    i -= 100;
                }
            }
            return list;
        }
    }
}
