
namespace ItemChecker.MVVM.Model
{
    public class ParserManualService : ParserService
    {
        public void Manual(string itemName)
        {
            ParserData response = CheckItems(itemName);
            ParserData.ParserItems.Add(response);
        }
    }
}
