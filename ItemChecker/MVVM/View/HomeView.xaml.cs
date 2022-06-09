using ItemChecker.MVVM.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace ItemChecker.MVVM.View
{
    public partial class BuyOrderView : UserControl
    {
        public BuyOrderView()
        {
            InitializeComponent();
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
        private void DataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (!ordersGrid.Items.IsEmpty)
            {
                HomeViewModel viewModel = (HomeViewModel)DataContext;
                if (e.Key == Key.Back && viewModel.CancelOrderCommand.CanExecute(null))
                    viewModel.CancelOrderCommand.Execute(viewModel.HomeTable.SelectedOrderItem);
            }
        }
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = ordersGrid.CurrentItem;
            if (!ordersGrid.Items.IsEmpty && item != null)
            {
                int columnIndex = ordersGrid.CurrentColumn.DisplayIndex;
                if (DataContext is HomeViewModel viewModel && viewModel.OpenItemOutCommand.CanExecute(null))
                    viewModel.OpenItemOutCommand.Execute(columnIndex);
            }
        }
        private void TimerPush_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            HomeViewModel viewModel = (HomeViewModel)DataContext;
            if (viewModel.TimerCommand.CanExecute(null))
                viewModel.TimerCommand.Execute(0);
        }

        private void inventComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            sellGroup.IsEnabled = invenTasks.SelectedIndex == 1;
        }
    }
}