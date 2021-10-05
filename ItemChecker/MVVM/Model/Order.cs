using System.Collections.Generic;

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

        public List<string> ComparePrices { get; set; }
        public decimal MaxPrecent { get; set; }

        public int MaxPrice { get; set; }
    }
}