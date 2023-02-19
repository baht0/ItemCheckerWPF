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
    public class ShowListViewModel : ObservableObject
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
        public SavedItems SavedItems
        {
            get { return _savedItems; }
            set
            {
                _savedItems = value;
                OnPropertyChanged();
            }
        }
        SavedItems _savedItems = new();

        public ShowListViewModel()
        {
            Timer.Elapsed += UpdateWindow;
            Timer.Enabled = true;
        }
        void UpdateWindow(Object sender, ElapsedEventArgs e)
        {
            if (SavedItems.ListName != SavedItems.ShowListName)
            {
                SavedItems.ListName = SavedItems.ShowListName;
                switch (SavedItems.ListName)
                {
                    case "Reserve":
                        SavedItems.Items = new(SavedItems.Reserve);
                        SavedItems.Services = BaseModel.Services;
                        break;
                    case "Rare":
                        SavedItems.Items = new(SavedItems.Rare);
                        SavedItems.Services = BaseModel.Parameters;
                        break;
                }
                SavedItems.ServiceId = 0;
            }
        }

        public ICommand RemoveCommand =>
            new RelayCommand((obj) =>
            {
                var item = obj as DataItem;
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to remove item?\n{item.ItemName}",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    switch (SavedItems.ListName)
                    {
                        case "Reserve":
                            SavedItems.Reserve.Remove(item);
                            SavedItems.Items = new(SavedItems.Reserve);
                            break;
                        case "Rare":
                            SavedItems.Rare.Remove(item);
                            SavedItems.Items = new(SavedItems.Rare);
                            break;
                    }
                    Message.Enqueue($"{item.ItemName}\nItem has been removed.");
                }
            }, (obj) => SavedItems.SelectedItem != null);
        public ICommand AddCommand =>
            new RelayCommand((obj) =>
            {
                DataItem item = new(SavedItems.ItemName, SavedItems.ServiceId);
                string message = string.Empty;
                switch (SavedItems.ListName)
                {
                    case "Reserve":
                        bool isAdded = SavedItems.Reserve.Add(item);
                        message = isAdded ? $"{item.ItemName}\nItem has been added." : "Not successful. Conditions not met.";
                        SavedItems.Items = new(SavedItems.Reserve);
                        SavedItems.ItemName = isAdded ? string.Empty : SavedItems.ItemName;
                        break;
                    case "Rare":
                        isAdded = SavedItems.Rare.Add(item);
                        message = isAdded ? $"{item.ItemName}\nItem has been added." : "Not successful. Conditions not met.";
                        SavedItems.ItemName = isAdded ? string.Empty : SavedItems.ItemName;
                        SavedItems.Items = new(SavedItems.Rare);
                        break;
                }
                Message.Enqueue(message);
            });
        public ICommand ClearCommand =>
            new RelayCommand((obj) =>
            {
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to clear the list?",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                    return;

                switch (SavedItems.ListName)
                {
                    case "Reserve":
                        SavedItems.Reserve.Clear();
                        SavedItems.Items = new(SavedItems.Reserve);
                        break;
                    case "Rare":
                        SavedItems.Rare.Clear();
                        SavedItems.Items = new(SavedItems.Rare);
                        break;
                }
                Message.Enqueue("The list has been cleared.");
            }, (obj) => SavedItems.Items.Any());
    }
}
