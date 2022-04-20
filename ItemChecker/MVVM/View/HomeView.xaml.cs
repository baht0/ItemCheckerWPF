using ItemChecker.MVVM.ViewModel;
using ItemChecker.Support;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ItemChecker.MVVM.View
{
    public partial class BuyOrderView : UserControl
    {
        public BuyOrderView()
        {
            InitializeComponent();
        }
        private void Number_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int result;
            e.Handled = !int.TryParse(e.Text, out result);
        }
        private void Decimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            decimal result;
            e.Handled = !decimal.TryParse(e.Text, out result);
        }
        private void DataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (!ordersGrid.Items.IsEmpty)
            {
                HomeViewModel viewModel = (HomeViewModel)DataContext;
                if (e.Key == Key.Back && viewModel.CancelOrderCommand.CanExecute(null))
                    viewModel.CancelOrderCommand.Execute(viewModel.SelectedOrderItem);
            }
        }
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = ordersGrid.CurrentItem;
            if (!ordersGrid.Items.IsEmpty && item != null)
            {
                PropertyInfo info = item.GetType().GetProperty("ItemName");
                string ItemName = (string)info.GetValue(item, null);
                ItemName = ItemName.Replace("(Holo/Foil)", "(Holo-Foil)");
                string market_has_name = Edit.MarketHashName(ItemName);

                int columnIndex = ordersGrid.CurrentColumn.DisplayIndex;
                switch (columnIndex)
                {
                    case 1 or 2:
                        Edit.openUrl("https://steamcommunity.com/market/listings/730/" + market_has_name);
                        break;
                    case 3 or 4:
                        Edit.openCsm(market_has_name);
                        break;
                    default:
                        Clipboard.SetText(ItemName);
                        break;
                }
            }
        }
        private void TimerPush_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            HomeViewModel viewModel = (HomeViewModel)DataContext;
            if (viewModel.TimerCommand.CanExecute(null))
                viewModel.TimerCommand.Execute(0);
        }
        private void TimerCsm_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            HomeViewModel viewModel = (HomeViewModel)DataContext;
            if (viewModel.TimerCommand.CanExecute(null))
                viewModel.TimerCommand.Execute(1);
        }
        private void TimerFloat_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            HomeViewModel viewModel = (HomeViewModel)DataContext;
            if (viewModel.TimerCommand.CanExecute(null))
                viewModel.TimerCommand.Execute(2);
        }
    }
}