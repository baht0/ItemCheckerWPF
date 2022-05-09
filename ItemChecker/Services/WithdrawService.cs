using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;
using System;
using System.Threading;

namespace ItemChecker.Services
{
    public class WithdrawService
    {
        JObject GetStackItems(JObject item, JObject stack_item)
        {
            if (stack_item.ContainsKey("3d"))
                item["3d"] = stack_item["3d"].ToString();
            if (stack_item.ContainsKey("float"))
                item["float"] = stack_item["float"].ToString();
            if (stack_item.ContainsKey("id"))
                item["id"] = Convert.ToInt64(stack_item["id"].ToString());
            if (stack_item.ContainsKey("img"))
                item["img"] = stack_item["img"].ToString();
            if (stack_item.ContainsKey("pattern"))
                if (stack_item["pattern"].ToString() != "null")
                    item["pattern"] = Convert.ToInt32(stack_item["pattern"].ToString());
            if (stack_item.ContainsKey("preview"))
                item["preview"] = stack_item["preview"].ToString();
            if (stack_item.ContainsKey("screenshot"))
                item["screenshot"] = stack_item["screenshot"].ToString();
            if (stack_item.ContainsKey("steamId"))
                item["steamId"] = stack_item["steamId"].ToString();

            return item;
        }

        public JArray CheckInventory()
        {
            int offset = 0;
            JArray inventory = new();
            while (true)
            {
                try
                {
                    if (inventory.Count < offset)
                        break;

                    BaseModel.Browser.Navigate().GoToUrl("https://cs.money/3.0/load_user_inventory/730?limit=60&noCache=true&offset=" + offset + "&order=desc&sort=price");
                    IWebElement html = BaseModel.WebDriverWait.Until(e => e.FindElement(By.XPath("//pre")));
                    string json = html.Text;
                    json = JObject.Parse(json)["items"].ToString();

                    inventory.Merge(JArray.Parse(json));
                }
                finally
                {
                    offset += 60;
                    Thread.Sleep(1000);
                }
            }
            JArray items = new();
            if (inventory.Count > 0)
            {
                foreach (JObject item in inventory)
                {
                    if (!item.ContainsKey("tradeLock"))
                    {
                        if (item.ContainsKey("isVirtual"))
                            items.Add(item);
                    }
                }
            }
            return items;
        }
        public JArray GetItems(JArray inventory)
        {
            JArray items = new();
            foreach (JObject item in inventory)
            {
                if (item.ContainsKey("stackSize"))
                {
                    Thread.Sleep(1500);
                    BaseModel.Browser.Navigate().GoToUrl("https://cs.money/2.0/get_user_stack/730/" + item["stackId"].ToString());
                    IWebElement html = BaseModel.WebDriverWait.Until(e => e.FindElement(By.XPath("//pre")));
                    string json = html.Text;
                    JArray stack = JArray.Parse(json);

                    foreach (JObject stack_item in stack)
                        items.Add(GetStackItems(item, stack_item));
                }
                else
                    items.Add(item);
            }
            return items;
        }
        public void WithdrawItems(JObject item)
        {
            JObject json = new(
                       new JProperty("skins",
                           new JObject(
                               new JProperty("bot", new JArray(item)),
                               new JProperty("user", new JArray()))));
            string body = json.ToString(Formatting.None);
            BaseModel.Browser.ExecuteJavaScript(Post.FetchRequest("application/json", body, "https://cs.money/2.0/withdraw_skins"));
        }
    }
}
