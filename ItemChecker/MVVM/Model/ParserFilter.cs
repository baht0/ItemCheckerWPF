namespace ItemChecker.MVVM.Model
{
    public class ParserFilter
    {
        public static ParserFilter FilterConfig { get; set; }

        //category
        public bool Normal { get; set; }
        public bool Stattrak { get; set; }
        public bool Souvenir { get; set; }
        public bool KnifeGlove { get; set; }
        public bool KnifeGloveStattrak { get; set; }
        //status
        public bool Tradable { get; set; } = true;
        public bool Ordered { get; set; }
        public bool Overstock { get; set; }
        public bool Unavailable { get; set; }
        //exterior
        public bool NotPainted { get; set; }
        public bool BattleScarred { get; set; }
        public bool WellWorn { get; set; }
        public bool FieldTested { get; set; }
        public bool MinimalWear { get; set; }
        public bool FactoryNew { get; set; }
        //type
        public bool Weapon { get; set; }
        public bool Knife { get; set; }
        public bool Gloves { get; set; }
        public bool Sticker { get; set; }
        public bool Agent { get; set; }
        public bool Capsule { get; set; }
        public bool Patch { get; set; }
        public bool Collectible { get; set; }
        public bool Key { get; set; }
        public bool Pass { get; set; }
        public bool MusicKit { get; set; }
        public bool Graffiti { get; set; }
        public bool Case { get; set; }
        public bool Package { get; set; }
        public bool PatchPack { get; set; }
        //Quality
        public bool Industrial { get; set; }
        public bool MilSpec { get; set; }
        public bool Restricted { get; set; }
        public bool Classified { get; set; }
        public bool Covert { get; set; }
        public bool Contraband { get; set; }
        //price
        public bool Price1 { get; set; }
        public bool Price2 { get; set; }
        public bool Price3 { get; set; }
        public bool Price4 { get; set; }
        public decimal Price1From { get; set; }
        public decimal Price1To { get; set; }
        public decimal Price2From { get; set; }
        public decimal Price2To { get; set; }
        public decimal Price3From { get; set; }
        public decimal Price3To { get; set; }
        public decimal Price4From { get; set; }
        public decimal Price4To { get; set; }
        //other
        public decimal PrecentFrom { get; set; }
        public decimal PrecentTo { get; set; }
        public decimal DifferenceFrom { get; set; }
        public decimal DifferenceTo { get; set; }
        public bool Hide100 { get; set; } = true;
        public bool Hide0 { get; set; } = true;
        public bool Have { get; set; }
    }
}
