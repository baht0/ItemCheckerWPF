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
            this.DataContext = new ParserViewModel();
        }
        private void Decimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            decimal result;
            e.Handled = !decimal.TryParse(e.Text, out result);
        }
        private void Number_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int result;
            e.Handled = !int.TryParse(e.Text, out result);
        }

        private void DataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (!parserGrid.Items.IsEmpty & e.Key == Key.Insert)
            {
                var viewModel = (ParserViewModel)DataContext;
                if (viewModel.AddQueueCommand.CanExecute(null))
                    viewModel.AddQueueCommand.Execute(viewModel.SelectedParserItem);
            }
        }
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!parserGrid.Items.IsEmpty)
            {
                object item = parserGrid.CurrentItem;
                PropertyInfo info = item.GetType().GetProperty("ItemName");
                string ItemName = (string)info.GetValue(item, null);

                string market_has_name = Edit.MarketHashName(ItemName);
                int columnIndex = parserGrid.CurrentColumn.DisplayIndex;
                switch (columnIndex)
                {
                    case 2:
                        if (Service1.Text.Contains("SteamMarket"))
                            Edit.openUrl("https://steamcommunity.com/market/listings/730/" + market_has_name);
                        else if (Service1.Text.Contains("Cs.Money"))
                            Edit.openCsm(market_has_name);
                        break;
                    case 3:
                        if (Service1.Text.Contains("SteamMarket"))
                            Edit.openUrl("https://steamcommunity.com/market/listings/730/" + market_has_name);
                        else if (Service1.Text.Contains("Cs.Money"))
                            Edit.openCsm(market_has_name);
                        break;
                    case 4:
                        if (Service2.Text.Contains("SteamMarket"))
                            Edit.openUrl("https://steamcommunity.com/market/listings/730/" + market_has_name);
                        else if (Service2.Text.Contains("Cs.Money"))
                            Edit.openCsm(market_has_name);
                        break;
                    case 5:
                        if (Service2.Text.Contains("SteamMarket"))
                            Edit.openUrl("https://steamcommunity.com/market/listings/730/" + market_has_name);
                        else if (Service2.Text.Contains("Cs.Money"))
                            Edit.openCsm(market_has_name);
                        break;
                    default:
                        Clipboard.SetText(ItemName);
                        break;
                }
            }
        }
    }
}
