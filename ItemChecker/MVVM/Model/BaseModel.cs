using ItemChecker.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace ItemChecker.MVVM.Model
{
    public class BaseModel : ObservableObject
    {
        //loading
        public static bool IsWorking { get; set; }
        public static bool IsBrowser { get; set; }
        //selenium
        public static IWebDriver Browser { get; set; }
        public static WebDriverWait WebDriverWait { get; set; }
    }
}
