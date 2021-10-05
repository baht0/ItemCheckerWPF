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

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
