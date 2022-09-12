using ItemChecker.Core;
using System.Collections.ObjectModel;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class Home
    {
        public string CurrencySymbol { get; set; } = SteamBase.CurrencyList.FirstOrDefault(x => x.Id == SteamAccount.CurrencyId).Symbol;
    }
    public class HomeTable : ObservableObject
    {
        private ObservableCollection<DataOrder> _orderedGrid = new(SteamMarket.Orders);
        private DataOrder _selectedOrderItem;
        public ObservableCollection<DataOrder> OrderedGrid
        {
            get
            {
                return _orderedGrid;
            }
            set
            {
                _orderedGrid = value;
                OnPropertyChanged();
            }
        }
        public DataOrder SelectedOrderItem
        {
            get
            {
                return _selectedOrderItem;
            }
            set
            {
                _selectedOrderItem = value;
                OnPropertyChanged();
            }
        }
    }
}
