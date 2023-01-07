using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    public class ShowListViewModel : ObservableObject
    {
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
        private SavedItems _savedItems = new();

        public ShowListViewModel(string listName)
        {
            switch (listName)
            {
                case "Reserve":
                    SavedItems.IsReserve = true;
                    SavedItems.List = new(SavedItems.Reserve);
                    break;
                case "Rare":
                    SavedItems.IsRare = true;
                    SavedItems.List = new(SavedItems.Rare);
                    break;
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
                    if (SavedItems.IsReserve)
                    {
                        SavedItems.Reserve.Remove(item);
                        SavedItems.List = new(SavedItems.Reserve);
                    }
                    else if (SavedItems.IsRare)
                    {
                        SavedItems.Rare.Remove(item);
                        SavedItems.List = new(SavedItems.Rare);
                    }
                    Message.Enqueue($"{item.ItemName}\nItem has been removed.");
                }
            }, (obj) => SavedItems.SelectedItem != null);
        public ICommand AddCommand =>
            new RelayCommand((obj) =>
            {
                var name = obj as string;

                if (String.IsNullOrEmpty(name))
                    return;

                DataItem item = new(name, SavedItems.ServiceId);
                string message = string.Empty;
                if (SavedItems.IsReserve)
                {
                    message = SavedItems.Reserve.Add(item) ? $"{item.ItemName}\nItem has been added." : "Not successful. Conditions not met.";
                    SavedItems.List = new(SavedItems.Reserve);
                }
                else if (SavedItems.IsRare)
                {
                    message = SavedItems.Rare.Add(item) ? $"{item.ItemName}\nItem has been added." : "Not successful. Conditions not met.";
                    SavedItems.List = new(SavedItems.Rare);
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

                if (SavedItems.IsReserve)
                {
                    SavedItems.Reserve.Clear();
                    SavedItems.List = new(SavedItems.Reserve);
                }
                else if (SavedItems.IsRare)
                {
                    SavedItems.Rare.Clear();
                    SavedItems.List = new(SavedItems.Rare);
                }
                Message.Enqueue("The list has been cleared.");
            });
    }
}
