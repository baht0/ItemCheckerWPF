using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ItemChecker.MVVM.View;

namespace ItemChecker.MVVM.ViewModel
{
    public class BuyOrderViewModel : MainViewModel
    {
        private int _itemCount;
        private List<MyOrder> orderGrid;
        public int ItemCount
        {
            get
            {
                return _itemCount;
            }
            set
            {
                _itemCount = value;
                OnPropertyChanged();
            }
        }
        public List<MyOrder> OrderedGrid
        {
            get
            {
                return orderGrid;
            }
            set
            {
                orderGrid = value;
                OnPropertyChanged();
            }
        }
        public BuyOrderViewModel()
        {
            OrderedGrid = Account.MyOrders;
            ItemCount = Account.MyOrders.Count;
        }
    }
}
