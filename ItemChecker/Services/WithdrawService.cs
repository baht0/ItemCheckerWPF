using ItemChecker.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using System.Threading;

namespace ItemChecker.Services
{
    public class WithdrawService : FavoriteCheckService
    {
        public JArray checkInventory()
        {
            int offset = 0;
            JArray inventory = new();
            while (true)
            {
                try
                {
                    if (inventory.Count < offset)
                        break;

                    Browser.Navigate().GoToUrl("https://cs.money/3.0/load_user_inventory/730?limit=60&noCache=true&offset=" + offset + "&order=desc&sort=price");
                    IWebElement html = WebDriverWait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//pre")));
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
        public JArray getItems(JArray inventory)
        {
            JArray items = new();
            foreach (JObject item in inventory)
            {
                Thread.Sleep(15000);
                if (item.ContainsKey("stackSize"))
                {
                    Thread.Sleep(1500);
                    Browser.Navigate().GoToUrl("https://cs.money/2.0/get_user_stack/730/" + item["stackId"].ToString());
                    IWebElement html = WebDriverWait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//pre")));
                    string json = html.Text;
                    JArray stack = JArray.Parse(json);

                    foreach (JObject stack_item in stack)
                        items.Add(getStackItems(item, stack_item));
                }
                else
                    items.Add(item);
            }
            return items;
        }
        public void withdrawItems(JObject item)
        {
            JObject json = new(
                       new JProperty("skins",
                           new JObject(
                               new JProperty("bot", new JArray(item)),
                               new JProperty("user", new JArray()))));
            string body = json.ToString(Formatting.None);
            Browser.ExecuteJavaScript(Post.FetchRequest("application/json", body, "https://cs.money/2.0/withdraw_skins"));
        }
    }
}
