using ItemChecker.MVVM.ViewModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ItemChecker.MVVM.View
{
    public partial class DetailsWindow : Window
    {
        public DetailsWindow(string itemName)
        {
            InitializeComponent();
            this.Title = $"{itemName} - Details";
            this.DataContext = new DetailsViewModel(itemName);
        }
        private void Window_DragMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void Button_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void currency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is DetailsViewModel vm && vm.SwitchCurrencyCommand.CanExecute(currency.SelectedItem))
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
    }
}
