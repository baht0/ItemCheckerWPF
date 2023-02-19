using ItemChecker.MVVM.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Linq;
using System;
using ItemChecker.MVVM.Model;

namespace ItemChecker.MVVM.View
{
    public partial class RareView : UserControl
    {
        public RareView()
        {
            InitializeComponent();
            qualityChb.IsEnabled = false;
        }
        void InputDecimal(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !decimal.TryParse(e.Text, out decimal result);
        }
        void InputInt(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out int result);
        }
        void SearchEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SearchTxt.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
        void currency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is RareViewModel vm && vm.SwitchCurrencyCommand.CanExecute(currency.SelectedItem))
                vm.SwitchCurrencyCommand.Execute(currency.SelectedItem);
        }
        void TimerFloat_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is RareViewModel viewModel && viewModel.TimerCommand.CanExecute(null))
                viewModel.TimerCommand.Execute(null);
        }
        void rareGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = rareGrid.CurrentItem;
            if (!rareGrid.Items.IsEmpty && item != null)
            {
                int columnIndex = rareGrid.CurrentColumn.DisplayIndex;
                if (DataContext is RareViewModel vm && vm.OpenItemOutCommand.CanExecute(columnIndex))
                    vm.OpenItemOutCommand.Execute(columnIndex);
            }
        }
        void rareGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (!rareGrid.Items.IsEmpty)
            {
                var viewModel = (RareViewModel)DataContext;
                var item = viewModel.DataGridRare.SelectedItem;
                if (e.Key == Key.F1)
                    MainWindow.OpenDetailsItem(item.ItemName);
            }
        }
        void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            floatGroup.Visibility = parameter.SelectedIndex == 0 ? Visibility.Visible : Visibility.Hidden;
            stickerGroup.Visibility = parameter.SelectedIndex == 1 ? Visibility.Visible : Visibility.Hidden;
            phaseGroup.Visibility = parameter.SelectedIndex == 2 ? Visibility.Visible : Visibility.Hidden;
        }
        void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is RareViewModel vm && vm.OpenStickerOutCommand.CanExecute(stickersList.SelectedItem))
                vm.OpenStickerOutCommand.Execute(stickersList.SelectedItem);
        }
        void ListShow_Click(object sender, RoutedEventArgs e)
        {
            SavedItems.ShowListName = "Rare";
            if (!MainWindow.IsWindowOpen<Window>("showListWindow"))
            {
                ShowListWindow window = new();
                window.Show();
            }
            else
            {
                Window wnd = Application.Current.Windows.OfType<Window>().Where(w => w.Name.Equals("showListWindow")).FirstOrDefault();
                wnd.Activate();
            }
        }

        void stickerQuality_Checked(object sender, RoutedEventArgs e)
        {
            qualityChb.IsEnabled = StickerQuality();
        }

        void stickerQuality_Unchecked(object sender, RoutedEventArgs e)
        {
            qualityChb.IsEnabled = StickerQuality();
        }
        Boolean StickerQuality()
        {
            return (bool)stickerNormal.IsChecked
                || (bool)stickerHolo.IsChecked
                || (bool)stickerGlitter.IsChecked
                || (bool)stickerFoil.IsChecked
                || (bool)stickerLenticular.IsChecked
                || (bool)stickerGold.IsChecked
                || (bool)stickerContraband.IsChecked;
        }
    }
}
