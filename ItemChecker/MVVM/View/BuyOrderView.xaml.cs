using ItemChecker.MVVM.Model;
using ItemChecker.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for SteamView.xaml
    /// </summary>
    public partial class BuyOrderView : UserControl
    {
        public BuyOrderView()
        {
            InitializeComponent();
            this.DataContext = new BuyOrderViewModel();
        }
    }
}
