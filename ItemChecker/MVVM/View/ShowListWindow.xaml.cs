using ItemChecker.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ItemChecker.MVVM.View
{
    /// <summary>
    /// Interaction logic for ShowListWindow.xaml
    /// </summary>
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

        private void favoriteGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (!favoriteGrid.Items.IsEmpty)
            {
                var viewModel = (ShowListViewModel)DataContext;
                if (e.Key == Key.Back && viewModel.RemoveCommand.CanExecute(null))
                    viewModel.RemoveCommand.Execute(viewModel.ItemsList.SelectedItem);
                if (e.Key == Key.F1)
                {
                    DetailsWindow detailsWindow = new(viewModel.ItemsList.SelectedItem.ItemName);
                    detailsWindow.Show();
                }
            }
        }
    }
}
