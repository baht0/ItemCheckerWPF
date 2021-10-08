using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ItemChecker.MVVM.Model
{
    public class Order : Main
    {
        public OrderStatistic Statistic { get; set; }
        //push
        public int PushTimer { get; set; }

        //Favorite
        public int FavoriteTimer { get; set; }
        public decimal MaxDeviation { get; set; }

        //float
        public int FloatTimer { get; set; }
        public int Compare { get; set; }

        public ObservableCollection<string> ComparePrices { get; set; }
        public decimal MaxPrecent { get; set; }

        public int MaxPrice { get; set; }
    }
}