using ItemChecker.Core;
using ItemChecker.MVVM.Model;
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Main.IsLoading)
                e.Cancel = true;
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to close?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
                return;
            StartUpViewModel viewModel = (StartUpViewModel)DataContext;
            if (viewModel.ExitCommand.CanExecute(null))
                viewModel.ExitCommand.Execute(null);
        }

        private void codeTextbox_KeyDown(object sender, KeyEventArgs e)
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