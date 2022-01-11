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
        public int Currency { get; set; } = SettingsProperties.Default.Currency;
        public bool Quit { get; set; } = SettingsProperties.Default.Quit;
        public bool SetHours { get; set; } = SettingsProperties.Default.SetHours;
        public DateTime TurnOn { get; set; } = SettingsProperties.Default.TurnOn;
        public DateTime TurnOff { get; set; } = SettingsProperties.Default.TurnOff;

        //steam
        public string SteamApiKey { get; set; } = SteamAccount.ApiKey;
        public bool NotEnoughBalance { get; set; } = SettingsProperties.Default.NotEnoughBalance;
        public int CancelOrder { get; set; } = SettingsProperties.Default.CancelOrder;

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
