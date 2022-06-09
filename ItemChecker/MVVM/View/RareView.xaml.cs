using ItemChecker.MVVM.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace ItemChecker.MVVM.View
{
    /// <summary>
    /// Interaction logic for RareView.xaml
    /// </summary>
    public partial class RareView : UserControl
    {
        public RareView()
        {
            InitializeComponent();
        }
        private void InputDecimal(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !decimal.TryParse(e.Text, out decimal result);
        }
        private void InputInt(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out int result);
        }
        private void SearchEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SearchTxt.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
        private void currency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is RareViewModel vm && vm.SwitchCurrencyCommand.CanExecute(currency.SelectedItem))
                vm.SwitchCurrencyCommand.Execute(currency.SelectedItem);
        }
        private void TimerFloat_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is RareViewModel viewModel && viewModel.TimerCommand.CanExecute(null))
                viewModel.TimerCommand.Execute(null);
        }
        private void rareGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = rareGrid.CurrentItem;
            if (!rareGrid.Items.IsEmpty && item != null)
            {
                int columnIndex = rareGrid.CurrentColumn.DisplayIndex;
                if (DataContext is RareViewModel vm && vm.OpenItemOutCommand.CanExecute(columnIndex))
                    vm.OpenItemOutCommand.Execute(columnIndex);
            }
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            floatGroup.Visibility = parameter.SelectedIndex == 0 ? Visibility.Visible : Visibility.Hidden;
            stickerGroup.Visibility = parameter.SelectedIndex == 1 ? Visibility.Visible : Visibility.Hidden;
            phaseGroup.Visibility = parameter.SelectedIndex == 2 ? Visibility.Visible : Visibility.Hidden;
        }
        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is RareViewModel vm && vm.OpenStickerOutCommand.CanExecute(stickersList.SelectedItem))
                vm.OpenStickerOutCommand.Execute(stickersList.SelectedItem);
        }
    }
}
