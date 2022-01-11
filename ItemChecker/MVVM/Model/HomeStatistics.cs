using ItemChecker.Properties;

namespace ItemChecker.MVVM.Model
{
    public class HomeStatistics : BaseModel
    {
        //services
        private string _pushButton = "Play";
        private string _csmButton = "Play";
        private string _floatButton = "Play";
        public string PushButton
        {
            get { return _pushButton; }
            set
            {
                _pushButton = value;
                OnPropertyChanged();
            }
        }
        public string CsmButton
        {
            get { return _csmButton; }
            set
            {
                _csmButton = value;
                OnPropertyChanged();
            }
        }
        public string FloatButton
        {
            get { return _floatButton; }
            set
            {
                _floatButton = value;
                OnPropertyChanged();
            }
        }

        private bool _pushService;
        private bool _csmService;
        private bool _floatService;
        public bool PushService
        {
            get
            {
                return _pushService;
            }
            set
            {
                _pushService = value;
                OnPropertyChanged();
            }
        }
        public bool CsmService
        {
            get
            {
                return _csmService;
            }
            set
            {
                _csmService = value;
                OnPropertyChanged();
            }
        }
        public bool FloatService
        {
            get
            {
                return _floatService;
            }
            set
            {
                _floatService = value;
                OnPropertyChanged();
            }
        }

        private int _csmListCount = HomeProperties.Default.CsmList != null ? HomeProperties.Default.CsmList.Count : 0;
        private int _floatListCount = HomeProperties.Default.FloatList != null ? HomeProperties.Default.FloatList.Count : 0;
        public int CsmListCount
        {
            get
            {
                return _csmListCount;
            }
            set
            {
                _csmListCount = value;
                OnPropertyChanged();
            }
        }
        public int FloatListCount
        {
            get
            {
                return _floatListCount;
            }
            set
            {
                _floatListCount = value;
                OnPropertyChanged();
            }
        }

        private int _checkPush = 0;
        private int _checkCsm = 0;
        private int _checkFloat = 0;
        private int _push = 0;
        private int _cancel = 0;
        private int _successfulTrades = 0;
        private int _purchasesMade = 0;
        public int CheckPush
        {
            get
            {
                return _checkPush;
            }
            set
            {
                _checkPush = value;
                OnPropertyChanged();
            }
        }
        public int CheckCsm
        {
            get
            {
                return _checkCsm;
            }
            set
            {
                _checkCsm = value;
                OnPropertyChanged();
            }
        }
        public int CheckFloat
        {
            get
            {
                return _checkFloat;
            }
            set
            {
                _checkFloat = value;
                OnPropertyChanged();
            }
        }
        public int Push
        {
            get
            {
                return _push;
            }
            set
            {
                _push = value;
                OnPropertyChanged();
            }
        }
        public int Cancel
        {
            get
            {
                return _cancel;
            }
            set
            {
                _cancel = value;
                OnPropertyChanged();
            }
        }
        public int SuccessfulTrades
        {
            get
            {
                return _successfulTrades;
            }
            set
            {
                _successfulTrades = value;
                OnPropertyChanged();
            }
        }
        public int PurchasesMade
        {
            get
            {
                return _purchasesMade;
            }
            set
            {
                _purchasesMade = value;
                OnPropertyChanged();
            }
        }

        private int _progressPush = 0;
        private int _maxProgressPush = 0;
        private string _timerPush = "Off";
        private int _progressCsm = 0;
        private int _maxProgressCsm = 0;
        private string _timerCsm = "Off";
        private int _progressFloat = 0;
        private int _maxProgressFloat = 0;
        private string _timerFloat = "Off";
        public int ProgressPush
        {
            get { return _progressPush; }
            set
            {
                _progressPush = value;
                OnPropertyChanged();
            }
        }
        public int MaxProgressPush
        {
            get { return _maxProgressPush; }
            set
            {
                _maxProgressPush = value;
                OnPropertyChanged();
            }
        }
        public string TimerPush
        {
            get { return _timerPush; }
            set
            {
                _timerPush = value;
                OnPropertyChanged();
            }
        }
        public int ProgressCsm
        {
            get { return _progressCsm; }
            set
            {
                _progressCsm = value;
                OnPropertyChanged();
            }
        }
        public int MaxProgressCsm
        {
            get { return _maxProgressCsm; }
            set
            {
                _maxProgressCsm = value;
                OnPropertyChanged();
            }
        }
        public string TimerCsm
        {
            get { return _timerCsm; }
            set
            {
                _timerCsm = value;
                OnPropertyChanged();
            }
        }
        public int ProgressFloat
        {
            get { return _progressFloat; }
            set
            {
                _progressFloat = value;
                OnPropertyChanged();
            }
        }
        public int MaxProgressFloat
        {
            get { return _maxProgressFloat; }
            set
            {
                _maxProgressFloat = value;
                OnPropertyChanged();
            }
        }
        public string TimerFloat
        {
            get { return _timerFloat; }
            set
            {
                _timerFloat = value;
                OnPropertyChanged();
            }
        }

        //tools
        private string _tradeButton = "Play";
        private string _sellButton = "Play";
        private string _withdrawButton = "Play";
        public string TradeButton
        {
            get { return _tradeButton; }
            set
            {
                _tradeButton = value;
                OnPropertyChanged();
            }
        }
        public string SellButton
        {
            get { return _sellButton; }
            set
            {
                _sellButton = value;
                OnPropertyChanged();
            }
        }
        public string WithdrawButton
        {
            get { return _withdrawButton; }
            set
            {
                _withdrawButton = value;
                OnPropertyChanged();
            }
        }

        private bool _tradeTool;
        private bool _sellTool;
        private bool _withdrawTool;
        public bool TradeTool
        {
            get
            {
                return _tradeTool;
            }
            set
            {
                _tradeTool = value;
                OnPropertyChanged();
            }
        }
        public bool SellTool
        {
            get
            {
                return _sellTool;
            }
            set
            {
                _sellTool = value;
                OnPropertyChanged();
            }
        }
        public bool WithdrawTool
        {
            get
            {
                return _withdrawTool;
            }
            set
            {
                _withdrawTool = value;
                OnPropertyChanged();
            }
        }

        private int _trades = 0;
        private int _sellItems = 0;
        private decimal _sum = 0;
        private int _withdrawItems = 0;
        public int Trades
        {
            get
            {
                return _trades;
            }
            set
            {
                _trades = value;
                OnPropertyChanged();
            }
        }
        public int SellItems
        {
            get
            {
                return _sellItems;
            }
            set
            {
                _sellItems = value;
                OnPropertyChanged();
            }
        }
        public decimal Sum
        {
            get
            {
                return _sum;
            }
            set
            {
                _sum = value;
                OnPropertyChanged();
            }
        }
        public int WithdrawItems
        {
            get
            {
                return _withdrawItems;
            }
            set
            {
                _withdrawItems = value;
                OnPropertyChanged();
            }
        }

        private int _progressTrade = 0;
        private int _maxProgressTrade = 0;
        private int _progressSell = 0;
        private int _maxProgressSell = 0;
        private int _progressWithdraw = 0;
        private int _maxProgressWithdraw = 0;
        public int ProgressTrade
        {
            get { return _progressTrade; }
            set
            {
                _progressTrade = value;
                OnPropertyChanged();
            }
        }
        public int MaxProgressTrade
        {
            get { return _maxProgressTrade; }
            set
            {
                _maxProgressTrade = value;
                OnPropertyChanged();
            }
        }
        public int ProgressSell
        {
            get { return _progressSell; }
            set
            {
                _progressSell = value;
                OnPropertyChanged();
            }
        }
        public int MaxProgressSell
        {
            get { return _maxProgressSell; }
            set
            {
                _maxProgressSell = value;
                OnPropertyChanged();
            }
        }
        public int ProgressWithdraw
        {
            get { return _progressWithdraw; }
            set
            {
                _progressWithdraw = value;
                OnPropertyChanged();
            }
        }
        public int MaxProgressWithdraw
        {
            get { return _maxProgressWithdraw; }
            set
            {
                _maxProgressWithdraw = value;
                OnPropertyChanged();
            }
        }
    }
}
