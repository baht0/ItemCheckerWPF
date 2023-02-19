using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    public class WhatsNewViewModel : ObservableObject
    {
        List<ProjectUpdates> Updates { get; set; } = new();

        public List<string> Versions
        {
            get
            {
                return _versions;
            }
            set
            {
                _versions = value;
                OnPropertyChanged();
            }
        }
        List<string> _versions = new();
        public string SelectedVersion
        {
            get
            {
                return _selectedVersion;
            }
            set
            {
                _selectedVersion = value;
                OnPropertyChanged();
            }
        }
        string _selectedVersion;
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }
        string _text = "Loading...";
        public DateTime Released
        {
            get
            {
                return _released;
            }
            set
            {
                _released = value;
                OnPropertyChanged();
            }
        }
        DateTime _released;

        public WhatsNewViewModel() => Task.Run(GetUpdateNotes);
        void GetUpdateNotes()
        {
            JArray json = JArray.Parse(DropboxRequest.Get.Read("Updates.json"));
            foreach (JObject update in json)
            {
                Updates.Add(new()
                {
                    Version = (string)update["version"],
                    Text = (string)update["text"],
                    Date = (DateTime)update["date"],
                });
                Versions.Add((string)update["version"]);
            }
            Versions.Reverse();
            SelectedVersion = Versions.FirstOrDefault();
        }
        public ICommand ShowCommand =>
            new RelayCommand((obj) =>
            {
                var info = Updates.FirstOrDefault(x => x.Version == (string)obj);
                Released = info.Date;
                Text = info.Text;
            });
    }
}
