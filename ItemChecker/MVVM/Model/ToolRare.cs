using HtmlAgilityPack;
using ItemChecker.Core;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;

namespace ItemChecker.MVVM.Model
{
    public class RareConfig : ObservableObject
    {
        public int Time { get; set; } = RareProperties.Default.Time;
        public int MinPrecent { get; set; } = 7;
        public List<string> ComparePrices => new()
                {
                    "Lowest ST", "Median ST"
                };
        public int CompareId { get; set; }
        public int ParameterId { get; set; }

        //float
        public decimal FactoryNew { get; set; } = RareProperties.Default.maxFloatValue_FN;
        public decimal MinimalWear { get; set; } = RareProperties.Default.maxFloatValue_MW;
        public decimal FieldTested { get; set; } = RareProperties.Default.maxFloatValue_FT;
        public decimal WellWorn { get; set; } = RareProperties.Default.maxFloatValue_WW;
        public decimal BattleScarred { get; set; } = RareProperties.Default.maxFloatValue_BS;
        //stickers
        public List<int> StickerCount => new()
                {
                    1, 2, 3, 4, 5
                };
        public int MinSticker { get; set; } = 1;
        public string NameContains { get; set; } = string.Empty;
        public bool Normal { get; set; }
        public bool Holo { get; set; }
        public bool Glitter { get; set; }
        public bool Foil { get; set; }
        public bool Gold { get; set; }
        public bool Lenticular { get; set; }
        public bool Contraband { get; set; }
        //dopplers
        public bool AllDopplers { get; set; }
        public bool Phase1 { get; set; }
        public bool Phase2 { get; set; }
        public bool Phase3 { get; set; }
        public bool Phase4 { get; set; }
        public bool Ruby { get; set; }
        public bool Sapphire { get; set; }
        public bool BlackPearl { get; set; }
        public bool Emerald { get; set; }

        protected RareConfig Config => (RareConfig)this.MemberwiseClone();
    }
    public class ToolRare : RareConfig
    {
        public int TimeMin
        {
            get
            {
                return _timeMin;
            }
            set
            {
                _timeMin = value;
                RareProperties.Default.Time = value;
                RareProperties.Default.Save();
            }
        }
        int _timeMin = RareProperties.Default.Time;

        public static CancellationTokenSource CTSource { get; set; } = new();
        public CancellationToken CToken { get; set; } = CTSource.Token;
        public System.Timers.Timer Timer { get; set; } = new(1000);
        int TimeSec { get; set; }

        public bool IsService
        {
            get
            {
                return _isService;
            }
            set
            {
                _isService = value;
                OnPropertyChanged();
            }
        }
        bool _isService;
        public int Check
        {
            get
            {
                return _check;
            }
            set
            {
                _check = value;
                OnPropertyChanged();
            }
        }
        int _check = 0;
        public int PurchasesMade
        {
            get
            {
                return _purchasesMade;
            }
            set
            {
                _purchasesMade = value;
                OnPropertyChanged();
            }
        }
        int _purchasesMade = 0;
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }
        int _progress = 0;
        public int MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged();
            }
        }
        int _maxProgress = 0;
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        string _status = string.Empty;

        public static List<DataRare> Items { get; set; } = new();
        public void Start()
        {
            if (!IsService)
            {
                Status = "Starting...";
                IsService = true;

                RareProperties.Default.Time = Time;
                RareProperties.Default.maxFloatValue_FN = FactoryNew;
                RareProperties.Default.maxFloatValue_MW = MinimalWear;
                RareProperties.Default.maxFloatValue_FT = FieldTested;
                RareProperties.Default.maxFloatValue_WW = WellWorn;
                RareProperties.Default.maxFloatValue_BS = BattleScarred;
                RareProperties.Default.Save();

                Timer.Elapsed += TimerTick;
                TimeSec = Time * 60;
                Timer.Enabled = true;
            }
            else
            {
                CTSource.Cancel();
                Status = string.Empty;
                IsService = false;
                Timer.Enabled = false;
                TimeSec = 0;
                Timer.Elapsed -= TimerTick;
            }
        }
        public void ResetTime()
        {
            TimeSec = 1;
        }

        void TimerTick(Object sender, ElapsedEventArgs e)
        {
            TimeSec--;
            TimeSpan timeSpan = TimeSpan.FromSeconds(TimeSec);
            Status = timeSpan.ToString("mm':'ss");
            if (TimeSec <= 0)
            {
                Status = "Preparation...";
                Timer.Enabled = false;
                Progress = 0;

                CTSource = new();
                CToken = CTSource.Token;
                CheckList();
            }
            if (!SavedItems.Rare.Any(x => x.ServiceId == ParameterId) && ParameterId != 2 && !AllDopplers)
            {
                CTSource.Cancel();
                Status = string.Empty;
                IsService = false;
                Timer.Enabled = false;
                TimeSec = 0;
                Timer.Elapsed -= TimerTick;
            }
        }
        void CheckList()
        {
            try
            {
                var serviceList = SavedItems.Rare.Where(x => x.ServiceId == ParameterId).ToList();
                serviceList = ParameterId == 2 && AllDopplers ? GetAllDoppler() : serviceList;
                int start = Items.Count;

                MaxProgress = serviceList.Count;
                Status = "Checking...";
                var service = new RareCheckService(Config);
                foreach (var list in serviceList)
                {
                    try
                    {
                        var checkedList = service.Check(list.ItemName);
                        foreach (var item in checkedList)
                            if (!Items.Any(x => x.FloatValue == item.FloatValue))
                                Items.Add(item);
                    }
                    catch (Exception exp)
                    {
                        BaseModel.ErrorLog(exp, false);
                    }
                    finally
                    {
                        Progress++;
                    }
                    if (CToken.IsCancellationRequested)
                        break;
                }
                Check++;

                DataGridRare.CanBeUpdated = true;
                if (Items.Count - start != 0)
                    Main.Notifications.Add(new()
                    {
                        Title = "Rare",
                        Message = $"Found {Items.Count - start} items."
                    });
            }
            catch (Exception exp)
            {
                CTSource.Cancel();
                Status = string.Empty;
                IsService = false;
                Timer.Enabled = false;
                TimeSec = 0;
                Timer.Elapsed -= TimerTick;

                BaseModel.ErrorLog(exp, true);
            }
            finally
            {
                if (!CToken.IsCancellationRequested)
                {
                    TimeSec = RareProperties.Default.Time * 60;
                    Timer.Enabled = true;
                }
            }
        }
        List<DataItem> GetAllDoppler()
        {
            var list = new List<DataItem>();
            foreach (var doppler in ItemsBase.List.Where(x => x.ItemName.Contains("Doppler")).ToList())
                list.Add(new DataItem(doppler.ItemName, 2));
            return list;
        }
    }
    public class RareCheckService
    {
        string ItemName;
        RareConfig Config { get; set; } = new();

        public RareCheckService(RareConfig config)
        {
            Config = config;
            Config.MinPrecent *= -1;
        }
        public List<DataRare> Check(string itemName)
        {
            ItemName = itemName;
            List<DataRare> items = new();
            decimal priceCompare = PriceCompare();

            var json = SteamRequest.Get.ItemListings(itemName);
            var attributes = json["listinginfo"].ToList<JToken>();
            foreach (JToken attribute in attributes)
            {
                try
                {
                    DataRare data = new()
                    {
                        ParameterId = Config.ParameterId,
                        CompareId = Config.CompareId,
                        ItemName = itemName,
                        PriceCompare = priceCompare,
                    };

                    var jProperty = attribute.ToObject<JProperty>();
                    data.DataBuy.ListingId = jProperty.Name;

                    data = GetPrices(data, json);
                    if (data.Precent < Config.MinPrecent)
                        break;

                    data.Stickers = GetStickers(data, json);
                    data = InspectLink(data, json);

                    switch (Config.ParameterId)
                    {
                        case 0://float
                            if (data.FloatValue < MaxFloat())
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
            }
            return items;
        }

        decimal PriceCompare()
        {
            JObject steamPrices = SteamRequest.Get.PriceOverview(ItemName, 1);

            decimal lowest_price = steamPrices.ContainsKey("lowest_price") ? Edit.GetDecimal(steamPrices["lowest_price"].ToString()) : 0m;
            decimal median_price = steamPrices.ContainsKey("median_price") ? Edit.GetDecimal(steamPrices["median_price"].ToString()) : 0m;

            switch (Config.CompareId)
            {
                case 0:
                    return lowest_price;
                case 1:
                    return median_price;
                default:
                    return 0;
            }
        }
        DataRare GetPrices(DataRare data, JObject json)
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
        List<string> GetStickers(DataRare data, JObject json)
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
        DataRare InspectLink(DataRare data, JObject json)
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
        decimal MaxFloat()
        {
            decimal maxFloat = 0;
            if (ItemName.Contains("Factory New")) maxFloat = RareProperties.Default.maxFloatValue_FN;
            else if (ItemName.Contains("Minimal Wear")) maxFloat = RareProperties.Default.maxFloatValue_MW;
            else if (ItemName.Contains("Field-Tested")) maxFloat = RareProperties.Default.maxFloatValue_FT;
            else if (ItemName.Contains("Well-Worn")) maxFloat = RareProperties.Default.maxFloatValue_WW;
            else if (ItemName.Contains("Battle-Scarred")) maxFloat = RareProperties.Default.maxFloatValue_BS;

            return maxFloat;
        }

        //sticker
        bool AllowStickers(DataRare data)
        {
            if (!String.IsNullOrEmpty(Config.NameContains) && !data.Stickers.Any(x => x.Contains(Config.NameContains)))
                return false;
            if (data.Stickers.Count >= Config.MinSticker)
            {
                foreach (var sticker in data.Stickers)
                {
                    var baseItem = ItemsBase.List.FirstOrDefault(x => x.ItemName == sticker);
                    if (baseItem == null)
                        return false;
                    switch (baseItem.Quality)
                    {
                        case Quality.MilSpec:
                            if (Config.Normal)
                                return Config.Normal;
                            break;
                        case Quality.Restricted:
                            if ((Config.Holo && data.ItemName.Contains("Holo")) || (Config.Glitter && data.ItemName.Contains("Glitter")))
                                return true;
                            break;
                        case Quality.Classified:
                            if (Config.Foil)
                                return true;
                            break;
                        case Quality.Covert:
                            if (Config.Gold)
                                return true;
                            break;
                        case Quality.Contraband:
                            if (Config.Contraband)
                                return true;
                            break;
                        default:
                            return true;
                    }
                    if (!Config.Normal &&
                        !Config.Holo &&
                        !Config.Glitter &&
                        !Config.Foil &&
                        !Config.Gold &&
                        !Config.Contraband)
                        return true;
                }
            }
            return false;
        }

        //doppler
        Doppler CheckDopplerPhase(int paintIndex)
        {
            switch (paintIndex)
            {
                case 415:
                    return Doppler.Ruby;
                case 416:
                    return Doppler.Sapphire;
                case 417:
                    return Doppler.BlackPearl;
                case 568 or 1119:
                    return Doppler.Emerald;
                case 418 or 569 or 1120:
                    return Doppler.Phase1;
                case 419 or 570 or 1121:
                    return Doppler.Phase2;
                case 420 or 571 or 1122:
                    return Doppler.Phase3;
                case 421 or 572 or 1123:
                    return Doppler.Phase4;
                default:
                    return Doppler.None;
            }
        }
        bool AllowDopplerPhase(DataRare data)
        {
            switch (data.Phase)
            {
                case Doppler.Ruby:
                    return Config.Ruby;
                case Doppler.Sapphire:
                    return Config.Sapphire;
                case Doppler.BlackPearl:
                    return Config.BlackPearl;
                case Doppler.Emerald:
                    return Config.Emerald;
                case Doppler.Phase1:
                    return Config.Phase1;
                case Doppler.Phase2:
                    return Config.Phase2;
                case Doppler.Phase3:
                    return Config.Phase3;
                case Doppler.Phase4:
                    return Config.Phase4;
                default:
                    return false;
            }
        }
    }
}
