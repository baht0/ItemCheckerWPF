using ItemChecker.MVVM.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace ItemChecker.MVVM.View
{
    public partial class ShowListWindow : Window
    {
        public ShowListWindow(string listName)
        {
            InitializeComponent();
            DataContext = new ShowListViewModel(listName);
        }
        private void Window_DragMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void Button_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void reserveGrid_KeyDown(object sender, KeyEventArgs e)
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
