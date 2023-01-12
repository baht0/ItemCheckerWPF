using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    public class ImportParserViewModel : ObservableObject
    {
        ImportParser _importParser = new();
        public ImportParser ImportParser
        {
            get
            {
                return _importParser;
            }
            set
            {
                _importParser = value;
                OnPropertyChanged();
            }
        }

        public ImportParserViewModel()
        {
            Task.Run(() =>
            {
                string folder = ProjectInfo.DocumentPath + "extract";
                if (Directory.Exists(folder))
                {
                    List<ImportFile> files = new();
                    foreach (var path in Directory.GetFiles(folder))
                    {
                        var json = JObject.Parse(File.ReadAllText(path));
                        ImportFile file = json["ParserCheckConfig"].ToObject<ImportFile>();
                        file.Size = (int)json["Size"];
                        file.Path = path;
                        files.Add(file);
                    }
                    ImportParser.List = new(files.OrderByDescending(d => d.CheckedTime));
                }
                ImportParser.IsBusy = false;
            });
        }
        public ICommand ClearCommand =>
            new RelayCommand((obj) =>
            {
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to delete all files?",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                    return;

                string folder = ProjectInfo.DocumentPath + "extract";
                Directory.Delete(folder, true);
                ImportParser = new();

            }, (obj) => ImportParser.List.Any());
        public ICommand DeleteCommand =>
            new RelayCommand((obj) =>
            {
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to delete?",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                    return;

                var file = obj as ImportFile;
                File.Delete(file.Path);
                ImportParser.List.Remove(file);

            }, (obj) => ImportParser.List.Any());
    }
}
