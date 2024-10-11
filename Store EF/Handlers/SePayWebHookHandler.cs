using Microsoft.AspNet.WebHooks;
using Newtonsoft.Json;
using Store_EF.Models;
using Store_EF.Models.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace Store_EF.Handlers
{
    public class SePayWebHookHandler : WebHookHandler
    {
        StoreEntities store = new StoreEntities();

        public SePayWebHookHandler()
        {
            Receiver = GenericJsonWebHookReceiver.ReceiverName;
        }

        public override Task ExecuteAsync(string receiver, WebHookHandlerContext context)
        {
            switch (context.Id.ToLower())
            {
                case "sepay":
                    {
                        SePayWebHook sePay = JsonConvert.DeserializeObject<SePayWebHook>(context.Data.ToString());
                        if (sePay.Code != null)
                        {
                            Payment curr = store.Payments.FirstOrDefault(x => x.Code == sePay.Code);
                            if (curr != null)
                            {
                                Order_ order = curr.Order_;
                                curr.TransactionId = sePay.ReferenceCode;
                                curr.PaymentDate = sePay.TransactionDate;
                                curr.Amount = order.TotalPrice();
                                if (curr.Amount == order.TotalPrice())
                                    curr.Status = "Succeeded";
                                else
                                    curr.Status = "Failed";
                                try
                                {
                                    store.SaveChanges();
                                    return Task.FromResult(true);
                                }
                                catch
                                {
                                    return Task.FromResult(false);
                                }
                            }
                        }
                    }
                    break;
            }
            return Task.FromResult(false);
        }
    }
}