using Microsoft.AspNet.WebHooks;
using Newtonsoft.Json;
using Store_EF.Models;
using System.Threading.Tasks;

namespace Store_EF.Handlers
{
    public class SePayWebHookHandler : WebHookHandler
    {
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
                        if (sePay.ReferenceCode != null)
                        {

                        }
                    }
                    break;
            }
            return Task.FromResult(true);
        }
    }
}