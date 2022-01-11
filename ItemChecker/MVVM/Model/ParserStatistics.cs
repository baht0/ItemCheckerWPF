namespace ItemChecker.MVVM.Model
{
    public class ParserStatistics : BaseModel
    {
        private string _price1 = "Price1";
        private string _price2 = "Price2";
        private string _price3 = "Price3";
        private string _price4 = "Price4";
        public string Price1
        {
            get
            {
                return _price1;
            }
            set
            {
                _price1 = value;
                OnPropertyChanged();
            }
        }
        public string Price2
        {
            get
            {
                return _price2;
            }
            set
            {
                _price2 = value;
                OnPropertyChanged();
            }
        }
        public string Price3
        {
            get
            {
                return _price3;
            }
            set
            {
                _price3 = value;
                OnPropertyChanged();
            }
        }
        public string Price4
        {
            get
            {
                return _price4;
            }
            set
            {
                _price4 = value;
                OnPropertyChanged();
            }
        }

        private string _mode = "Unknown";
        private string _service1 = "Service1";
        private string _service2 = "Service2";
        private string _currency = "Unknown";
        public string Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
                OnPropertyChanged();
            }
        }
        public string Service1
        {
            get
            {
                return _service1;
            }
            set
            {
                _service1 = value;
                OnPropertyChanged();
            }
        }
        public string Service2
        {
            get
            {
                return _service2;
            }
            set
            {
                _service2 = value;
                OnPropertyChanged();
            }
        }
        public string Currency
        {
            get
            {
                return _currency;
            }
            set
            {
                _currency = value;
                OnPropertyChanged();
            }
        }

        private int _countList = 0;
        private int _currentProgress;
        private int _maxProgress;
        private string _timer;
        private bool _timerOn;
        public int CountList
        {
            get { return _countList; }
            set
            {
                _countList = value;
                OnPropertyChanged();
            }
        }
        public int CurrentProgress
        {
            get { return _currentProgress; }
            set
            {
                _currentProgress = value;
                OnPropertyChanged();
            }
        }
        public int MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged();
            }
        }
        public string Timer
        {
            get { return _timer; }
            set
            {
                _timer = value;
                OnPropertyChanged();
            }
        }
        public bool TimerOn
        {
            get { return _timerOn; }
            set
            {
                _timerOn = value;
                OnPropertyChanged();
            }
        }

        public static string DataCurrency { get; set; }
    }
}
