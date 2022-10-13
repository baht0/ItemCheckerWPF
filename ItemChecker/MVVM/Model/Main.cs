using ItemChecker.Support;
using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class Main
    {
        public static List<string> ServicesShort
        {
            get
            {
                return new List<string>()
                    {
                        "SteamMarket",
                        "Cs.Money",
                        "Loot.Farm",
                        "Buff163"
                    };
            }
        }
        public static List<string> Services
        {
            get
            {
                return new List<string>()
                    {
                        "SteamMarket(A)",
                        "SteamMarket",
                        "Cs.Money",
                        "Loot.Farm",
                        "Buff163(A)",
                        "Buff163"
                    };
            }
        }
        public static SnackbarMessageQueue Message { get; set; } = new();
        public static List<DataNotification> Notifications { get; set; } = new();
    }
    public class MainInfo
    {
        public decimal Balance { get; set; } = SteamAccount.Balance;
        public string CurrencySymbol { get; set; } = SteamBase.AllowCurrencys.FirstOrDefault(x => x.Id == SteamAccount.CurrencyId).Symbol;
        public decimal BalanceUsd { get; set; } = Edit.ConverterToUsd(SteamAccount.Balance, SteamBase.AllowCurrencys.FirstOrDefault(x => x.Id == SteamAccount.CurrencyId).Value);

        public SnackbarMessageQueue Message { get; set; } = Main.Message;
        public bool IsNotification { get; set; } = Main.Notifications.Any(x => !x.IsRead);
        public ObservableCollection<DataNotification> Notifications { get; set; } = new(Main.Notifications.OrderByDescending(x => x.Date));
    }
}