using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using ItemChecker.MVVM.ViewModel;

namespace ItemChecker.MVVM.View
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
            this.DataContext = new SettingViewModel();
        }

        private void Window_DragMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void Button_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Number_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int result;
            e.Handled = !int.TryParse(e.Text, out result);
        }
        private void Decimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            decimal result;
            e.Handled = !decimal.TryParse(e.Text, out result);
        }
        private void String_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new(@"^[a-zA-Z_]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }
    }
}