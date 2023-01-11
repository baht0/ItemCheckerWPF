using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using ItemChecker.MVVM.ViewModel;

namespace ItemChecker.MVVM.View
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
            this.DataContext = new SettingViewModel();
        }
        void Window_DragMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        void Button_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        void Decimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !decimal.TryParse(e.Text, out decimal result);
        }
        void String_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new(@"^[a-zA-Z_]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        void whatIsNewButton_Click(object sender, RoutedEventArgs e)
        {
            if (!MainWindow.IsWindowOpen<Window>("whatsNewWindow"))
            {
                WhatsNewWindow window = new();
                window.Show();
            }
            else
            {
                Window window = Application.Current.Windows.OfType<Window>().Where(w => w.Name.Equals("whatsNewWindow")).FirstOrDefault();
                window.Activate();
            }
        }
    }
}