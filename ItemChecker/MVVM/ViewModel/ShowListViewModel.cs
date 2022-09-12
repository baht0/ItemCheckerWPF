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
        SnackbarMessageQueue _message = new();
        public SnackbarMessageQueue Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        private ItemsList _itemsList = new();
        public ItemsList ItemsList
        {
            get { return _itemsList; }
            set
            {
                _itemsList = value;
                OnPropertyChanged();
            }
        }

        public ShowListViewModel(string listName)
        {
            switch (listName)
            {
                case "Favorite":
                    ItemsList.IsFavorite = true;
                    ItemsList.List = new(ItemsList.Favorite);
                    break;
                case "Rare":
                    ItemsList.IsRare = true;
                    ItemsList.List = new(ItemsList.Rare);
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
                    if (ItemsList.IsFavorite)
                    {
                        ItemsList.Favorite.Remove(item);
                        ItemsList.List = new(ItemsList.Favorite);
                    }
                    else if (ItemsList.IsRare)
                    {
                        ItemsList.Rare.Remove(item);
                        ItemsList.List = new(ItemsList.Rare);
                    }
                    Message.Enqueue($"{item.ItemName}\nItem has been removed.");
                }
            }, (obj) => ItemsList.SelectedItem != null);
        public ICommand AddCommand =>
            new RelayCommand((obj) =>
            {
                var name = obj as string;

                if (String.IsNullOrEmpty(name))
                    return;

                DataItem item = new(name, ItemsList.ServiceId);
                string message = string.Empty;
                if (ItemsList.IsFavorite)
                {
                    message = ItemsList.Favorite.Add(item) ? $"{item.ItemName}\nItem has been added." : "Not successful. Conditions not met.";
                    ItemsList.List = new(ItemsList.Favorite);
                }
                else if (ItemsList.IsRare)
                {
                    message = ItemsList.Rare.Add(item) ? $"{item.ItemName}\nItem has been added." : "Not successful. Conditions not met.";
                    ItemsList.List = new(ItemsList.Rare);
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

                if (ItemsList.IsFavorite)
                {
                    ItemsList.Favorite.Clear();
                    ItemsList.List = new(ItemsList.Favorite);
                }
                else if (ItemsList.IsRare)
                {
                    ItemsList.Rare.Clear();
                    ItemsList.List = new(ItemsList.Rare);
                }
                Message.Enqueue("The list has been cleared.");
            });
    }
}
