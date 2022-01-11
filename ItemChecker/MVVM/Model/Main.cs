using ItemChecker.Properties;

namespace ItemChecker.MVVM.Model
{
    public class Main
    {
        public string User { get; set; } = SteamAccount.User;
        public decimal Course { get; set; } = SettingsProperties.Default.CurrencyValue;
        public decimal Balance { get; set; } = SteamAccount.Balance;
        public decimal BalanceCsm { get; set; } = CsmAccount.BalanceCsm;
        public decimal BalanceUsd { get; set; } = SteamAccount.BalanceUsd;
        public decimal BalanceCsmUsd { get; set; } = CsmAccount.BalanceCsmUsd;
        public decimal AvailableAmount { get; set; } = SteamAccount.GetAvailableAmount();
        public string CurrencyString { get; set; } = SettingsProperties.Default.Currency == 0 ? "USD ($)" : "RUB (₽)";
        public string StatusCommunity { get; set; } = BaseModel.StatusCommunity != "normal" ? "CheckCircle" : "CloseCircle";
    }
}