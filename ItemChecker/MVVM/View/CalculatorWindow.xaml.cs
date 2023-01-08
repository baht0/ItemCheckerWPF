using ItemChecker.MVVM.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ItemChecker.MVVM.View
{
    public partial class CalculatorWindow : Window
    {
        public CalculatorWindow()
        {
            InitializeComponent();
            this.DataContext = new CalculatorViewModel();
        }
        private void Window_DragMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void MinWin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void calculatorWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        private void InputDecimal(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !decimal.TryParse(e.Text, out decimal result);
        }
        private void compare_TextChanged(object sender, TextChangedEventArgs e)
        {
            string[] values = { purchaseTxt.Text.Replace(",", "."), priceTxt.Text.Replace(",", "."), commissionTxt.Text };

            if (DataContext is CalculatorViewModel viewModel && viewModel.CompareCommand.CanExecute(values))
                viewModel.CompareCommand.Execute(values);
        }
        private void Commission_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            commissionTxt.IsEnabled = commissionCmb.SelectedItem == "Custom";
            commissionTxt.Text = "0";

            if (DataContext is CalculatorViewModel viewModel && viewModel.CommissionCommand.CanExecute(commissionTxt.Text))
                viewModel.CommissionCommand.Execute(commissionTxt.Text);
        }
        private void commissionTxt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
                commissionCmb.Focus();
        }

        private void ValueTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is CalculatorViewModel viewModel && viewModel.CurrencyConvertCommand.CanExecute(ValueTxt.Text.Replace(",", ".")))
                viewModel.CurrencyConvertCommand.Execute(ValueTxt.Text.Replace(",", "."));
        }
        private void copy_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as TextBlock;
            Clipboard.SetText(item.Text);
        }
    }
}