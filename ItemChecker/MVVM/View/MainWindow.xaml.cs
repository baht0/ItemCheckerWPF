using ItemChecker.MVVM.ViewModel;
using ItemChecker.Support;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ItemChecker.MVVM.View
{
    public partial class MainWindow : Window
    {
        HomeViewModel homeViewModel = new();
        ParserViewModel parserViewModel = new();
        RareViewModel rareViewModel = new();

        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
               ? Application.Current.Windows.OfType<T>().Any()
               : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            bodyContent.Content = homeViewModel;
            Home.IsEnabled = false;
            Title = "Home - ItemChecker";
        }
        private void Window_DragMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void MinWin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void Button_Close(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void buttonItemChecker_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            ContextMenu contextMenu = btn.ContextMenu;
            contextMenu.PlacementTarget = btn;
            contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            contextMenu.IsOpen = true;
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var obj = sender as MenuItem;
            switch (obj.Header)
            {
                case "Trades":
                    {
                        if (!MainWindow.IsWindowOpen<Window>("tradesWindow"))
                        {
                            TradesWindow window = new();
                            window.Show();
                        }
                        else
                        {
                            Window window = Application.Current.Windows.OfType<Window>().Where(w => w.Name.Equals("tradesWindow")).FirstOrDefault();
                            window.Activate();
                        }
                        break;
                    }
                case "Calculator":
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
                        break;
                    }
                case "SteamMarket":
                    {
                        Edit.OpenUrl("https://steamcommunity.com/market/");
                        break;
                    }
                case "MyInventory":
                    {
                        Edit.OpenUrl("https://steamcommunity.com/my/inventory/");
                        break;
                    }
                case "Cs.Money":
                    {
                        Edit.OpenUrl("https://cs.money/");
                        break;
                    }
                case "Loot.Farm":
                    {
                        Edit.OpenUrl("https://loot.farm/");
                        break;
                    }
                case "Buff163":
                    {
                        Edit.OpenUrl("https://buff.163.com/");
                        break;
                    }
                case "Settings":
                    {
                        SettingWindow window = new();
                        window.ShowDialog();
                        break;
                    }
                case "Exit":
                    {
                        MessageBoxResult result = MessageBox.Show("Are you sure you want to close?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes && DataContext is MainViewModel viewModel && viewModel.ExitCommand.CanExecute(null))
                            viewModel.ExitCommand.Execute(null);
                        break;
                    }
            }
        }
        private void Home_Click(object sender, RoutedEventArgs e)
        {
            if (bodyContent.Content != homeViewModel)
            {
                Title = "Home - ItemChecker";
                bodyContent.Content = homeViewModel;
                Home.IsEnabled = false;
                Parser.IsEnabled = true;
                Rare.IsEnabled = true;
            }
        }
        private void Parser_Click(object sender, RoutedEventArgs e)
        {
            if (bodyContent.Content != parserViewModel)
            {
                Title = "Parser - ItemChecker";
                bodyContent.Content = parserViewModel;
                Home.IsEnabled = true;
                Parser.IsEnabled = false;
                Rare.IsEnabled = true;
            }
        }
        private void Rare_Click(object sender, RoutedEventArgs e)
        {
            if (bodyContent.Content != rareViewModel)
            {
                Title = "Rare - ItemChecker";
                bodyContent.Content = rareViewModel;
                Home.IsEnabled = true;
                Parser.IsEnabled = true;
                Rare.IsEnabled = false;
            }
        }

        private void Notification_Click(object sender, RoutedEventArgs e)
        {
            PopupNotification.IsOpen = true;
        }
        private void PopupNotification_Closed(object sender, System.EventArgs e)
        {
            MainViewModel viewModel = (MainViewModel)DataContext;
            if (viewModel.ReadNotificationCommand.CanExecute(null))
                viewModel.ReadNotificationCommand.Execute(null);
        }
    }
}