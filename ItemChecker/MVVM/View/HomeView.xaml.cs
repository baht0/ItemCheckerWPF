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
                HomeViewModel vm = (HomeViewModel)DataContext;
                if (e.Key == Key.Back && vm.CancelOrderCommand.CanExecute(vm.HomeTable.SelectedOrderItem))
                    vm.CancelOrderCommand.Execute(vm.HomeTable.SelectedOrderItem);
                if (e.Key == Key.F1)
                    MainWindow.OpenDetailsItem(vm.HomeTable.SelectedOrderItem.ItemName);
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
            if (DataContext is HomeViewModel vm && vm.ResetTimerCommand.CanExecute(null))
                vm.ResetTimerCommand.Execute(0);
        }
        private void ListShow_Click(object sender, RoutedEventArgs e)
        {
            if (!MainWindow.IsWindowOpen<Window>("showListWindow"))
            {
                ShowListWindow window = new("Reserve");
                window.Show();
            }
            else
            {
                Window wnd = Application.Current.Windows.OfType<Window>().Where(w => w.Name.Equals("showListWindow")).FirstOrDefault();
                wnd.Activate();
            }
        }

        #region inventory
        private void inventoryListBox_KeyDown(object sender, KeyEventArgs e)
        {
            HomeViewModel vm = (HomeViewModel)DataContext;
            if (e.Key == Key.F1)
                MainWindow.OpenDetailsItem(vm.SelectedInventory.ItemName);
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
            inventoryGrid.Height = invenTasks.SelectedIndex == 1 && inventoryGrid.Visibility == Visibility.Visible ? 400 : 510;
            sellGroup.Visibility = invenTasks.SelectedIndex == 1 && inventoryGrid.Visibility == Visibility.Visible ? Visibility.Visible : Visibility.Hidden;
        }

        private void priceCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            sellPrice.IsEnabled = priceCombox.SelectedIndex == 2 && (bool)selectedOnly.IsChecked;
        }
        private void selectedOnly_Checked(object sender, RoutedEventArgs e)
        {
            sellPrice.IsEnabled = (bool)selectedOnly.IsChecked && priceCombox.SelectedIndex == 2;
        }
        private void allAvailable_Checked(object sender, RoutedEventArgs e)
        {
            sellPrice.IsEnabled = (bool)selectedOnly.IsChecked && priceCombox.SelectedIndex == 2;
        }
        #endregion
    }
}