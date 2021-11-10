using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using OpenQA.Selenium.Support.Extensions;

namespace ItemChecker.Services
{
    public class OrderService : BaseService
    {

        //delete order
        public void CancelOrder(OrderData order)
        {
            Browser.ExecuteJavaScript(Post.CancelBuyOrder(order.OrderId, Account.SessionId));
            Account.MyOrders.Remove(order);

            Account.GetAvailableAmount();
        }
        protected void CheckConditions(OrderData order, decimal orderPrice)
        {
            if (GeneralProperties.Default.NotEnoughBalance & Account.Balance < orderPrice)
            {
                CancelOrder(order);
                OrderStatistic.Cancel++;
            }
            if (GeneralProperties.Default.CancelOrder > order.Precent & order.Precent != -100)
            {
                CancelOrder(order);
                OrderStatistic.Cancel++;
            }
            if (Main.Overstock.Contains(order.ItemName) | Main.Unavailable.Contains(order.ItemName))
            {
                CancelOrder(order);
                OrderStatistic.Cancel++;
            }
        }
    }
}
