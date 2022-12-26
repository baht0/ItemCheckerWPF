using HtmlAgilityPack;
using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemChecker.Services
{
    public class RareCheckService : BaseService
    {
        public List<DataRare> Check(string itemName)
        {
            List<DataRare> items = new();
            itemName = Edit.RemoveDoppler(itemName);
            decimal priceCompare = SetPrice(itemName);

            var json = SteamRequest.Get.ItemListings(itemName);
            var attributes = json["listinginfo"].ToList<JToken>();
            foreach (JToken attribute in attributes)
            {
                try
                {
                    DataRare data = new()
                    {
                        ParameterId = RareCheckConfig.CheckedConfig.ParameterId,
                        CompareId = RareCheckConfig.CheckedConfig.CompareId,
                        ItemName = itemName,
                        PriceCompare = priceCompare,
                    };

                    var jProperty = attribute.ToObject<JProperty>();
                    data.DataBuy.ListingId = jProperty.Name;

                    data.DataBuy.Subtotal = Convert.ToDecimal(json["listinginfo"][data.DataBuy.ListingId]["converted_price"]);
                    decimal fee_steam = Convert.ToDecimal(json["listinginfo"][data.DataBuy.ListingId]["converted_steam_fee"]);
                    decimal fee_csgo = Convert.ToDecimal(json["listinginfo"][data.DataBuy.ListingId]["converted_publisher_fee"]);
                    data.DataBuy.Fee = fee_steam + fee_csgo;
                    data.DataBuy.Total = data.DataBuy.Subtotal + data.DataBuy.Fee;
                    data.Price = data.DataBuy.Total / 100;

                    data.Precent = Edit.Precent(data.Price, data.PriceCompare);
                    if (data.Precent < RareCheckConfig.CheckedConfig.MinPrecent)
                        break;

                    data.Difference = Edit.Difference(data.Price, data.PriceCompare);
                    string ass_id = json["listinginfo"][data.DataBuy.ListingId]["asset"]["id"].ToString();

                    data.Stickers = GetStickers(json, ass_id);

                    string link = json["listinginfo"][data.DataBuy.ListingId]["asset"]["market_actions"][0]["link"].ToString();
                    link = link.Replace("%listingid%", data.DataBuy.ListingId);
                    link = link.Replace("%assetid%", ass_id);
                    data.Link = link;

                    decimal maxFloat = SetMaxFloat(itemName);
                    data.FloatValue = SetFloatValue(link);

                    switch (RareCheckConfig.CheckedConfig.ParameterId)
                    {
                        case 0://float
                            if (data.FloatValue < maxFloat)
                                items.Add(data);
                            break;
                        case 1://sticker
                            if (CheckStickers(data))
                                items.Add(data);
                            break;
                        case 2://doppler

                            break;
                    }
                }
                catch
                {
                    continue;
                }
                if (RareCheckStatus.token.IsCancellationRequested)
                    break;
            }
            return items;
        }

        Decimal SetPrice(string itemName)
        {
            JObject steamPrices = SteamRequest.Get.PriceOverview(itemName, 1);

            decimal lowest_price = steamPrices.ContainsKey("lowest_price") ? Edit.GetPrice(steamPrices["lowest_price"].ToString()) : 0m;
            decimal median_price = steamPrices.ContainsKey("median_price") ? Edit.GetPrice(steamPrices["median_price"].ToString()) : 0m;

            switch (RareProperties.Default.CompareId)
            {
                case 0:
                    return lowest_price;
                case 1:
                    return median_price;
                default:
                    return 0;
            }
        }
        Decimal SetMaxFloat(string itemName)
        {
            decimal maxFloat = 0;
            if (itemName.Contains("Factory New")) maxFloat = RareProperties.Default.maxFloatValue_FN;
            else if (itemName.Contains("Minimal Wear")) maxFloat = RareProperties.Default.maxFloatValue_MW;
            else if (itemName.Contains("Field-Tested")) maxFloat = RareProperties.Default.maxFloatValue_FT;
            else if (itemName.Contains("Well-Worn")) maxFloat = RareProperties.Default.maxFloatValue_WW;
            else if (itemName.Contains("Battle-Scarred")) maxFloat = RareProperties.Default.maxFloatValue_BS;

            return maxFloat;
        }
        Decimal SetFloatValue(string link)
        {
            try
            {
                string json = HttpRequest.RequestGetAsync(@"https://api.csgofloat.com/?url=" + link).Result;

                return Convert.ToDecimal(JObject.Parse(json)["iteminfo"]["floatvalue"].ToString());
            }
            catch (Exception exp)
            {
                errorLog(exp, false);
                return 1;
            }
        }
        List<string> GetStickers(JObject obj, string ass_id)
        {
            List<string> stickers = new();
            JArray descriptions = JArray.Parse(obj["assets"]["730"]["2"][ass_id]["descriptions"].ToString());
            string value = descriptions.LastOrDefault()["value"].ToString().Trim();
            if (!String.IsNullOrEmpty(value))
            {
                HtmlDocument htmlDoc = new();
                htmlDoc.LoadHtml(value);                
                string[] str = htmlDoc.DocumentNode.SelectSingleNode("//div").InnerText.Trim().Split(',');
                foreach (string sticker in str)
                {
                    string name = sticker.Replace("Sticker:", string.Empty);
                    stickers.Add($"Sticker |{name}");
                }
            }
            return stickers;
        }

        Boolean CheckStickers(DataRare data)
        {
            if (!String.IsNullOrEmpty(RareCheckConfig.CheckedConfig.NameContains) && !data.Stickers.Any(x => x.Contains(RareCheckConfig.CheckedConfig.NameContains)))
                return false;
            if (data.Stickers.Count >= RareCheckConfig.CheckedConfig.MinSticker)
            {
                foreach (var sticker in data.Stickers)
                {
                    var baseItem = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == sticker);
                    if (baseItem == null)
                        return false;
                    switch (baseItem.Quality)
                    {
                        case "Mil-Spec":
                            if (RareCheckConfig.CheckedConfig.Normal)
                                return true;
                            break;
                        case "Restricted":
                            if ((RareCheckConfig.CheckedConfig.Holo && data.ItemName.Contains("Holo")) || (RareCheckConfig.CheckedConfig.Glitter && data.ItemName.Contains("Glitter")))
                                return true;
                            break;
                        case "Classified":
                            if (RareCheckConfig.CheckedConfig.Foil)
                                return true;
                            break;
                        case "Covert":
                            if (RareCheckConfig.CheckedConfig.Gold)
                                return true;
                            break;
                        case "Contraband":
                            if (RareCheckConfig.CheckedConfig.Contraband)
                                return true;
                            break;
                        default:
                            return true;
                    }
                    if (!RareCheckConfig.CheckedConfig.Normal &&
                        !RareCheckConfig.CheckedConfig.Holo &&
                        !RareCheckConfig.CheckedConfig.Glitter &&
                        !RareCheckConfig.CheckedConfig.Foil &&
                        !RareCheckConfig.CheckedConfig.Gold &&
                        !RareCheckConfig.CheckedConfig.Contraband)
                        return true;
                }
            }
            return false;
        }
    }
}
