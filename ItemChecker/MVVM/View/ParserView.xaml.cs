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
                PropertyInfo info = item.GetType().GetProperty("ItemName");
                string ItemName = (string)info.GetValue(item, null);
                ItemName = Edit.removeDoppler(ItemName).Replace("(Holo/Foil)", "(Holo-Foil)");
                string market_has_name = Edit.MarketHashName(ItemName);

                ParserViewModel viewModel = (ParserViewModel)DataContext;
                int columnIndex = parserGrid.CurrentColumn.DisplayIndex;
                switch (columnIndex)
                {
                    case 1 or 2:
                        if (viewModel.ParserStatistics.Service1 == "SteamMarket" | viewModel.ParserStatistics.Service1 == "SteamMarket(A)")
                            Edit.openUrl("https://steamcommunity.com/market/listings/730/" + market_has_name);
                        else if (viewModel.ParserStatistics.Service1 == "Cs.Money")
                            Edit.openCsm(market_has_name);
                        break;
                    case 3 or 4:
                        if (viewModel.ParserStatistics.Service2 == "SteamMarket" | viewModel.ParserStatistics.Service2 == "SteamMarket(A)")
                            Edit.openUrl("https://steamcommunity.com/market/listings/730/" + market_has_name);
                        else if (viewModel.ParserStatistics.Service2 == "Cs.Money")
                            Edit.openCsm(market_has_name);
                        break;
                    default:
                        Clipboard.SetText(ItemName);
                        break;
                }
            }
        }
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SearchTxt.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
    }
}
