using ItemChecker.MVVM.ViewModel;
using ItemChecker.Support;
using System.Reflection;
using System.Windows;
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
            decimal result;
            e.Handled = !decimal.TryParse(e.Text, out result);
        }
        private void NumberPlus_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int result;
            e.Handled = !int.TryParse(e.Text, out result);
        }

        private void DataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (!parserGrid.Items.IsEmpty)
            {
                ParserViewModel viewModel = (ParserViewModel)DataContext;
                if (e.Key == Key.Insert && viewModel.AddQueueCommand.CanExecute(null))
                    viewModel.AddQueueCommand.Execute(viewModel.SelectedParserItem);
                else if (e.Key == Key.F && Keyboard.IsKeyDown(Key.LeftCtrl) && viewModel.RemoveFavoriteCommand.CanExecute(null))
                    viewModel.RemoveFavoriteCommand.Execute(((ParserViewModel)DataContext).SelectedParserItem.ItemName);
                else if (e.Key == Key.F && viewModel.AddFavoriteCommand.CanExecute(null))
                    viewModel.AddFavoriteCommand.Execute(((ParserViewModel)DataContext).SelectedParserItem.ItemName);
            }
        }
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = parserGrid.CurrentItem;
            if (!parserGrid.Items.IsEmpty && item != null)
            {
                int columnIndex = parserGrid.CurrentColumn.DisplayIndex;
                ParserViewModel viewModel = (ParserViewModel)DataContext;
                if (viewModel.OpenItemOutCommand.CanExecute(null))
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
            if (service1.SelectedIndex == 2)
                csmGroup.IsEnabled = true;
            else
                csmGroup.IsEnabled = false;
        }
    }
}
