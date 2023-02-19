namespace ItemChecker.MVVM.Model
{
    public class ImportParser : BaseTable<ImportFile>
    {
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }
        bool _isBusy = true;
        public bool IsInfoShow
        {
            get
            {
                return _isInfoShow;
            }
            set
            {
                _isInfoShow = value;
                OnPropertyChanged();
            }
        }
        bool _isInfoShow;
    }
    public class ImportFile : ParserConfig
    {
        public string Path { get; set; }
        public int Size { get; set; }
    }
}
