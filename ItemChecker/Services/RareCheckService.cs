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
        public static List<DataRare> Check(string itemName)
        {
            List<DataRare> items = new();
            decimal priceCompare = PriceCompare(itemName);

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

                    data = GetPrices(data, json);
                    if (data.Precent < RareCheckConfig.CheckedConfig.MinPrecent)
                        break;

                    data.Stickers = GetStickers(data, json);
                    data = InspectLink(data, json);

                    switch (RareCheckConfig.CheckedConfig.ParameterId)
                    {
                        case 0://float
                            if (data.FloatValue < MaxFloat(itemName))
                                items.Add(data);
                            break;
                        case 1://sticker
                            if (AllowStickers(data))
                                items.Add(data);
                            break;
                        case 2://doppler
                            if (AllowDopplerPhase(data))
                                items.Add(data);
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

        static decimal PriceCompare(string itemName)
        {
            JObject steamPrices = SteamRequest.Get.PriceOverview(itemName, 1);

            decimal lowest_price = steamPrices.ContainsKey("lowest_price") ? Edit.GetDecimal(steamPrices["lowest_price"].ToString()) : 0m;
            decimal median_price = steamPrices.ContainsKey("median_price") ? Edit.GetDecimal(steamPrices["median_price"].ToString()) : 0m;

            switch (RareCheckConfig.CheckedConfig.CompareId)
            {
                case 0:
                    return lowest_price;
                case 1:
                    return median_price;
                default:
                    return 0;
            }
        }
        static DataRare GetPrices(DataRare data, JObject json)
        {
            data.DataBuy.Subtotal = Convert.ToDecimal(json["listinginfo"][data.DataBuy.ListingId]["converted_price"]);
            decimal fee_steam = Convert.ToDecimal(json["listinginfo"][data.DataBuy.ListingId]["converted_steam_fee"]);
            decimal fee_csgo = Convert.ToDecimal(json["listinginfo"][data.DataBuy.ListingId]["converted_publisher_fee"]);
            data.DataBuy.Fee = fee_steam + fee_csgo;
            data.DataBuy.Total = data.DataBuy.Subtotal + data.DataBuy.Fee;
            data.Price = data.DataBuy.Total / 100;

            data.Precent = Edit.Precent(data.Price, data.PriceCompare);

            data.Difference = Edit.Difference(data.Price, data.PriceCompare);

            return data;
        }
        static List<string> GetStickers(DataRare data, JObject json)
        {
            string ass_id = json["listinginfo"][data.DataBuy.ListingId]["asset"]["id"].ToString();
            var stickers = new List<string>();
            var descriptions = JArray.Parse(json["assets"]["730"]["2"][ass_id]["descriptions"].ToString());
            var value = descriptions.LastOrDefault()["value"].ToString().Trim();
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
        static DataRare InspectLink(DataRare data, JObject json)
        {
            string ass_id = json["listinginfo"][data.DataBuy.ListingId]["asset"]["id"].ToString();
            string link = json["listinginfo"][data.DataBuy.ListingId]["asset"]["market_actions"][0]["link"].ToString();
            link = link.Replace("%listingid%", data.DataBuy.ListingId);
            link = link.Replace("%assetid%", ass_id);
            data.Link = link;

            json = ServicesRequest.InspectLinkDetails(data.Link);
            data.FloatValue = Convert.ToDecimal(json["floatvalue"].ToString());
            int paintIndex = Convert.ToInt32(json["paintindex"].ToString());
            data.Phase = CheckDopplerPhase(paintIndex);

            return data;
        }

        //float
        static decimal MaxFloat(string itemName)
        {
            decimal maxFloat = 0;
            if (itemName.Contains("Factory New")) maxFloat = RareProperties.Default.maxFloatValue_FN;
            else if (itemName.Contains("Minimal Wear")) maxFloat = RareProperties.Default.maxFloatValue_MW;
            else if (itemName.Contains("Field-Tested")) maxFloat = RareProperties.Default.maxFloatValue_FT;
            else if (itemName.Contains("Well-Worn")) maxFloat = RareProperties.Default.maxFloatValue_WW;
            else if (itemName.Contains("Battle-Scarred")) maxFloat = RareProperties.Default.maxFloatValue_BS;

            return maxFloat;
        }

        //sticker
        static bool AllowStickers(DataRare data)
        {
            if (!String.IsNullOrEmpty(RareCheckConfig.CheckedConfig.NameContains) && !data.Stickers.Any(x => x.Contains(RareCheckConfig.CheckedConfig.NameContains)))
                return false;
            if (data.Stickers.Count >= RareCheckConfig.CheckedConfig.MinSticker)
            {
                foreach (var sticker in data.Stickers)
                {
                    var baseItem = ItemsBase.List.FirstOrDefault(x => x.ItemName == sticker);
                    if (baseItem == null)
                        return false;
                    switch (baseItem.Quality)
                    {
                        case "Mil-Spec":
                            if (RareCheckConfig.CheckedConfig.Normal)
                                return RareCheckConfig.CheckedConfig.Normal;
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

        //doppler
        static string CheckDopplerPhase(int paintIndex)
        {
            switch (paintIndex)
            {
                case 415:
                    return "Ruby";
                case 416:
                    return "Sapphire";
                case 417:
                    return "Black Pearl";
                case 568 or 1119:
                    return "Emerald";
                case 418 or 569 or 1120:
                    return "Phase 1";
                case 419 or 570 or 1121:
                    return "Phase 2";
                case 420 or 571 or 1122:
                    return "Phase 3";
                case 421 or 572 or 1123:
                    return "Phase 4";
                default:
                    return "-";
            }
        }
        static bool AllowDopplerPhase(DataRare data)
        {
            switch (data.Phase)
            {
                case "Ruby":
                    return RareCheckConfig.CheckedConfig.Ruby;
                case "Sapphire":
                    return RareCheckConfig.CheckedConfig.Sapphire;
                case "Black pearl":
                    return RareCheckConfig.CheckedConfig.BlackPearl;
                case "Emerald":
                    return RareCheckConfig.CheckedConfig.Emerald;
                case "Phase 1":
                    return RareCheckConfig.CheckedConfig.Phase1;
                case "Phase 2":
                    return RareCheckConfig.CheckedConfig.Phase2;
                case "Phase 3":
                    return RareCheckConfig.CheckedConfig.Phase3;
                case "Phase 4":
                    return RareCheckConfig.CheckedConfig.Phase4;
                default:
                    return false;
            }
        }
    }
}
