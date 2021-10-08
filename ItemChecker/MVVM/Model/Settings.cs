using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ItemChecker.MVVM.Model
{
    public class Settings
    {
        //general
        public string CurrencyApi { get; set; }
        public ObservableCollection<string> CurrencyList { get; set; }
        public int Currency { get; set; }
        public bool ExitChrome { get; set; }
        public bool Guard { get; set; }
        public bool NotEnoughBalance { get; set; }
        public int CancelOrder { get; set; }
        //float
        public decimal FactoryNew { get; set; }
        public decimal MinimalWear { get; set; }
        public decimal FieldTested { get; set; }
        public decimal WellWorn { get; set; }
        public decimal BattleScarred { get; set; }
        //profile
        public string CurrentProfile { get; set; }
    }
}
