using ItemChecker.Core;
using ItemChecker.Net;
using ItemChecker.Properties;
using System;
using System.Linq;
using System.Threading;
using System.Timers;

namespace ItemChecker.MVVM.Model
{
    public class ToolPush : ObservableObject
    {
        public int ServiceId
        {
            get
            {
                return _serviceId;
            }
            set
            {
                _serviceId = value;
                HomeProperties.Default.ServiceId = value;
                HomeProperties.Default.Save();
                OnPropertyChanged();
            }
        }
        int _serviceId = HomeProperties.Default.ServiceId;
        public int MinPrecent
        {
            get
            {
                return _minPrecent;
            }
            set
            {
                _minPrecent = value;
                HomeProperties.Default.MinPrecent = value;
                HomeProperties.Default.Save();
                OnPropertyChanged();
            }
        }
        int _minPrecent = HomeProperties.Default.MinPrecent;
        public int TimeMin
        {
            get
            {
                return _timeMin;
            }
            set
            {
                _timeMin = value;
                HomeProperties.Default.Time = value;
                HomeProperties.Default.Save();
            }
        }
        int _timeMin = HomeProperties.Default.Time;

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
        int _check;
        public int Count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = value;
                OnPropertyChanged();
            }
        }
        int _count;
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }
        int _progress;
        public int MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged();
            }
        }
        int _maxProgress;
        public string StatusStr
        {
            get { return _statusStr; }
            set
            {
                _statusStr = value;
                OnPropertyChanged();
            }
        }
        string _statusStr = string.Empty;

        public void Start(ToolPush config)
        {
            if (!IsService)
            {
                IsService = true;
                StatusStr = "Starting...";
                TimeSec = config.TimeMin * 60;
                Timer.Elapsed += TimerTick;
                Timer.Enabled = true;
            }
            else
            {
                CTSource.Cancel();
                Timer.Elapsed -= TimerTick;
                Timer.Enabled = false;
                StatusStr = string.Empty;
                IsService = false;
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
            StatusStr = timeSpan.ToString("mm':'ss");
            if (TimeSec <= 0)
            {
                StatusStr = "Preparation...";
                Timer.Enabled = false;
                Progress = 0;

                CTSource = new();
                CToken = CTSource.Token;
                MainTask();
            }
            if (!SteamAccount.Orders.Any())
            {
                CTSource.Cancel();
                StatusStr = string.Empty;
                IsService = false;
                Timer.Enabled = false;
                Timer.Elapsed -= TimerTick;
            }
        }
        void MainTask()
        {
            try
            {
                SteamAccount.GetBalance();
                OrderCheckService.SteamOrders(false);

                StatusStr = "Pushing...";
                MaxProgress = SteamAccount.Orders.Count;
                foreach (DataOrder order in SteamAccount.Orders)
                {
                    try
                    {
                        Count += Push(order) ? 1 : 0;
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
                StatusStr = "Update...";
                if (Check % 5 == 4 || SteamAccount.Orders.GetAvailableAmount() >= SteamAccount.MaxOrderAmount * 0.5m)
                {
                    OrderCheckService.PlaceOrderFromReserve();
                    OrderCheckService.SteamOrders(true);
                }
                DataGridOrders.CanBeUpdated = true;
                Check++;
            }
            catch (Exception exp)
            {
                CTSource.Cancel();
                StatusStr = string.Empty;
                IsService = false;
                Timer.Enabled = false;
                Timer.Elapsed -= TimerTick;

                BaseModel.ErrorLog(exp, true);
            }
            finally
            {
                if (!CToken.IsCancellationRequested)
                {
                    TimeSec = HomeProperties.Default.Time * 60;
                    Timer.Enabled = true;
                }
            }
        }
        bool Push(DataOrder order)
        {
            ItemBaseService.UpdateSteamItem(order.ItemName, SteamAccount.Currency.Id);
            var item = ItemsBase.List.FirstOrDefault(x => x.ItemName == order.ItemName).Steam;

            if (item.HighestBuyOrder > order.OrderPrice && SteamAccount.Balance >= item.HighestBuyOrder && (item.HighestBuyOrder - order.OrderPrice) <= SteamAccount.Orders.GetAvailableAmount())
            {
                SteamRequest.Post.CancelBuyOrder(order.ItemName, order.Id);
                Thread.Sleep(1500);
                SteamRequest.Post.CreateBuyOrder(order.ItemName, item.HighestBuyOrder, SteamAccount.Currency.Id);
                Thread.Sleep(1500);
                return true;
            }
            return false;
        }
    }
}
