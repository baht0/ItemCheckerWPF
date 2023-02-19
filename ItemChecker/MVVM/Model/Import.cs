namespace ItemChecker.MVVM.Model
{
    public class ImportParser : BaseTable<ImportFile>
    {
        public ImportFile SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                IsInfoShow = true;
                OnPropertyChanged();
            }
        }
        ImportFile _selectedItem;
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
