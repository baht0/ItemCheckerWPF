using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ItemChecker.MVVM.Model;
using ItemChecker.MVVM.ViewModel;

namespace ItemChecker.MVVM.View
{
    public partial class DetailsWindow : Window
    {
        bool isSelectedItem;
        public DetailsWindow(string itemName)
        {
            InitializeComponent();
            DataContext = new DetailsViewModel();
            if (string.IsNullOrEmpty(itemName) && DataContext is DetailsViewModel vm && vm.ShowSearchCommand.CanExecute(null))
                vm.ShowSearchCommand.Execute(null);
        }
        private void Window_DragMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void Button_Close(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void currency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isSelectedItem && DataContext is DetailsViewModel vm && vm.SwitchCurrencyCommand.CanExecute(currency.SelectedItem))
                vm.SwitchCurrencyCommand.Execute(currency.SelectedItem);
        }
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = pricesGrid.CurrentItem;
            if (!pricesGrid.Items.IsEmpty && item != null)
            {
                if (DataContext is DetailsViewModel vm && vm.OpenItemOutCommand.CanExecute(item))
                    vm.OpenItemOutCommand.Execute(item);
            }
        }
        private void Compare_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is DetailsViewModel vm && vm.CompareCommand.CanExecute(null))
                vm.CompareCommand.Execute(null);
        }

        private void searchBtn_Click(object sender, RoutedEventArgs e)
        {
            string itemName = searchTxt.Text;
            if (SteamBase.ItemList.Any(x => x.ItemName == itemName))
            {
                searchTxt.Text = string.Empty;
                if (DataContext is DetailsViewModel vm && vm.SearchCommand.CanExecute(itemName))
                    vm.SearchCommand.Execute(itemName);
            }
            else
            {
                MessageBoxResult result = MessageBox.Show(
                    "Please check the correctness of the ItemName.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                searchTxt.Focus();
            }
        }
        private void searchTxt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                searchBtn_Click(null, new());
        }

        private void itemsCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            isSelectedItem = true;
            if (DataContext is DetailsViewModel vm && vm.UpdateItemsViewCommand.CanExecute(null))
            {
                vm.UpdateItemsViewCommand.Execute(null);
                isSelectedItem = false;
            }
        }
    }
}