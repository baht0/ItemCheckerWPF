using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class ParserInfo : BaseModel
    {
        private int _infoItemCurrent;
        private int _infoItemCount;
        private List<DataInventoryCsm> _inventoryCsm = new();
        private DataInventoryCsm _itemCsm = new();
        private bool _csm = false;
        private DataInventoryLf _itemLf = new();
        private bool _lf = false;
        private DataSteamMarket _itemSt = new();
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
        public List<DataInventoryCsm> InventoryCsm
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
        public DataInventoryCsm ItemCsm
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
        public DataInventoryLf ItemLf
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
