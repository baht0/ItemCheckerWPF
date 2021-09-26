using ItemChecker.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ItemChecker.MVVM.View
{
    /// <summary>
    /// Interaction logic for ParserView.xaml
    /// </summary>
    public partial class ParserView : UserControl
    {
        public ParserView()
        {
            InitializeComponent();
            this.DataContext = new ParserViewModel();
        }
        private void PricePreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void PrecentPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new(@"^(0|-[1-9]\d{0,3})$");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
