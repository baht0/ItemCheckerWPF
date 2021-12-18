using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using OpenQA.Selenium.Support.Extensions;
using System.Linq;

namespace ItemChecker.Services
{
    public class OrderService : BaseService
    {

        //delete order
        public void CancelOrder(DataOrder order)
        {
            Browser.ExecuteJavaScript(Post.CancelBuyOrder(order.OrderId, Account.SessionId));
            DataOrder.Orders.Remove(order);

            Account.GetAvailableAmount();
        }
        protected void CheckConditions(DataOrder order, decimal orderPrice)
        {
            if (GeneralProperties.Default.NotEnoughBalance & Account.Balance < orderPrice)
            {
                CancelOrder(order);
                Home.Cancel++;
            }
            if (GeneralProperties.Default.CancelOrder > order.Precent & order.Precent != -100)
            {
                CancelOrder(order);
                Home.Cancel++;
            }
            if (ItemBase.Overstock.Any(x => x.ItemName == order.ItemName) | ItemBase.Unavailable.Any(x => x.ItemName == order.ItemName))
            {
                CancelOrder(order);
                Home.Cancel++;
            }
        }
    }
}
