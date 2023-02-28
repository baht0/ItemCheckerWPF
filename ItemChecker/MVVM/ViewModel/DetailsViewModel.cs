using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using MaterialDesignThemes.Wpf;
using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    public class DetailsViewModel : ObservableObject
    {
        readonly Timer Timer = new(100);
        public SnackbarMessageQueue Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }
        SnackbarMessageQueue _message = new();
        public Details Details
        {
            get { return _details; }
            set
            {
                _details = value;
                OnPropertyChanged();
            }
        }
        Details _details = new();
        public DetailsViewModel(bool isMenu)
        {
            Details.IsSearch = isMenu && !Details.Items.Any();

            Timer.Elapsed += UpdateWindow;
            Timer.Enabled = true;

            DataGridDetails.Items = new(Details.Items);
            if (Details.Items.Any())
                DataGridDetails.SelectedItem = Details.Item ?? Details.Items.LastOrDefault();
        }
        void UpdateWindow(Object sender, ElapsedEventArgs e)
        {
            if (DataGridDetails.Items.Count < Details.Items.Count)
            {
                DataGridDetails.Items = new(Details.Items);
                DataGridDetails.SelectedItem = Details.Items.LastOrDefault();
            }
            if (Details.Item != null)
            {
                DataGridDetails.SelectedItem = Details.Item;
                Details.Item = null;
            }
        }
        public ICommand UpdateItemsViewCommand =>
            new RelayCommand((obj) =>
            {
                OnPropertyChanged(nameof(DataGridDetails.SelectedItem));
            });

        public DataGridDetails DataGridDetails
        {
            get
            {
                return _dataGridDetails;
            }
            set
            {
                _dataGridDetails = value;
                OnPropertyChanged();
            }
        }
        DataGridDetails _dataGridDetails = new();
        public ICommand DeleteCommand =>
            new RelayCommand((obj) =>
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete all items?", "Question",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Details.IsSearch = true;
                    Details.Items.Clear();
                    DataGridDetails.Items = new(Details.Items);
                    DataGridDetails.SelectedItem = new(null);
                }
            }, (obj) => !DataGridDetails.Items.Any(x => x.IsBusy) && !DataGridDetails.Items.Any(x => x.Info.IsBusy));
        public ICommand ShowSearchCommand =>
            new RelayCommand((obj) =>
            {
                if (!Details.IsSearch)
                    Details.IsSearch = true;
                else if (DataGridDetails.Items.Count > 0)
                    Details.IsSearch = false;
            });
        public ICommand SearchCommand =>
            new RelayCommand((obj) =>
            {
                var str = obj as string;
                Details.Items.Add(str);
                DataGridDetails.Items = new(Details.Items);
                DataGridDetails.SelectedItem = Details.Items.LastOrDefault();
                Details.IsSearch = false;
            });

        public ICommand ReloadCommand =>
            new RelayCommand((obj) =>
            {
                DataGridDetails.SelectedItem.UpdateServices();
            }, (obj) => !DataGridDetails.Items.Any(x => x.IsBusy) && !DataGridDetails.Items.Any(x => x.Info.IsBusy));
        public ICommand CopyCommand =>
            new RelayCommand((obj) =>
            {
                Clipboard.SetText(DataGridDetails.SelectedItem.ItemName);
                Message.Enqueue("Item name copied.");
            });
        public ICommand SwitchCurrencyCommand =>
            new RelayCommand((obj) =>
            {
                string currName = (string)obj;
                var current = Details.CurrentCurrency;
                Details.CurrentCurrency = DataGridDetails.SwitchCurrency(current, currName);
            }, (obj) => DataGridDetails.SelectedItem != null && DataGridDetails.SelectedItem.Services != null && DataGridDetails.SelectedItem.Services.Any());
        public ICommand OpenItemOutCommand =>
            new RelayCommand((obj) =>
            {
                var item = (DetailService)obj;
                DataGridDetails.ShowItemInService(item.ServiceId);
            });
        public ICommand CompareCommand =>
            new RelayCommand((obj) =>
            {
                DataGridDetails.Compare();
            }, (obj) => DataGridDetails.SelectedItem != null && DataGridDetails.SelectedItem.Services.Any()
                        && DataGridDetails.SelectedItem.ItemName != "Unknown" && DataGridDetails.SelectedItem.Services != null);
    }
}