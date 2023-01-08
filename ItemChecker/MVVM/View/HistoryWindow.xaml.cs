using ItemChecker.MVVM.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ItemChecker.MVVM.View
{
    public partial class HistoryWindow : Window
    {
        public HistoryWindow()
        {
            InitializeComponent();
            DataContext = new HistoryViewModel();
        }
        private void Window_DragMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void Button_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void historyWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        private void currency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is HistoryViewModel vm && vm.SwitchCurrencyCommand.CanExecute(currency.SelectedItem))
                vm.SwitchCurrencyCommand.Execute(currency.SelectedItem);
        }

        private void dateintervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is HistoryViewModel vm && vm.SwitchIntervalCommand.CanExecute(currency.SelectedItem))
            {
                int index = dateintervalComboBox.SelectedIndex;
                vm.SwitchIntervalCommand.Execute(index);
            }
        }
        private void dateInterval_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            dateintervalComboBox.SelectedItem = "Custom";
        }
    }
}
