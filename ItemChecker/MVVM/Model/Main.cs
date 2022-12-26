using ItemChecker.Support;
using MaterialDesignThemes.Wpf;
using System;
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
        public static void CheckBalance()
        {
            ServiceAccount.GetBalances();
            var steamUsd = Edit.ConverterToUsd(SteamAccount.Balance, SteamAccount.Currency.Value);
            History.HistoryRecords.Add(new()
            {
                Total = steamUsd + ServiceAccount.Csm.Balance + ServiceAccount.Lfm.Balance + ServiceAccount.Buff.Balance,
                Steam = steamUsd,
                CsMoney = ServiceAccount.Csm.Balance,
                LootFarm = ServiceAccount.Lfm.Balance,
                Buff163 = ServiceAccount.Buff.Balance,
            });
        }
    }
    public class MainInfo
    {
        public decimal Balance { get; set; } = SteamAccount.Balance;
        public string CurrencySymbol { get; set; } = SteamAccount.Currency.Symbol;
        public decimal BalanceUsd { get; set; } = Edit.ConverterToUsd(SteamAccount.Balance, SteamAccount.Currency.Value);

        public SnackbarMessageQueue Message { get; set; } = Main.Message;
        public bool IsNotification { get; set; } = Main.Notifications.Any(x => !x.IsRead);
        public ObservableCollection<DataNotification> Notifications { get; set; } = new(Main.Notifications.OrderByDescending(x => x.Date));
    }
    public class DataNotification
    {
        public bool IsRead { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
    }
}