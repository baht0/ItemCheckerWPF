using ItemChecker.MVVM.ViewModel;
using System.Windows;
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
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void InputDecimal(object sender, TextCompositionEventArgs e)
        {
            decimal result;
            e.Handled = !decimal.TryParse(e.Text, out result);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
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
    }
}
