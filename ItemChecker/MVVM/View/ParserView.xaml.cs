using ItemChecker.MVVM.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace ItemChecker.MVVM.View
{
    public partial class ParserView : UserControl
    {
        public ParserView()
        {
            InitializeComponent();
        }
        private void InputDecimal(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !decimal.TryParse(e.Text, out decimal result);
        }
        private void NumberPlus_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out int result);
        }

        private void DataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (!parserGrid.Items.IsEmpty)
            {
                ParserViewModel viewModel = (ParserViewModel)DataContext;
                if (e.Key == Key.Insert && viewModel.AddQueueCommand.CanExecute(null))
                    viewModel.AddQueueCommand.Execute(viewModel.SelectedItem);
                else if (e.Key == Key.F && Keyboard.IsKeyDown(Key.LeftCtrl) && viewModel.RemoveFavoriteCommand.CanExecute(null))
                    viewModel.RemoveFavoriteCommand.Execute(((ParserViewModel)DataContext).SelectedItem.ItemName);
                else if (e.Key == Key.F && viewModel.AddFavoriteCommand.CanExecute(null))
                    viewModel.AddFavoriteCommand.Execute(((ParserViewModel)DataContext).SelectedItem.ItemName);
            }
        }
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = parserGrid.CurrentItem;
            if (!parserGrid.Items.IsEmpty && item != null)
            {
                int columnIndex = parserGrid.CurrentColumn.DisplayIndex;
                if (DataContext is ParserViewModel viewModel && viewModel.OpenItemOutCommand.CanExecute(null))
                    viewModel.OpenItemOutCommand.Execute(columnIndex);
            }
        }
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SearchTxt.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void ComboBoxSer1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            csmGroup.IsEnabled = service1.SelectedIndex == 2;
        }
        private void currency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!parserGrid.Items.IsEmpty && DataContext is ParserViewModel vm && vm.SwitchCurrencyCommand.CanExecute(currency.SelectedItem))
                vm.SwitchCurrencyCommand.Execute(currency.SelectedItem);
        }
    }
}
