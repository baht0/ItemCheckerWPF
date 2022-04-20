using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.MVVM.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Windows;
using ItemChecker.Support;

namespace ItemChecker.Services
{
    public class CsmCheckService : BaseService
    {
        static List<string> old_id = new();
        int successfulTrades { get; set; } = 0;

        public List<DataCsmCheck> CheckSteamPrice()
        {
            ParserCheckService checkService = new();
            List<DataCsmCheck> list = new();
            foreach (string itemName in HomeCsmCheck.List)
            {
                if (HomeCsmCheck.token.IsCancellationRequested)
                    break;
                try
                {
                    DataParser data = checkService.Check(itemName, 2, 0);
                    if (SettingsProperties.Default.CurrencyId == 0)
                        data.Price4 = Edit.ConverterToUsd(data.Price4, SettingsProperties.Default.CurrencyValue);
                    list.Add(new()
                    {
                        ItemName = itemName,
                        StmPrice = data.Price4,
                    });
                }
                catch (Exception exp)
                {
                    HomeCsmCheck.cts.Cancel();
                    if (!exp.Message.Contains("429"))
                    {
                        BaseService.errorLog(exp);
                        BaseService.errorMessage(exp);
                    }
                    else
                        MessageBox.Show(exp.Message, "Parser stoped!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            return list;
        }

        public Int32 Check(string checkItem)
        {
            //string[] item_line = checkItem.Split(';');

            //string json = Get.InventoriesCsMoney(HttpUtility.UrlEncode(item_line[0]));
            //if (!json.Contains("error"))
            //{
            //    JArray items = new();
            //    JArray inventory = JArray.Parse(JObject.Parse(json)["items"].ToString());
            //    foreach (JObject item in inventory)
            //    {
            //        if ((string)item["fullName"] != item_line[0])
            //            continue;
            //        if (checkItem.Contains(';'))
            //        {
            //            decimal maxPrice = decimal.Parse(item_line[1]) + HomeProperties.Default.MinPrecent;
            //            decimal price = Convert.ToDecimal(item["price"]);
            //            if (price > maxPrice)
            //                break;
            //        }
            //        if (item.ContainsKey("stackSize"))
            //        {
            //            var response = Get.Request("https://inventories.cs.money/4.0/get_bot_stack/730/" + item["stackId"].ToString());
            //            JArray stack = JArray.Parse(response);
            //            foreach (JObject stack_item in stack)
            //                items.Add(getStackItems(item, stack_item));
            //        }
            //        else
            //            items.Add(item);
            //    }
            //    if (items.Any())
            //        addCart(items);
            //}

            return successfulTrades;
        }
        Boolean addCart(JArray items)
        {
            ClearCart();
            decimal sum = 0;
            foreach (JObject item in items)
            {
                JObject json = new(
                           new JProperty("type", 2),
                           new JProperty("item", new JObject(item)));
                string body = json.ToString(Formatting.None);
                Browser.ExecuteJavaScript(Post.FetchRequest("application/json", body, "https://cs.money/add_cart"));
                sum += Convert.ToDecimal(item["price"].ToString());
                Thread.Sleep(1500);
            }
            sum *= -1;
            return sendOffer(items, sum);
        }
        Boolean sendOffer(JArray items, decimal sum)
        {
            JObject json = new(
                        new JProperty("skins",
                            new JObject(
                                new JProperty("user", new JArray()),
                                new JProperty("bot", new JArray(items)))),
                        new JProperty("balance", sum),
                            new JProperty("games", new JObject()),
                            new JProperty("isVirtual", false));
            string body = json.ToString(Formatting.None);

            JObject response = sendRespons(body);
            if (response.ContainsKey("error"))
            {
                if (response["error"].ToString() == "11")
                {
                    JArray skins = JArray.Parse(response["details"]["skins"].ToString());
                    if (items.Count > skins.Count)
                    {
                        JArray itemsCopy = items;
                        foreach (JObject skinId in skins)
                        {
                            string id = skinId["skinId"].ToString();
                            foreach (JObject item in items)
                                if (item["id"].ToString() == id)
                                {
                                    Browser.ExecuteJavaScript(Delete.FetchRequest("https://cs.money/remove_cart_item?type=2&id=" + id));
                                    sum -= Convert.ToDecimal(item["price"].ToString());
                                    itemsCopy.Remove(item);
                                    Thread.Sleep(1500);
                                }
                        }
                        sendOffer(itemsCopy, sum);
                        return true;
                    }
                    else return false;
                }
                else return false;
            }
            else return true;
        }

        public void getTransactions()
        {
            Thread.Sleep(1000);

            Browser.Navigate().GoToUrl("https://cs.money/2.0/get_transactions?type=0&status=0&appId=730&limit=20");
            IWebElement html = WebDriverWait.Until(e => e.FindElement(By.XPath("//pre")));

            string json = html.Text;
            JArray trades = JArray.Parse(json);
            if (trades.Any())
                foreach (JObject trade in trades)
                {
                    string id = (string)trade["offers"][0]["id"];
                    if (!old_id.Contains(id))
                        confirmVirtualOffer(id);
                }
        }
        void confirmVirtualOffer(string id)
        {
            JObject json = new(
                        new JProperty("offer_id", id),
                        new JProperty("action", "confirm"));
            string body = json.ToString(Formatting.None);
            Browser.ExecuteJavaScript(Post.FetchRequest("application/json", body, "https://cs.money/confirm_virtual_offer"));
            successfulTrades++;
            Thread.Sleep(1500);
            old_id.Add(id);
        }

        protected JObject getStackItems(JObject item, JObject stack_item)
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
        JObject sendRespons(string body)
        {
            //Browser.ExecuteJavaScript("window.open();");
            //Browser.SwitchTo().Window(Browser.WindowHandles.Last());
            Browser.SwitchTo().NewWindow(WindowType.Tab);
            Browser.ExecuteJavaScript(Post.FetchRequestWithResponse("application/json", body, "https://cs.money/2.0/send_offer"));
            IWebElement html = WebDriverWait.Until(e => e.FindElement(By.XPath("//pre")));
            string json = html.Text;
            Thread.Sleep(300);
            //Close the tab or window
            Browser.Close();
            //Browser.ExecuteJavaScript("window.close();");
            Browser.SwitchTo().Window(Browser.WindowHandles.First());

            return JObject.Parse(json);
        }
        public void ClearCart()
        {
            Browser.ExecuteJavaScript(Post.FetchRequest("application/json", "{\"type\":1}", "https://cs.money/clear_cart"));
            Thread.Sleep(1000);
            Browser.ExecuteJavaScript(Post.FetchRequest("application/json", "{\"type\":2}", "https://cs.money/clear_cart"));
        }
    }
}
