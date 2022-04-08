﻿using ItemChecker.Net;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using System.Web;

namespace UpdateBase
{
    class Program
    {
        static List<SteamBase> SteamBase { get; set; } = new();
        static List<SteamBase> NewItemsBase { get; set; } = new();
        static void Main(string[] args)
        {
            try
            {
                JArray skinsBase = JArray.Parse(Post.DropboxRead("steamBase.json"));
                foreach (JObject item in skinsBase)
                {
                    SteamBase.Add(new SteamBase()
                    {
                        ItemName = item["itemName"].ToString(),
                        SteamId = Convert.ToInt32(item["steamId"]),
                        Type = item["type"].ToString(),
                        Quality = item["quality"].ToString(),
                    });
                }

                List<string> weapons = new()
                {
                    "CZ75-Auto", "Desert Eagle", "Glock-18", "USP-S", "P250", "Five-SeveN", "P2000", "Tec-9", "R8 Revolver", "Dual Berettas",
                    "P90", "UMP-45", "MAC-10", "MP7", "MP9", "MP5-SD", "PP-Bizon",
                    "Sawed-Off", "MAG-7", "Nova", "XM1014", "Negev", "M249",
                    "AK-47", "AWP", "M4A4", "M4A1-S", "AUG", "SG 553", "Galil AR", "FAMAS", "SSG 08", "SCAR-20", "G3SG1"
                };
                List<string> knifes = new()
                {
                    "Nomad Knife", "Skeleton Knife", "Survival Knife", "Paracord Knife", "Classic Knife", "Bayonet",
                    "Bowie Knife", "Butterfly Knife", "Falchion Knife", "Flip Knife", "Gut Knife", "Huntsman Knife",
                    "Karambit", "M9 Bayonet", "Navaja Knife", "Shadow Daggers", "Stiletto Knife", "Talon Knife", "Ursus Knife"
                };

                Console.WriteLine($"Program started: {DateTime.Now}");
                Console.WriteLine($"Check Command:\n0. Skins (weapon, knife, gloves);\n1. Weapons;\n2. Knifes;\n3. Gloves;\n4. Stickers;\n5. Containers;\n6. Agents;\n7. Patch;\n8. Music Kits;\n9. Collectable Pins;\n10. Graffitis;\n11. SET SteamId.");

                int com = -1;
                while (com < 0)
                {
                    Console.WriteLine("Number: ");
                    com = Convert.ToInt32(Console.ReadLine());
                    switch (com)
                    {
                        case 0:
                            Check(weapons, "Weapon");
                            Check(knifes, "Knife");
                            Check(new List<string>() { "gloves" }, "Gloves");
                            break;
                        case 1:
                            Check(weapons, "Weapon");
                            break;
                        case 2:
                            Check(knifes, "Knife");
                            break;
                        case 3:
                            Check(new List<string>() { "gloves" }, "Gloves");
                            break;
                        case 4:
                            Console.WriteLine($"Stickers:\n1. Regular;\n2. Tournament;");
                            Console.WriteLine("Number: ");
                            switch (Convert.ToInt32(Console.ReadLine()))
                            {
                                case 1:
                                    Check(new List<string>() { "sticker" }, "Sticker", "regular");
                                    break;
                                case 2:
                                    Check(new List<string>() { "sticker" }, "Sticker", "tournament");
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 5:
                            Console.WriteLine($"Containers:\n1. Skin Cases;\n2. Souvenir Packages;\n3. Sticker Capsules;\n4. Autograph Capsules;\n5. Gift Packages;");
                            Console.WriteLine("Number: ");
                            switch (Convert.ToInt32(Console.ReadLine()))
                            {
                                case 1:
                                    Check(new List<string>() { "container" }, "Skin Case");
                                    break;
                                case 2:
                                    Check(new List<string>() { "container" }, "Souvenir Package");
                                    break;
                                case 3:
                                    Check(new List<string>() { "container" }, "Sticker Capsule");
                                    break;
                                case 4:
                                    Check(new List<string>() { "container" }, "Autograph Capsule");
                                    break;
                                case 5:
                                    Check(new List<string>() { "container" }, "Gift Package");
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 6:
                            Check(new List<string>() { "agents" }, "Agent");
                            break;
                        case 7:
                            Check(new List<string>() { "patchs" }, "Patch");
                            break;
                        case 8:
                            Check(new List<string>() { "music kit" }, "Music Kit");
                            break;
                        case 9:
                            Check(new List<string>() { "collectable" }, "Collectable");
                            break;
                        case 10:
                            Check(new List<string>() { "graffiti" }, "Graffiti");
                            break;
                        case 11:
                            SetSteamId();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\r\n======================");
                Console.WriteLine(ex);
            }
            finally
            {
                Console.WriteLine($"\r\n======================\r\n[{NewItemsBase.Count}] Program ended: {DateTime.Now}");
                JArray jArray = new();
                foreach (SteamBase item in NewItemsBase)
                {
                    jArray.Add(new JObject(
                        new JProperty("itemName", item.ItemName),
                        new JProperty("steamId", item.SteamId),
                        new JProperty("type", item.Type),
                        new JProperty("quality", item.Quality)));
                }
                File.WriteAllText($"_newItemsBase.txt", jArray.ToString());
                Console.ReadKey();
            }
        }
        static void Check(List<string> items, string type, string sub = "")
        {
            int pages = 1;
            Console.WriteLine($"'{type}' start. {DateTime.Now}");
            for (int i = 1; i <= pages; i++)
            {
                foreach (string item in items)
                {
                    int start = NewItemsBase.Count;

                    string url = string.Empty;
                    switch (type)
                    {
                        case "Weapon" or "Knife":
                            url = "https://csgostash.com/weapon/" + item.Replace(" ", "+") + "?name=&rarity_contraband=1&rarity_ancient=1&rarity_legendary=1&rarity_mythical=1&rarity_rare=1&rarity_uncommon=1&rarity_common=1&has_st=1&no_st=1&has_souv=1&no_souv=1&sort=name&order=desc&page=" + i;
                            break;
                        case "Gloves":
                            url = "https://csgostash.com/gloves?name=&gloves_hydra=1&gloves_bloodhound=1&gloves_driver=1&gloves_handwraps=1&gloves_moto=1&gloves_specialist=1&gloves_sport=1&sort=name&order=desc&page=" + i;
                            break;
                        case "Sticker":
                            url = "https://csgostash.com/stickers/" + sub + "?name=&sticker_type=any&rarity_contraband=1&rarity_covert=1&rarity_legendary=1&rarity_mythical=1&rarity_rare=1&container=any&sort_agg=avg&sort=name&order=desc&page=" + i;
                            break;
                        case "Skin Case" or "Souvenir Package" or "Sticker Capsule" or "Autograph Capsule" or "Gift Package":
                            string typeUrl = type.Replace(" ", "-").ToLower();
                            url = "https://csgostash.com/containers/" + typeUrl + "s?name=&sort=name&order=desc&page=" + i;
                            break;
                        case "Agent":
                            url = "https://csgostash.com/agents?name=&team_t=1&team_ct=1&rarity_ancient=1&rarity_legendary=1&rarity_mythical=1&rarity_rare=1&sort=name&order=desc&page=" + i;
                            break;
                        case "Patch":
                            url = "https://csgostash.com/patches?name=&rarity_legendary=1&rarity_mythical=1&rarity_rare=1&sort=name&order=desc&page=" + i;
                            break;
                        case "Music Kit":
                            url = "https://csgostash.com/music?name=&container=any&sort=name&order=desc&page=" + i;
                            break;
                        case "Collectable":
                            url = "https://csgostash.com/pins?name=&rarity_ancient=1&rarity_legendary=1&rarity_mythical=1&rarity_rare=1&sort=name&order=desc&page=" + i;
                            break;
                        case "Graffiti":
                            url = "https://csgostash.com/graffiti?name=&rarity_legendary=1&rarity_mythical=1&rarity_rare=1&rarity_common=1&graffiti_type=any&container=any&sort=name&order=desc&page=" + i;
                            break;
                    }
                    
                    string html = Get.Request(url);
                    HtmlDocument htmlDoc = new();
                    htmlDoc.LoadHtml(html);
                    HtmlNodeCollection skins = htmlDoc.DocumentNode.SelectNodes("//div[@class='container main-content']/div[@class='row'][4]/div[@class='col-lg-4 col-md-6 col-widen text-center']");
                    
                    pages = CountPages(htmlDoc);                    
                    foreach (HtmlNode skin in skins)
                    {
                        switch (type)
                        {
                            case "Weapon" or "Knife" or "Gloves" or "Sticker" or "Patch" or "Collectable" or "Graffiti":
                                html = Get.Request(skin.SelectSingleNode(".//div[2]/p/a").Attributes["href"].Value);
                                htmlDoc = new();
                                htmlDoc.LoadHtml(html);
                                break;
                        }
                        switch (type)
                        {
                            case "Weapon":
                                CheckWeapon(htmlDoc, skin, item);
                                break;
                            case "Knife":
                                CheckKnife(htmlDoc, skin, item);
                                break;
                            case "Gloves":
                                CheckGloves(htmlDoc, skin);
                                break;
                            case "Sticker" or "Patch" or "Collectable":
                                CheckStickerPatchCollectable(htmlDoc, skin, type);
                                break;
                            case "Skin Case" or "Souvenir Package" or "Sticker Capsule" or "Autograph Capsule" or "Gift Package":
                                CheckContainer(skin, type);
                                break;
                            case "Music Kit":
                                CheckMusic(skin);
                                break;
                            case "Agent":
                                CheckAgent(skin);
                                break;
                            case "Graffiti":
                                CheckGraffiti(htmlDoc, skin);
                                break;
                        }
                    }
                    Console.WriteLine($"'{item}' {i}/{pages} done. Added [{NewItemsBase.Count - start}]...");
                }
            }
            Console.WriteLine($"'{type}' [{NewItemsBase.Count}] done...");
        }

        static void CheckWeapon(HtmlDocument htmlDoc, HtmlNode skin, string item)
        {
            HtmlNodeCollection exteriors = htmlDoc.DocumentNode.SelectNodes("//table[@class='table table-hover table-bordered table-condensed price-details-table']/tbody/tr");

            foreach (HtmlNode exterior in exteriors)
            {
                string exter = exterior.SelectSingleNode(".//td").InnerText.Trim();
                string option = SetOption(exter);
                exter = exter.Replace("StatTrak ", string.Empty);
                exter = exter.Replace("Souvenir ", string.Empty);
                string skinName = skin.SelectSingleNode(".//div/h3/a").InnerText.Trim();

                string itemName = option + $"{item} | {skinName} ({exter})";
                string quality = SetQuality(skin.SelectSingleNode(".//a/div/p").InnerText.Trim());
                AddBase(itemName, quality, "Weapon");
            }
        }
        static void CheckKnife(HtmlDocument htmlDoc, HtmlNode skin, string item)
        {
            HtmlNodeCollection exteriors = htmlDoc.DocumentNode.SelectNodes("//table[@class='table table-hover table-bordered table-condensed price-details-table']/tbody/tr");

            foreach (HtmlNode exterior in exteriors)
            {
                string exter = exterior.SelectSingleNode(".//td").InnerText.Trim();
                string option = "★ " + SetOption(exter);
                exter = exter.Replace("StatTrak ", string.Empty);
                string skinName = skin.SelectSingleNode(".//div/h3/a").InnerText.Trim();

                string itemName = option + $"{item} | {skinName} ({exter})";
                if (skinName == "★ (Vanilla)")
                    itemName = option + item;
                string quality = SetQuality(skin.SelectSingleNode(".//a/div/p").InnerText.Trim());
                AddBase(itemName, quality, "Knife");
            }
        }
        static void CheckGloves(HtmlDocument htmlDoc, HtmlNode skin)
        {
            HtmlNodeCollection exteriors = htmlDoc.DocumentNode.SelectNodes("//table[@class='table table-hover table-bordered table-condensed price-details-table']/tbody/tr");

            foreach (HtmlNode exterior in exteriors)
            {
                string exter = exterior.SelectSingleNode(".//td").InnerText.Trim();
                string option = "★ " + SetOption(exter);
                string skinName = skin.SelectSingleNode(".//div/h3/a").InnerText.Trim();

                string itemName = option + skinName + $" ({exter})";
                string quality = SetQuality(skin.SelectSingleNode(".//div/div/p").InnerText.Trim());
                AddBase(itemName, quality, "Gloves");
            }
        }
        static void CheckStickerPatchCollectable(HtmlDocument htmlDoc, HtmlNode skin, string type)
        {
            string itemName = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='well result-box nomargin']/div/div/h2").InnerText.Trim();
            string quality = SetQuality(skin.SelectSingleNode(".//div/p").InnerText.Trim());
            AddBase(itemName, quality, type);
        }
        static void CheckContainer(HtmlNode skin, string type)
        {
            string itemName = skin.SelectSingleNode(".//div/a/h4").InnerText.Trim();

            AddBase(itemName, null, type);
        }
        static void CheckAgent(HtmlNode skin)
        {
            string itemName = skin.SelectSingleNode(".//div/h3/a").InnerText.Trim();
            string quality = SetQuality(skin.SelectSingleNode(".//div/p").InnerText.Trim());

            AddBase(itemName, quality, "Agent");
        }
        static void CheckMusic(HtmlNode skin)
        {
            string title = skin.SelectSingleNode(".//div/h3/a").InnerText.Trim();
            string artist = skin.SelectSingleNode(".//div/h4").InnerText.Trim().Replace("By ", string.Empty);
            string strk = skin.SelectSingleNode(".//div/div[2]").InnerText.Trim();
            string itemName = !title.Contains(',') ? $"Music Kit | {artist}, {title}" : $"Music Kit | {artist}: {title}";

            string quality = SetQuality(skin.SelectSingleNode(".//div/div").InnerText.Trim());
            switch (strk)
            {
                case "StatTrak Available":
                    AddBase(itemName, quality, "Music Kit");
                    AddBase("StatTrak™ " + itemName, quality, "Music Kit");
                    break;
                case "StatTrak Only":
                    AddBase("StatTrak™ " + itemName, quality, "Music Kit");
                    break;
                default:
                    AddBase(itemName, quality, "Music Kit");
                    break;
            }
        }
        static void CheckGraffiti(HtmlDocument htmlDoc, HtmlNode skin)
        {
            string skinName = skin.SelectSingleNode(".//div/h3/a").InnerText.Trim();
            string quality = SetQuality(skin.SelectSingleNode(".//div/p").InnerText.Trim());
            if (quality == "Industrial Grade")
            {
                HtmlNodeCollection colors = htmlDoc.DocumentNode.SelectNodes("//div[@class='container main-content']/div[@class='row text-center'][3]/div[@class='col-lg-3 col-md-4 col-sm-6 col-widen']");
                foreach (HtmlNode color in colors)
                {
                    string nameColor = color.SelectSingleNode(".//div/h4").InnerText;
                    AddBase($"Sealed Graffiti | {skinName} ({nameColor})", quality, "Graffiti");
                }
            }
            else
                AddBase($"Sealed Graffiti | {skinName}", quality, "Graffiti");
        }
        static void SetSteamId()
        {
            int i = 1;
            int start = SteamBase.Where(x => x.SteamId == 0).Count();
            Console.WriteLine($"Found: {start}, {DateTime.Now}");
            foreach (var item in SteamBase)
            {
                if (item.SteamId == 0)
                {
                    string html = string.Empty;
                    do
                    {
                        Thread.Sleep(100);
                        html = SteamRequest(item.ItemName);
                    } while (String.IsNullOrEmpty(html));

                    item.SteamId = ItemNameId(html);
                    Console.WriteLine($"{i}) {item.ItemName} / {item.SteamId}");
                    i++;
                }
            }
            int end = SteamBase.Where(x => x.SteamId == 0).Count();
            Console.WriteLine($"Successfully: {start - end}/{start}.");

            if (start - end > 0)
            {
                JArray jArray = new();
                foreach (SteamBase item in SteamBase)
                {
                    jArray.Add(new JObject(
                        new JProperty("itemName", item.ItemName),
                        new JProperty("steamId", item.SteamId),
                        new JProperty("type", item.Type),
                        new JProperty("quality", item.Quality)));
                }
                File.WriteAllText($"_steamBase.txt", jArray.ToString());
            }
        }

        static void AddBase(string itemName, string quality, string type)
        {
            itemName = HttpUtility.HtmlDecode(itemName);
            itemName = itemName.Trim();
            itemName = itemName.Replace("\n", " ");
            if (!SteamBase.Any(x => x.ItemName == itemName))
                NewItemsBase.Add(new()
                {
                    ItemName = itemName,
                    Quality = quality,
                    Type = type
                });
        }
        static String SetQuality(string line)
        {
            if (line.Contains("Contraband"))
                return "Contraband";
            if (line.Contains("Covert") || line.Contains("Extraordinary") || line.Contains("Master Agent"))
                return "Covert";
            if (line.Contains("Classified") || line.Contains("Exotic") || line.Contains("Superior Agent"))
                return "Classified";
            if (line.Contains("Restricted") || line.Contains("Remarkable") || line.Contains("Exceptional Agent"))
                return "Restricted";
            if (line.Contains("Mil-Spec") || line.Contains("High Grade") || line.Contains("Distinguished Agent"))
                return "Mil-Spec";
            if (line.Contains("Industrial Grade") || line.Contains("Base Grade Graffiti"))
                return "Industrial Grade";
            if (line.Contains("Consumer Grade"))
                return "Consumer Grade";

            return null;
        }
        static String SetOption(string line)
        {
            if (line.Contains("StatTrak"))
                return "StatTrak™ ";
            if (line.Contains("Souvenir"))
                return "Souvenir ";

            return string.Empty;
        }
        static Int32 CountPages(HtmlDocument htmlDoc)
        {
            try
            {
                var page = htmlDoc.DocumentNode.SelectNodes("//div[@class='container main-content']/div[@class='row'][2]/div/ul/li");
                page.Remove(page.LastOrDefault());

                return Int32.Parse(page.LastOrDefault().InnerText);
            }
            catch
            {
                return 1;
            }
        }
        static String SteamRequest(string itemName)
        {
            try
            {
                return Get.Request("https://steamcommunity.com/market/listings/730/" + HttpUtility.UrlPathEncode(itemName));
            }
            catch
            {
                Thread.Sleep(TimeSpan.FromMinutes(1));
                return string.Empty;
            }
        }
        static Int32 ItemNameId(string html)
        {
            try
            {
                html = html.Substring(html.IndexOf("Market_LoadOrderSpread"));
                var a = html.IndexOf("(");
                var b = html.IndexOf(")");
                string str = html.Substring(a, b);

                int id = Convert.ToInt32(System.Text.RegularExpressions.Regex.Replace(str, @"[^\d]+", ""));

                return id;
            }
            catch
            {
                return 0;
            }
        }
    }
}