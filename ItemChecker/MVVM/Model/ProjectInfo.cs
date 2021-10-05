namespace ItemChecker.MVVM.Model
{
    public class ProjectInfo
    {
        public string Version { get; set; }
        public string ItemCheckerNet { get; set; }
        public string ItemCheckerSupport { get; set; }
        public string NewtonsoftJson { get; set; }
        public string WebDriver { get; set; }
        public string WebDriverSupport { get; set; }
        public string MaterialDesignColors { get; set; }
        public string MaterialDesignThemesWpf { get; set; }
        public string ChromeDriver { get; set; }

        public static ProjectInfo CurrentVersion { get; set; }
        public static ProjectInfo LatestVersion { get; set; }
        public static bool IsUpdate { get; set; } = false;

        public ProjectInfo(string Version, string ItemCheckerNet, string ItemCheckerSupport, string NewtonsoftJson, string WebDriver, string WebDriverSupport, string MaterialDesignColors, string MaterialDesignThemesWpf, string ChromeDriver)
        {
            this.Version = Version;
            this.ItemCheckerNet = ItemCheckerNet;
            this.ItemCheckerSupport = ItemCheckerSupport;
            this.NewtonsoftJson = NewtonsoftJson;
            this.WebDriver = WebDriver;
            this.WebDriverSupport = WebDriverSupport;
            this.MaterialDesignColors = MaterialDesignColors;
            this.MaterialDesignThemesWpf = MaterialDesignThemesWpf;
            this.ChromeDriver = ChromeDriver;
        }
    }
}
