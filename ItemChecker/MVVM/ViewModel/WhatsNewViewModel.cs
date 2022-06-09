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
        private List<string> _versions = new();
        private string _selectedVersion;
        private string _text = "Loading...";
        private DateTime _released;

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

        public WhatsNewViewModel()
        {
            Task.Run(() => GetUpdateNotes());
        }
        void GetUpdateNotes()
        {
            JArray json = JArray.Parse(Get.DropboxRead("Updates.json"));
            foreach (JObject update in json)
            {
                ProjectUpdates.Updates.Add(new()
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
                var info = ProjectUpdates.Updates.FirstOrDefault(x => x.Version == (string)obj);
                Released = info.Date;
                Text = info.Text;
            });
    }
}
