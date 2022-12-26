using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.MVVM.ViewModel;
using ItemChecker.Properties;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
            Application.Current.Shutdown();
        }
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to delete all saved account data?\n\nThe settings are saved.", "Question",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
                return;

            string path = ProjectInfo.DocumentPath + "Net";
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            MainProperties.Default.SteamCurrencyId = 0;
            MainProperties.Default.Save();

            Application.Current.Shutdown();
        }

        private void passTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                StartUpViewModel viewModel = (StartUpViewModel)DataContext;
                if (viewModel.SignInCommand.CanExecute(null))
                    viewModel.SignInCommand.Execute(passTextbox);
            }
        }
        private void code2FA_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (code2FA.Text.Length == 5)
            {
                StartUpViewModel viewModel = (StartUpViewModel)DataContext;
                if (viewModel.SubmitCodeCommand.CanExecute(null))
                    viewModel.SubmitCodeCommand.Execute(code2FA.Text);
            }
        }
    }
}