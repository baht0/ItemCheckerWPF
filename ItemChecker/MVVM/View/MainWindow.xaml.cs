﻿using ItemChecker.MVVM.Model;
using ItemChecker.MVVM.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace ItemChecker.MVVM.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new BuyOrderViewModel();
        }
        private void DragMove_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void Home_Click(object sender, RoutedEventArgs e)
        {
            if (!Main.IsLoading & !Main.Timer.Enabled)
                DataContext = new BuyOrderViewModel();
        }
        private void Parser_Click(object sender, RoutedEventArgs e)
        {
            if (!Main.IsLoading & !Main.Timer.Enabled)
                DataContext = new ParserViewModel();
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow settingWindow = new();
            settingWindow.ShowDialog();
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
    }
}
