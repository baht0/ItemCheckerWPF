using ItemChecker.MVVM.ViewModel;
using System;
using System.Linq;
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

        private void currency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!parserGrid.Items.IsEmpty && DataContext is ParserViewModel vm && vm.SwitchCurrencyCommand.CanExecute(currency.SelectedItem))
                vm.SwitchCurrencyCommand.Execute(currency.SelectedItem);
        }
        private void DataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (!parserGrid.Items.IsEmpty)
            {
                ParserViewModel viewModel = (ParserViewModel)DataContext;
                var item = viewModel.ParserTable.SelectedItem;
                if (e.Key == Key.Insert && viewModel.AddQueueCommand.CanExecute(item))
                {
                    viewModel.AddQueueCommand.Execute(item);
                    checkTab.IsChecked = false;
                    queueTab.IsChecked = true;
                }
                if (e.Key == Key.F && Keyboard.IsKeyDown(Key.LeftCtrl) && viewModel.RemoveReserveCommand.CanExecute(item.ItemName))
                    viewModel.RemoveReserveCommand.Execute(item.ItemName);
                else if (e.Key == Key.F && viewModel.AddReserveCommand.CanExecute(item.ItemName))
                    viewModel.AddReserveCommand.Execute(item.ItemName);
                if (e.Key == Key.F1)
                    MainWindow.OpenDetailsItem(item.ItemName);
            }
        }
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = parserGrid.CurrentItem;
            if (!parserGrid.Items.IsEmpty && item != null)
            {
                int columnIndex = parserGrid.CurrentColumn.DisplayIndex;
                if (DataContext is ParserViewModel viewModel && viewModel.OpenItemOutCommand.CanExecute(columnIndex))
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
            int id = service1.SelectedIndex;
            csmGroup.IsEnabled = id == 2;
            maxPriceTxt.IsReadOnly = true;
            if (DataContext is ParserViewModel viewModel && viewModel.MaxPriceCommand.CanExecute(id))
                viewModel.MaxPriceCommand.Execute(id);
        }
        private void Import_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ImportParserWindow window = new();
            window.ShowDialog();
            if (!String.IsNullOrEmpty(window.ReturnValue))
            {
                string path = window.ReturnValue;
                if (DataContext is ParserViewModel viewModel && viewModel.ImportCommand.CanExecute(path))
                    viewModel.ImportCommand.Execute(path);
            }
        }

        private void queueListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!queueListBox.Items.IsEmpty)
            {
                ParserViewModel viewModel = (ParserViewModel)DataContext;
                var item = viewModel.ParserQueue.SelectedQueue;
                if (e.Key == Key.Back && viewModel.RemoveQueueCommand.CanExecute(item))
                    viewModel.RemoveQueueCommand.Execute(item);
                if (e.Key == Key.F1)
                    MainWindow.OpenDetailsItem(item.ItemName);
            }
        }
        private void queueListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!queueListBox.Items.IsEmpty)
            {
                var selectedItem = parserGrid.Items.OfType<Model.DataParser>().FirstOrDefault(x => x.ItemName == (queueListBox.SelectedItem as Model.DataQueue).ItemName);
                if (selectedItem != null)
                {
                    parserGrid.UpdateLayout();
                    parserGrid.SelectedItem = selectedItem;
                    parserGrid.ScrollIntoView(selectedItem);
                }
            }
        }

        private void maxPriceTxt_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            maxPriceTxt.IsReadOnly = false;
        }
    }
}
