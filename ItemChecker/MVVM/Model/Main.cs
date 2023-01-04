using ItemChecker.Core;
using ItemChecker.Services;
using ItemChecker.Support;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;

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
        public static Notification<DataNotification> Notifications { get; set; } = new()
        {
            new DataNotification()
            {
                IsRead = true,
                Title = "Welcome!",
                Message = "The program has been launched!"
            }
        };
        internal static decimal AllIn { get; set; }

        public static void CreateHistoryRecords()
        {
            SteamAccount.GetBalance();
            ServiceAccount.GetBalances();

            var steam = Currency.ConverterToUsd(SteamAccount.Balance, SteamAccount.Currency.Id) + InventoryService.GetSumOfItems();
            var csm = ServiceAccount.Csm.Balance + ServiceAccount.Csm.GetSumOfItems();
            var lfm = ServiceAccount.Lfm.Balance + ServiceAccount.Lfm.GetSumOfItems();

            decimal total = steam + csm + lfm + ServiceAccount.Buff.Balance;
            History.Records.Add(new()
            {
                Total = total,
                Steam = steam,
                CsMoney = csm,
                LootFarm = lfm,
                Buff163 = ServiceAccount.Buff.Balance,
            });
            AllIn = total;
        }
    }
    public class MainInfo : ObservableObject
    {
        public decimal AllIn
        {
            get
            {
                return _allIn;
            }
            set
            {
                _allIn = value;
                OnPropertyChanged();
            }
        }
        public decimal _allIn = Main.AllIn;
        public SnackbarMessageQueue Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }
        public SnackbarMessageQueue _message = Main.Message;
        public bool IsNotification
        {
            get
            {
                return _isNotification;
            }
            set
            {
                _isNotification = value;
                OnPropertyChanged();
            }
        }
        public bool _isNotification = Main.Notifications.Any(x => !x.IsRead);
        public ObservableCollection<DataNotification> Notifications
        {
            get
            {
                return _notifications;
            }
            set
            {
                _notifications = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<DataNotification> _notifications = new(Main.Notifications.OrderByDescending(x => x.Date));
    }
    public class DataNotification
    {
        public bool IsRead { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
    }
    public class Notification<T> : List<T>
    {
        public new void Add(T notification)
        {
            base.Add(notification);
            var data = notification as DataNotification;
            if (data.Title != "Welcome!")
                PlayNotificationSound();
        }
        void PlayNotificationSound()
        {
            bool found = false;
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"AppEvents\Schemes\Apps\.Default\Notification.Reminder\.Current"))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue(null); // pass null to get (Default)
                        if (o != null)
                        {
                            SoundPlayer theSound = new SoundPlayer((String)o);
                            theSound.Play();
                            found = true;
                        }
                    }
                }
            }
            catch
            { }
            if (!found)
                SystemSounds.Beep.Play(); // consolation prize
        }
    }
}