using ItemChecker.MVVM.ViewModel;
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
        private void Calc_Click(object sender, RoutedEventArgs e)
        {
            CalculatorWindow calculator = new();
            calculator.Show();
        }
        private void Set_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow setting = new();
            setting.ShowDialog();
        }
    }
}