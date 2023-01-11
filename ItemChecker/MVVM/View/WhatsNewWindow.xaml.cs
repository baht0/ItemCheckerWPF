using ItemChecker.MVVM.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace ItemChecker.MVVM.View
{
    /// <summary>
    /// Interaction logic for WhatsNewWindow.xaml
    /// </summary>
    public partial class WhatsNewWindow : Window
    {
        public WhatsNewWindow()
        {
            InitializeComponent();
            this.DataContext = new WhatsNewViewModel();
        }
        void Window_DragMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        void Button_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        void whatsNewWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            WhatsNewViewModel viewModel = (WhatsNewViewModel)DataContext;
            if (viewModel.ShowCommand.CanExecute(null))
                viewModel.ShowCommand.Execute(ListUpdates.SelectedItem);
        }
    }
}
