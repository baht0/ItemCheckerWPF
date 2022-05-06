using ItemChecker.MVVM.ViewModel;
using ItemChecker.Support;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ItemChecker.MVVM.View
{
    public partial class MainWindow : Window
    {
        HomeViewModel homeViewModel = new();
        ParserViewModel parserViewModel = new();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            bodyContent.Content = homeViewModel;
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
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            if (bodyContent.Content != homeViewModel)
                bodyContent.Content = homeViewModel;
        }
        private void Parser_Click(object sender, RoutedEventArgs e)
        {
            if (bodyContent.Content != parserViewModel)
                bodyContent.Content = parserViewModel;
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Image rectangle = sender as Image;
                ContextMenu contextMenu = rectangle.ContextMenu;
                contextMenu.PlacementTarget = rectangle;
                contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                contextMenu.IsOpen = true;
            }
        }
        private void Calculator_Click(object sender, RoutedEventArgs e)
        {
            PopupCalculator.IsOpen = true;
        }
        private void Notification_Click(object sender, RoutedEventArgs e)
        {
            PopupNotification.IsOpen = true;
        }
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow setting = new();
            setting.ShowDialog();
        }

        private void CSM_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Edit.openUrl("https://cs.money/");
        }
        private void Lf_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Edit.openUrl("https://loot.farm/");
        }
        private void Buff_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Edit.openUrl("https://buff.163.com/");
        }
        private void Inventory_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Edit.openUrl("https://steamcommunity.com/my/inventory/");
        }
        private void SteamMarket_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Edit.openUrl("https://steamcommunity.com/market/");
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
    }
}