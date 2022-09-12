using ItemChecker.Core;
using System.Collections.ObjectModel;

namespace ItemChecker.MVVM.Model
{
    public class ImportParser : ObservableObject
    {
        public ObservableCollection<ImportFile> List
        {
            get { return _list; }
            set
            {
                _list = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<ImportFile> _list = new();
        public ImportFile Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                _selected = value;
                OnPropertyChanged();
            }
        }
        ImportFile _selected = new();
    }
    public class ImportFile : ParserCheckConfig
    {
        public string Path { get; set; }
        public int Size { get; set; }
    }
}
