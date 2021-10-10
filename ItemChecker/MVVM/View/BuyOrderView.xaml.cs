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
            this.DataContext = new BuyOrderViewModel();
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
                if (e.Key == Key.Back)
                {
                    BuyOrderViewModel viewModel = (BuyOrderViewModel)DataContext;
                    if (viewModel.CancelOrderCommand.CanExecute(null))
                        viewModel.CancelOrderCommand.Execute(viewModel.SelectedOrderItem);
                }
                if (e.Key == Key.F)
                {
                    MainViewModel viewModel = (MainViewModel)DataContext;
                    if (viewModel.AddFavoriteCommand.CanExecute(null))
                        viewModel.AddFavoriteCommand.Execute(((BuyOrderViewModel)DataContext).SelectedOrderItem);
                }
            }
        }
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!ordersGrid.Items.IsEmpty)
            {
                object item = ordersGrid.CurrentItem;
                PropertyInfo info = item.GetType().GetProperty("ItemName");
                string ItemName = (string)info.GetValue(item, null);

                string market_has_name = Edit.MarketHashName(ItemName);

                int columnIndex = ordersGrid.CurrentColumn.DisplayIndex;
                switch (columnIndex)
                {
                    case 2:
                        Edit.openUrl("https://steamcommunity.com/market/listings/730/" + market_has_name);
                        break;
                    case 3:
                        Edit.openUrl("https://steamcommunity.com/market/listings/730/" + market_has_name);
                        break;
                    case 4:
                        Edit.openCsm(market_has_name);
                        break;
                    case 5:
                        Edit.openCsm(market_has_name);
                        break;
                    default:
                        Clipboard.SetText(ItemName);
                        break;
                }
            }
        }

        private void TimerTextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            BuyOrderViewModel viewModel = (BuyOrderViewModel)DataContext;
            if (viewModel.TimerCommand.CanExecute(null))
                viewModel.TimerCommand.Execute(null);
        }
    }
}