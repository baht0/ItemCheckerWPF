using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Services;
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
        private List<string> _listUpdates = new();
        private string _text;
        private string _currentVersion = DataProjectInfo.CurrentVersion;

        public List<string> ListUpdates
        {
            get
            {
                return _listUpdates;
            }
            set
            {
                _listUpdates = value;
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
        public string CurrentVersion
        {
            get
            {
                return _currentVersion;
            }
            set
            {
                _currentVersion = value;
                OnPropertyChanged();
            }
        }

        public WhatsNewViewModel()
        {
            Task.Run(() => GetUpdates());
        }
        void GetUpdates()
        {
            string json = Post.DropboxListFolder("Updates");
            JArray updates = JArray.Parse(JObject.Parse(json)["entries"].ToString());

            foreach (JObject update in updates)
            {
                if (((string)update["name"]).Contains("Version"))
                    continue;
                string ver = (string)update["name"];
                ver = ver.Replace("update_", string.Empty);
                ver = ver.Replace(".txt", string.Empty);
                ListUpdates.Add(ver);
            }
            ListUpdates = ListUpdates.OrderByDescending(x => x).ToList();
        }
        public ICommand ShowCommand =>
            new RelayCommand((obj) =>
            {
                try
                {
                    Text = "Loading...";
                    Task.Run(() => {
                        Text = Post.DropboxRead($"Updates/update_{(string)obj}.txt");
                    });
                }
                catch (Exception ex)
                {
                    BaseService.errorLog(ex);
                    BaseService.errorMessage(ex);
                }
            });
    }
}
