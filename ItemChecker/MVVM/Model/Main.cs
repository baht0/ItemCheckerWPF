using ItemChecker.Properties;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class Main
    {
        public static SnackbarMessageQueue Message { get; set; } = new();
        public static ObservableCollection<DataNotification> Notifications { get; set; } = new();
    }
    public class MainInfo
    {
        public string User { get; set; } = SteamAccount.UserName;
        public decimal Currency { get; set; } = SettingsProperties.Default.RUB;
        public string CurrencyString { get; set; } = SettingsProperties.Default.CurrencyId == 0 ? "USD ($)" : "RUB (₽)";
        public decimal Balance { get; set; } = SteamAccount.Balance;
        public decimal BalanceCsm { get; set; } = CsmAccount.Balance;
        public decimal BalanceUsd { get; set; } = SteamAccount.BalanceUsd;
        public decimal BalanceCsmUsd { get; set; } = CsmAccount.BalanceUsd;
        public string StatusCommunity { get; set; } = BaseModel.StatusCommunity == "normal" ? "CheckCircle" : "CloseCircle";

        public SnackbarMessageQueue Message { get; set; } = Main.Message;
        public bool IsNotification { get; set; } = Main.Notifications.Any(x => !x.IsRead);
        public ObservableCollection<DataNotification> Notifications { get; set; } = new ObservableCollection<DataNotification>(Main.Notifications.OrderByDescending(x => x.Date));
    }
}