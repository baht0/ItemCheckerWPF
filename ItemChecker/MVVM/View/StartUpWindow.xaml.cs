using ItemChecker.Core;
using ItemChecker.MVVM.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace ItemChecker.MVVM.View
{
    public partial class StartWindow : Window, IView
    {
        public StartWindow()
        {
            InitializeComponent();
            this.DataContext = new StartUpViewModel(this);
        }

        private void Window_DragMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to close?", "Question",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
                return;
            if (DataContext is StartUpViewModel vm)
                vm.ExitCommand.Execute(null);
        }
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to delete all saved account data?\n\nThe settings are saved.", "Question",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
                return;
            if (DataContext is StartUpViewModel vm)
            {
                vm.DeleteDataCommand.Execute(null);
                vm.ExitCommand.Execute(null);
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                StartUpViewModel viewModel = (StartUpViewModel)DataContext;
                if (viewModel.LoginCommand.CanExecute(null))
                    viewModel.LoginCommand.Execute(passTextbox);
            }
        }
    }
}