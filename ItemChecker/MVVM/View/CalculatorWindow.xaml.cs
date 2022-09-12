using ItemChecker.MVVM.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ItemChecker.MVVM.View
{
    /// <summary>
    /// Interaction logic for CalculatorWindow.xaml
    /// </summary>
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

        private void InputDecimal(object sender, TextCompositionEventArgs e)
        {
            decimal result;
            e.Handled = !decimal.TryParse(e.Text, out result);
        }
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ValueTxt.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                price1.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                price2.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
        private void Commission_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is CalculatorViewModel viewModel && viewModel.CommissionCommand.CanExecute(null))
                viewModel.CommissionCommand.Execute(null);
        }
    }
}
