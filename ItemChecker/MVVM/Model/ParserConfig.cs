using ItemChecker.Properties;
using System.Collections.ObjectModel;
using System.Threading;

namespace ItemChecker.MVVM.Model
{
    public class ParserConfig : BaseModel
    {
        public bool Souvenir { get; set; } = ParserProperties.Default.Souvenir;
        public bool Stattrak { get; set; } = ParserProperties.Default.Stattrak;
        public bool KnifeGlove { get; set; } = ParserProperties.Default.KnifeGlove;
        public bool KnifeGloveStattrak { get; set; } = ParserProperties.Default.KnifeGloveStattrak;
        public bool Overstock { get; set; } = ParserProperties.Default.Overstock;
        public bool Ordered { get; set; } = ParserProperties.Default.Ordered;
        public bool Dopplers { get; set; } = ParserProperties.Default.Dopplers;
        public bool OnlyDopplers { get; set; } = ParserProperties.Default.OnlyDopplers;
        public decimal MinPrice { get; set; } = ParserProperties.Default.MinPrice;
        public decimal MaxPrice { get; set; } = ParserProperties.Default.MaxPrice;

        public ObservableCollection<string> Services { get; set; } = new()
        {
            "SteamMarket",
            "SteamMarket(A)",
            "Cs.Money",
            "Loot.Farm"
        };
        public int ServiceOne { get; set; } = ParserProperties.Default.ServiceOne;
        public int ServiceTwo { get; set; } = ParserProperties.Default.ServiceTwo;

        public static System.Timers.Timer Timer { get; set; } = new(1000);
        public static int TimerTick { get; set; }
        public static CancellationTokenSource cts { get; set; } = new();
        public static CancellationToken token { get; set; } = cts.Token;
    }
}
