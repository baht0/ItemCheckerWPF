using ItemChecker.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace ItemChecker.MVVM.Model
{
    public class ParserConfig : BaseModel
    {
        public List<string> CheckList { get; set; } = ParserService.ReadList("CheckList");
        public int MinPrice { get; set; } = ParserProperties.Default.MinPrice;
        public int MaxPrice { get; set; } = ParserProperties.Default.MaxPrice;
        public bool Normal { get; set; } = ParserProperties.Default.Normal;
        public bool Souvenir { get; set; } = ParserProperties.Default.Souvenir;
        public bool Stattrak { get; set; } = ParserProperties.Default.Stattrak;
        public bool KnifeGlove { get; set; } = ParserProperties.Default.KnifeGlove;
        public bool KnifeGloveStattrak { get; set; } = ParserProperties.Default.KnifeGloveStattrak;
        public bool Overstock { get; set; } = ParserProperties.Default.Overstock;
        public bool Ordered { get; set; } = ParserProperties.Default.Ordered;
        public bool WithoutLock { get; set; } = ParserProperties.Default.WithoutLock;
        public bool RareItems { get; set; } = ParserProperties.Default.RareItems;
        public bool UserItems { get; set; } = ParserProperties.Default.UserItems;
        public bool OnlyDopplers { get; set; } = ParserProperties.Default.OnlyDopplers;

        public ObservableCollection<string> Services { get; set; } = new()
        {
            "SteamMarket(A)",
            "SteamMarket",
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
