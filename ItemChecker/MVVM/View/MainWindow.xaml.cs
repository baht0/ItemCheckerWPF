using ItemChecker.MVVM.Model;
using ItemChecker.MVVM.ViewModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ItemChecker.MVVM.View
{
    public partial class MainWindow : Window
    {
        readonly HomeViewModel homeViewModel = new();
        readonly ParserViewModel parserViewModel = new();
        readonly RareViewModel rareViewModel = new();

        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
               ? Application.Current.Windows.OfType<T>().Any()
               : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }
        public static void OpenDetailsItem(string itemName)
        {
            if (!string.IsNullOrEmpty(itemName))
            {
                Details.Items.Add(itemName);
                Details.Item = Details.Items.FirstOrDefault(x => x.ItemName == itemName);
            }
            if (!IsWindowOpen<Window>("detailsWindow"))
            {
                DetailsWindow window = new(string.IsNullOrEmpty(itemName));
                window.Show();
            }
            else
            {
                Window window = Application.Current.Windows.OfType<Window>().Where(w => w.Name.Equals("detailsWindow")).FirstOrDefault();
                window.Activate();
            }
        }
        public static void CloseShowListWin(string listName)
        {
            if (IsWindowOpen<Window>("showListWindow") && listName == SavedItems.ShowListName)
            {
                Window window = Application.Current.Windows.OfType<Window>().Where(w => w.Name.Equals("showListWindow")).FirstOrDefault();
                window.Close();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            bodyContent.Content = homeViewModel;
            Home.IsEnabled = false;
            Title = "ItemChecker - Home";
        }
        void Window_DragMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        void MinWin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        void Button_Close(object sender, RoutedEventArgs e)
        {
            Close();
        }
        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        void buttonItemChecker_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            ContextMenu contextMenu = btn.ContextMenu;
            contextMenu.PlacementTarget = btn;
            contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            contextMenu.IsOpen = true;
        }
        void Home_Click(object sender, RoutedEventArgs e)
        {
            if (bodyContent.Content != homeViewModel)
            {
                Title = "ItemChecker - Home";
                bodyContent.Content = homeViewModel;
                Home.IsEnabled = false;
                Parser.IsEnabled = true;
                Rare.IsEnabled = true;
            }
        }
        void Parser_Click(object sender, RoutedEventArgs e)
        {
            if (bodyContent.Content != parserViewModel)
            {
                Title = "ItemChecker - Parser";
                bodyContent.Content = parserViewModel;
                Home.IsEnabled = true;
                Parser.IsEnabled = false;
                Rare.IsEnabled = true;
            }
        }
        void Rare_Click(object sender, RoutedEventArgs e)
        {
            if (bodyContent.Content != rareViewModel)
            {
                Title = "ItemChecker - Rare";
                bodyContent.Content = rareViewModel;
                Home.IsEnabled = true;
                Parser.IsEnabled = true;
                Rare.IsEnabled = false;
            }
        }

        void Notification_Click(object sender, RoutedEventArgs e)
        {
            PopupNotification.IsOpen = true;
        }
        void PopupNotification_Closed(object sender, System.EventArgs e)
        {
            MainViewModel viewModel = (MainViewModel)DataContext;
            if (viewModel.ReadNotificationCommand.CanExecute(null))
                viewModel.ReadNotificationCommand.Execute(null);
        }
    }
}