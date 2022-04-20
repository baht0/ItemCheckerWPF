using System.Collections.Generic;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class ParserInfo : BaseModel
    {
        private int _infoItemCurrent;
        private int _infoItemCount;
        private List<DataInventoriesCsm> _inventoryCsm = new();
        private DataInventoriesCsm _itemCsm = new();
        private decimal _itemCsmComparePrice = 0;
        private bool _csm = false;

        private Lfm _itemLf = new();
        private bool _lf = false;

        private DataSteamMarket _itemSt = new();
        private string _itemNameLfm = "Unknown";
        private bool _st = false;

        //info
        public int InfoItemCurrent
        {
            get
            {
                return _infoItemCurrent;
            }
            set
            {
                _infoItemCurrent = value;
                OnPropertyChanged();
                ItemCsm = InventoryCsm.Any() ? InventoryCsm[value] : new();
            }
        }
        public int InfoItemCount
        {
            get
            {
                return _infoItemCount;
            }
            set
            {
                _infoItemCount = value;
                OnPropertyChanged();
            }
        }
        public List<DataInventoriesCsm> InventoryCsm
        {
            get
            {
                return _inventoryCsm;
            }
            set
            {
                _inventoryCsm = value;
                OnPropertyChanged();
            }
        }
        public DataInventoriesCsm ItemCsm
        {
            get
            {
                return _itemCsm;
            }
            set
            {
                _itemCsm = value;
                OnPropertyChanged();
            }
        }
        public decimal ItemCsmComparePrice
        {
            get
            {
                return _itemCsmComparePrice;
            }
            set
            {
                _itemCsmComparePrice = value;
                OnPropertyChanged();
            }
        }
        public bool CSM
        {
            get
            {
                return _csm;
            }
            set
            {
                _csm = value;
                OnPropertyChanged();
            }
        }

        public Lfm ItemLf
        {
            get
            {
                return _itemLf;
            }
            set
            {
                _itemLf = value;
                OnPropertyChanged();
            }
        }
        public string ItemNameLfm
        {
            get
            {
                return _itemNameLfm;
            }
            set
            {
                _itemNameLfm = value;
                OnPropertyChanged();
            }
        }
        public bool LF
        {
            get
            {
                return _lf;
            }
            set
            {
                _lf = value;
                OnPropertyChanged();
            }
        }

        public DataSteamMarket ItemSt
        {
            get
            {
                return _itemSt;
            }
            set
            {
                _itemSt = value;
                OnPropertyChanged();
            }
        }
        public bool ST
        {
            get
            {
                return _st;
            }
            set
            {
                _st = value;
                OnPropertyChanged();
            }
        }
    }
}
