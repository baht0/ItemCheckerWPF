using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class Settings
    {
        //general
        public string CurrencyApi { get; set; }
        public List<string> CurrencyList { get; set; }
        public int Currency { get; set; }
        public bool ExitChrome { get; set; }
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
