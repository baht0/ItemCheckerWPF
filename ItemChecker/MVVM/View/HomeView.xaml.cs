using ItemChecker.MVVM.Model;
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
        void InputInt(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out int result);
        }
        void InputDecimal(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !decimal.TryParse(e.Text, out decimal result);
        }

        void DataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (!ordersGrid.Items.IsEmpty)
            {
                HomeViewModel vm = (HomeViewModel)DataContext;
                if (e.Key == Key.Back && vm.CancelOrderCommand.CanExecute(vm.DataGridOrders.SelectedItem))
                    vm.CancelOrderCommand.Execute(vm.DataGridOrders.SelectedItem);
                if (e.Key == Key.F1)
                    MainWindow.OpenDetailsItem(vm.DataGridOrders.SelectedItem.ItemName);
            }
        }
        void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = ordersGrid.CurrentItem;
            if (!ordersGrid.Items.IsEmpty && item != null)
            {
                int columnIndex = ordersGrid.CurrentColumn.DisplayIndex;
                if (DataContext is HomeViewModel viewModel && viewModel.OpenItemOutCommand.CanExecute(columnIndex))
                    viewModel.OpenItemOutCommand.Execute(columnIndex);
            }
        }

        #region push
        private void serviceCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            minPrecentTxt.IsEnabled = serviceCmb.SelectedIndex != 0;
        }
        void ListShow_Click(object sender, RoutedEventArgs e)
        {
            SavedItems.ShowListName = "Reserve";
            if (!MainWindow.IsWindowOpen<Window>("showListWindow"))
            {
                ShowListWindow window = new();
                window.Show();
            }
            else
            {
                Window wnd = Application.Current.Windows.OfType<Window>().Where(w => w.Name.Equals("showListWindow")).FirstOrDefault();
                wnd.Activate();
            }
        }
        void TimerPush_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is HomeViewModel vm && vm.ResetTimerCommand.CanExecute(null))
                vm.ResetTimerCommand.Execute(null);
        }
        #endregion

        #region inventory
        void inventoryListBox_KeyDown(object sender, KeyEventArgs e)
        {
            HomeViewModel vm = (HomeViewModel)DataContext;
            if (e.Key == Key.F1)
                MainWindow.OpenDetailsItem(vm.InventoryTool.SelectedItem.ItemName);
        }
        void inventoryListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!inventoryListBox.Items.IsEmpty)
            {
                object item = inventoryListBox.SelectedItem;
                if (item != null && DataContext is HomeViewModel viewModel && viewModel.ShowInventoryItemCommand.CanExecute(item))
                    viewModel.ShowInventoryItemCommand.Execute(item);
            }
        }
        void inventComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            inventoryGrid.Height = invenTasks.SelectedIndex == 1 && inventoryGrid.Visibility == Visibility.Visible ? 400 : 510;
            sellGroup.Visibility = invenTasks.SelectedIndex == 1 && inventoryGrid.Visibility == Visibility.Visible ? Visibility.Visible : Visibility.Hidden;
        }

        void priceCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            sellPrice.IsEnabled = priceCombox.SelectedIndex == 2 && (bool)selectedOnly.IsChecked;
        }
        void selectedOnly_Checked(object sender, RoutedEventArgs e)
        {
            sellPrice.IsEnabled = (bool)selectedOnly.IsChecked && priceCombox.SelectedIndex == 2;
        }
        void allAvailable_Checked(object sender, RoutedEventArgs e)
        {
            sellPrice.IsEnabled = (bool)selectedOnly.IsChecked && priceCombox.SelectedIndex == 2;
        }
        #endregion

    }
}