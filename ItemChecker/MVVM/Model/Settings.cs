using ItemChecker.Properties;
using System;
using System.Collections.ObjectModel;

namespace ItemChecker.MVVM.Model
{
    public class Settings
    {
        //general
        public string CurrencyApi { get; set; } = SettingsProperties.Default.CurrencyApiKey;
        public ObservableCollection<string> CurrencyList { get; set; } = new()
        {
            "USD ($)",
            "RUB (₽)"
        };
        public int CurrencyId { get; set; } = SettingsProperties.Default.CurrencyId;
        public bool Quit { get; set; } = SettingsProperties.Default.Quit;
        public bool SetHours { get; set; } = SettingsProperties.Default.SetHours;
        public DateTime TurnOn { get; set; } = SettingsProperties.Default.TurnOn;
        public DateTime TurnOff { get; set; } = SettingsProperties.Default.TurnOff;

        //order
        public ObservableCollection<string> ServicesList { get; set; } = new()
        {
            "SteamMarket(A)",
            "SteamMarket",
            "Cs.Money",
            "Loot.Farm"
        };
        public int ServiceId { get; set; } = SettingsProperties.Default.ServiceId;
        public int MinPrecent { get; set; } = SettingsProperties.Default.MinPrecent;

        public string SteamApiKey { get; set; } = SteamAccount.ApiKey;
        public string AccountName { get; set; } = SteamAccount.AccountName;
        public string SteamId { get; set; } = SteamAccount.Id64;
        public string SteamMarket { get; set; } = SteamAccount.SteamMarket;
        //float
        public decimal FactoryNew { get; set; } = FloatProperties.Default.maxFloatValue_FN;
        public decimal MinimalWear { get; set; } = FloatProperties.Default.maxFloatValue_MW;
        public decimal FieldTested { get; set; } = FloatProperties.Default.maxFloatValue_FT;
        public decimal WellWorn { get; set; } = FloatProperties.Default.maxFloatValue_WW;
        public decimal BattleScarred { get; set; } = FloatProperties.Default.maxFloatValue_BS;
    }
}
