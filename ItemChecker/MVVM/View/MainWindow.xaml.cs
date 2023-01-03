﻿using ItemChecker.MVVM.Model;
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