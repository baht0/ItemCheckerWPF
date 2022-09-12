using ItemChecker.MVVM.ViewModel;
using System.Linq;
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
                if (e.Key == Key.Back && viewModel.CancelOrderCommand.CanExecute(viewModel.HomeTable.SelectedOrderItem))
                    viewModel.CancelOrderCommand.Execute(viewModel.HomeTable.SelectedOrderItem);
                if (e.Key == Key.F1)
                {
                    DetailsWindow detailsWindow = new(viewModel.HomeTable.SelectedOrderItem.ItemName);
                    detailsWindow.Show();
                }
            }
        }
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = ordersGrid.CurrentItem;
            if (!ordersGrid.Items.IsEmpty && item != null)
            {
                int columnIndex = ordersGrid.CurrentColumn.DisplayIndex;
                if (DataContext is HomeViewModel viewModel && viewModel.OpenItemOutCommand.CanExecute(columnIndex))
                    viewModel.OpenItemOutCommand.Execute(columnIndex);
            }
        }

        private void TimerPush_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is HomeViewModel vm && vm.TimerCommand.CanExecute(null))
                vm.TimerCommand.Execute(0);
        }
        private void ListShow_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!MainWindow.IsWindowOpen<Window>("showListWindow"))
            {
                ShowListWindow window = new("Favorite");
                window.Show();
            }
            else
            {
                Window wnd = Application.Current.Windows.OfType<Window>().Where(w => w.Name.Equals("showListWindow")).FirstOrDefault();
                wnd.Activate();
            }
        }

        private void inventoryListBox_KeyDown(object sender, KeyEventArgs e)
        {
            var viewModel = (HomeViewModel)DataContext;
            if (e.Key == Key.F1)
            {
                DetailsWindow detailsWindow = new(viewModel.SelectedInventory.ItemName);
                detailsWindow.Show();
            }
        }
        private void inventoryListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!inventoryListBox.Items.IsEmpty)
            {
                object item = inventoryListBox.SelectedItem;
                if (item != null && DataContext is HomeViewModel viewModel && viewModel.ShowInventoryItemCommand.CanExecute(item))
                    viewModel.ShowInventoryItemCommand.Execute(item);
            }
        }
        private void inventComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            sellGroup.IsEnabled = invenTasks.SelectedIndex == 1;
        }
    }
}