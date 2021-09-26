using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Properties;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    public class ParserViewModel : MainViewModel
    {
        private int _itemCount;
        private Parser _parserConfig;
        private List<ParserData> _parserGrid;
        public int ItemCount
        {
            get
            {
                return _itemCount;
            }
            set
            {
                _itemCount = value;
                OnPropertyChanged();
            }
        }
        public Parser ParserConfig
        {
            get
            {
                return _parserConfig;
            }
            set
            {
                _parserConfig = value;
                OnPropertyChanged();
            }
        }
        public List<ParserData> ParserGrid
        {
            get
            {
                return _parserGrid;
            }
            set
            {
                _parserGrid = value;
                OnPropertyChanged();
            }
        }
        public ParserViewModel()
        {
            ParserConfig = new Parser()
            {
                Services = new List<string>()
                {
                    "SteamMarket",
                    "Cs.Money",
                    "Loot.Farm"
                },
                ServiceOne = ParserProperties.Default.serviceOne,
                ServiceTwo = ParserProperties.Default.serviceTwo,

                MaxPrice = ParserProperties.Default.maxPrice,
                MinPrice = ParserProperties.Default.minPrice,
                MaxPrecent = ParserProperties.Default.maxPrecent,
                MinPrecent = ParserProperties.Default.minPrecent,
                SteamSales = ParserProperties.Default.steamSales,
                NameContains = ParserProperties.Default.nameContains,
                Knife = ParserProperties.Default.knife,
                Stattrak = ParserProperties.Default.stattrak,
                Souvenir = ParserProperties.Default.souvenir,
                Sticker = ParserProperties.Default.sticker
            };
            Parser.getList();
            ParserGrid = ParserData.ParserItems;
            ItemCount = ParserData.ParserItems.Count;
        }
        public ICommand CheckCommand
        {
            get
            {
                return new RelayCommand((obj) =>
                {
                    Task.Run(() =>
                    {
                        Parser d = (Parser)obj;
                    });
                });
            }
        }
    }
}
