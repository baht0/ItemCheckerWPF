using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.MVVM.View;
using ItemChecker.Support;

namespace ItemChecker.MVVM.ViewModel
{
    public class HomeViewModel : ObservableObject
    {
        readonly Timer TimerView = new(500);
        public BaseModel Home
        {
            get
            {
                return _home;
            }
            set
            {
                _home = value;
                OnPropertyChanged();
            }
        }
        BaseModel _home = new();
        public HomeViewModel()
        {
            Task.Run(() => { 
                OrderCheckService.SteamOrders(true);
                DataGridOrders.Items = new(SteamAccount.Orders);
                DataGridOrders.IsBusy = false;
                OnPropertyChanged(nameof(DataGridOrders));
            });
            TimerView.Elapsed += UpdateView;
            TimerView.Enabled = true;
        }
        void UpdateView(Object sender, ElapsedEventArgs e)
        {
            try
            {
                if (DataGridOrders.CanBeUpdated)
                {
                    DataGridOrders.Items = new(SteamAccount.Orders);
                    DataGridOrders.CanBeUpdated = false;
                }
            }
            catch (Exception ex)
            {
                BaseModel.ErrorLog(ex, false);
            }
        }

        //Table
        public DataGridOrders DataGridOrders
        {
            get
            {
                return _dataGridOrders;
            }
            set
            {
                _dataGridOrders = value;
                OnPropertyChanged();
            }
        }
        DataGridOrders _dataGridOrders = new();
        public ICommand OpenItemOutCommand =>
            new RelayCommand((obj) =>
            {
                DataGridOrders.ShowItemInService((int)obj);

            }, (obj) => DataGridOrders.Items.Any());
        public ICommand OrdersCommand =>
            new RelayCommand((obj) =>
            {
                switch (Convert.ToInt32(obj))
                {
                    case 0:
                        Task.Run(DataGridOrders.UpdateTable);
                        break;
                    case 1:
                        Task.Run(DataGridOrders.CancelOrders);
                        break;
                }
            }, (obj) => !DataGridOrders.IsBusy);
        public ICommand CancelOrderCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    DataGridOrders.CancelOrder(obj as DataOrder);
                });
            }, (obj) => DataGridOrders.Items.Any() && DataGridOrders.SelectedItem != null);

        //Push
        public ToolPush PushTool
        {
            get
            {
                return _pushTool;
            }
            set
            {
                _pushTool = value;
                OnPropertyChanged();
            }
        }
        ToolPush _pushTool = new();
        public ICommand PushCommand =>
            new RelayCommand((obj) =>
            {
                MainWindow.CloseShowListWin("Reserve");
                PushTool.Start(obj as ToolPush);

            }, (obj) => SteamAccount.Orders.Any() && PushTool.TimeMin > 0);
        public ICommand ResetTimerCommand =>
            new RelayCommand((obj) =>
            {
                PushTool.ResetTime();

            }, (obj) => PushTool.IsService);

        //Inventory
        public ToolInventory InventoryTool
        {
            get
            {
                return _inventoryTool;
            }
            set
            {
                _inventoryTool = value;
                OnPropertyChanged();
            }
        }
        ToolInventory _inventoryTool = new();
        public ICommand UpdateInventoryCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(InventoryTool.UpdateInventory);

            }, (obj) => !InventoryTool.IsBusy);
        public ICommand ShowInventoryItemCommand =>
            new RelayCommand((obj) =>
            {
                var item = (DataInventory)obj;

                Edit.OpenUrl("https://steamcommunity.com/my/inventory/#730_2_" + item.Data.FirstOrDefault().AssetId);

            }, (obj) => InventoryTool.Items.Any() && InventoryTool.SelectedItem != null);
        public ICommand InventoryTaskCommand =>
            new RelayCommand((obj) =>
            {
                InventoryTool.StartTask();

            }, (obj) => InventoryTool.TaskId == 0
                            || (InventoryTool.TaskId == 1 && InventoryTool.Items.Any()
                                && ((InventoryTool.AllAvailable && InventoryTool.SellingPriceId != 2)
                                    || (InventoryTool.SelectedOnly && InventoryTool.SelectedItem != null
                                        && (InventoryTool.SellingPriceId < 2 || (InventoryTool.SellingPriceId == 2 && InventoryTool.Price != 0))))));
    }
}