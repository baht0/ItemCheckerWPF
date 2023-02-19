using ItemChecker.Properties;
using ItemChecker.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ItemChecker.MVVM.Model
{
    public class DataGridOrders : BaseTable<DataOrder>
    {
        public static bool CanBeUpdated { get; set; }
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }
        bool _isBusy = true;

        public void ShowItemInService(int columnId)
        {
            var item = SelectedItem;
            string itemName = item.ItemName.Replace("(Holo/Foil)", "(Holo-Foil)");
            string market_hash_name = Uri.EscapeDataString(itemName);
            switch (columnId)
            {
                case 1:
                    Edit.OpenUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
                    break;
                case 2 or 3:
                    switch (HomeProperties.Default.ServiceId)
                    {
                        case 0 or 1:
                            Edit.OpenUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
                            break;
                        case 2:
                            Edit.OpenCsm(itemName);
                            break;
                        case 3:
                            Edit.OpenUrl("https://loot.farm/");
                            break;
                        case 4:
                            var id = ItemsBase.List.FirstOrDefault(x => x.ItemName == item.ItemName).Buff.Id;
                            if (id != 0)
                                Edit.OpenUrl("https://buff.163.com/goods/" + id + "#tab=buying");
                            else
                                Edit.OpenUrl("https://buff.163.com/market/csgo#tab=buying&page_num=1&search=" + market_hash_name);
                            break;
                        case 5:
                            id = ItemsBase.List.FirstOrDefault(x => x.ItemName == item.ItemName).Buff.Id;
                            if (id != 0)
                                Edit.OpenUrl("https://buff.163.com/goods/" + id);
                            else
                                Edit.OpenUrl("https://buff.163.com/market/csgo#tab=selling&page_num=1&search=" + market_hash_name);
                            break;
                    }
                    break;
                default:
                    Clipboard.SetText(itemName);
                    break;
            }
        }
        public void CancelOrder(DataOrder item)
        {
            try
            {
                IsBusy = true;
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to cancel order?\n{item.ItemName}",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    SteamAccount.Orders.Cancel(item);
                    Items = new(SteamAccount.Orders);
                    Main.Message.Enqueue($"{item.ItemName}\nOrder has been canceled.");
                }
            }
            catch (Exception ex)
            {
                BaseModel.ErrorLog(ex, true);
            }
            finally
            {
                IsBusy = false;
            }
        }
        public void UpdateTable()
        {
            try
            {
                IsBusy = true;
                OrderCheckService.SteamOrders(true);
                Items = new(SteamAccount.Orders);
                Main.Message.Enqueue("MyOrders update is complete.");
            }
            catch (Exception ex)
            {
                BaseModel.ErrorLog(ex, true);
            }
            finally
            {
                IsBusy = false;
            }
        }
        public void CancelOrders()
        {
            try
            {
                IsBusy = true;
                List<DataOrder> orders = new(SteamAccount.Orders);
                foreach (DataOrder order in orders)
                    SteamAccount.Orders.Cancel(order);
                Items = new(SteamAccount.Orders);
                Main.Message.Enqueue("All MyOrders have been cancelled.");
            }
            catch (Exception ex)
            {
                BaseModel.ErrorLog(ex, true);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
