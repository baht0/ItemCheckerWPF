using ItemChecker.MVVM.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace ItemChecker.MVVM.View
{
    public partial class ShowListWindow : Window
    {
        public ShowListWindow()
        {
            InitializeComponent();
            DataContext = new ShowListViewModel();
        }
        void Window_DragMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        void Button_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        void showListWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        void reserveGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (!reserveGrid.Items.IsEmpty)
            {
                var viewModel = (ShowListViewModel)DataContext;
                var item = viewModel.SavedItems.SelectedItem;
                if (e.Key == Key.Back && viewModel.RemoveCommand.CanExecute(item))
                    viewModel.RemoveCommand.Execute(item);
                if (e.Key == Key.F1)
                    MainWindow.OpenDetailsItem(item.ItemName);
            }
        }
    }
}
