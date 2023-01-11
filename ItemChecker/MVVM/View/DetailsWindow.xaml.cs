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
        public DetailsWindow(bool isMenu)
        {
            InitializeComponent();
            DataContext = new DetailsViewModel(isMenu);
        }
        void Window_DragMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        void Button_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        void detailsWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        void currency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isSelectedItem && DataContext is DetailsViewModel vm && vm.SwitchCurrencyCommand.CanExecute(currency.SelectedItem))
                vm.SwitchCurrencyCommand.Execute(currency.SelectedItem);
        }
        void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = pricesGrid.CurrentItem;
            if (!pricesGrid.Items.IsEmpty && item != null)
            {
                if (DataContext is DetailsViewModel vm && vm.OpenItemOutCommand.CanExecute(item))
                    vm.OpenItemOutCommand.Execute(item);
            }
        }
        void Compare_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is DetailsViewModel vm && vm.CompareCommand.CanExecute(null))
                vm.CompareCommand.Execute(null);
        }

        void searchBtn_Click(object sender, RoutedEventArgs e)
        {
            string itemName = searchTxt.Text;
            if (ItemsBase.List.Any(x => x.ItemName == itemName))
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
        void searchTxt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                searchBtn_Click(null, new());
        }

        void itemsCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
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