using ItemChecker.MVVM.ViewModel;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ItemChecker.MVVM.View
{
    /// <summary>
    /// Interaction logic for TradesWindow.xaml
    /// </summary>
    public partial class TradesWindow : Window
    {
        public TradesWindow()
        {
            InitializeComponent();
            DataContext = new TradesViewModel();
            YLabel.LabelFormatter = value => value.ToString("C");

        }
        private void Window_DragMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void Button_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void InputDecimal(object sender, TextCompositionEventArgs e)
        {
            decimal result;
            e.Handled = !decimal.TryParse(e.Text, out result);
        }
        private void currency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is TradesViewModel vm && vm.SwitchCurrencyCommand.CanExecute(currency.SelectedItem))
                vm.SwitchCurrencyCommand.Execute(currency.SelectedItem);
        }
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = tradesGrid.CurrentItem;
            if (!tradesGrid.Items.IsEmpty && item != null)
            {
                int columnIndex = tradesGrid.CurrentColumn.DisplayIndex;
                if (DataContext is TradesViewModel viewModel && viewModel.OpenItemOutCommand.CanExecute(columnIndex))
                    viewModel.OpenItemOutCommand.Execute(columnIndex);
            }
        }
        private void tradesGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (!tradesGrid.Items.IsEmpty)
            {
                var viewModel = (TradesViewModel)DataContext;
                if (e.Key == Key.Back && viewModel.RemoveCommand.CanExecute(viewModel.Trades.SelectedItem))
                    viewModel.RemoveCommand.Execute(viewModel.Trades.SelectedItem);
                if (e.Key == Key.F1)
                {
                    DetailsWindow detailsWindow = new(viewModel.Trades.SelectedItem.ItemName);
                    detailsWindow.Show();
                }
            }
        }
        private void Calculator_Click(object sender, RoutedEventArgs e)
        {
            if (!MainWindow.IsWindowOpen<Window>("calculatorWindow"))
            {
                CalculatorWindow window = new();
                window.Show();
            }
            else
            {
                Window window = Application.Current.Windows.OfType<Window>().Where(w => w.Name.Equals("calculatorWindow")).FirstOrDefault();
                window.WindowState = WindowState.Normal;
                window.Activate();
            }
        }

        private void dateintervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = dateintervalComboBox.SelectedIndex;
            switch (index)
            {
                case 0:
                    dateFrom.SelectedDate = DateTime.Today.AddDays(-7);
                    dateTo.SelectedDate = DateTime.Today;
                    break;
                case 1:
                    dateFrom.SelectedDate = DateTime.Today.AddDays(-30);
                    dateTo.SelectedDate = DateTime.Today;
                    break;
                case 2:
                    dateFrom.SelectedDate = DateTime.Today.AddMonths(-3);
                    dateTo.SelectedDate = DateTime.Today;
                    break;
                case 3:
                    dateFrom.SelectedDate = DateTime.Today.AddMonths(-6);
                    dateTo.SelectedDate = DateTime.Today;
                    break;
                case 4:
                    dateFrom.SelectedDate = DateTime.Today.AddYears(-5);
                    dateTo.SelectedDate = DateTime.Today.AddDays(8);
                    break;
            }
            dateintervalComboBox.SelectedIndex = index;
        }
        private void dateInterval_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            dateintervalComboBox.SelectedItem = "Custom";
        }
    }
}
