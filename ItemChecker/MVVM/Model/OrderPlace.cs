using System.Collections.ObjectModel;

namespace ItemChecker.MVVM.Model
{
    public class OrderPlace
    {
        public static ObservableCollection<string> Queue = new();
        public static decimal AmountRub { get; set; } = 0;
    }
}