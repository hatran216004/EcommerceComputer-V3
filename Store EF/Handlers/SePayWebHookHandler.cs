using Microsoft.AspNet.WebHooks;
using Newtonsoft.Json;
using Store_EF.Models;
using Store_EF.Models.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
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
                                curr.TransactionId = sePay.ReferenceCode;
                                curr.PaymentDate = sePay.TransactionDate;
                                curr.Amount = sePay.TransferAmount;
                                if (curr.Amount == curr.Order.TotalPrice())
                                    curr.Status = "Succeeded";
                                else
                                    curr.Status = "Refunding";
                                try
                                {
                                    store.SaveChanges();
                                    context.Response = context.Request.CreateResponse(HttpStatusCode.OK);
                                    return Task.FromResult(true);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex);
                                    context.Response = context.Request.CreateResponse(HttpStatusCode.BadRequest);
                                    return Task.FromResult(true);
                                }
                            }
                        }
                    }
                    break;
            }
            context.Response = context.Request.CreateResponse(HttpStatusCode.NotFound);
            return Task.FromResult(true);
        }
    }
}